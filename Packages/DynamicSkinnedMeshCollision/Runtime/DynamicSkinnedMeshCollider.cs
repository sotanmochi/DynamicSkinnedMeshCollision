using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace DynamicSkinnedMeshCollision
{
    public sealed class DynamicSkinnedMeshCollider : MonoBehaviour
    {
        const string ShaderName = "DynamicSkinnedMeshCollision/MeshCompute";
        const string KernelName = "GetVertexPositions";

        [StructLayout(LayoutKind.Sequential)]
        struct VertexData
        {
            public Vector3 Position;
        }

        [SerializeField] bool _autoInitialize = true;

        bool _initialized;
        bool _meshUpdated;

        SkinnedMeshRenderer _skinnedMeshRenderer;
        MeshCollider _meshCollider;
        Mesh _mesh;

        JobHandle _bakeMeshJob;

        NativeArray<VertexData> _vertexDataArray;
        ComputeBuffer _outputVertexBuffer;

        ComputeShader _computeShader;
        int _kernelId;
        int _dispatchCount;
        AsyncGPUReadbackRequest _readbackRequest;

        public SkinnedMeshRenderer SkinnedMeshRenderer => _skinnedMeshRenderer;
        public Mesh Mesh => _mesh;

        public bool AutoInitialize
        {
            get => _autoInitialize;
            set => _autoInitialize = value;
        }

        #region MonoBehaviour Callbacks

        void Start()
        {
            if (!_autoInitialize) return;
            Initialize();
        }

        void Update()
        {
            if (!_initialized) return;
            DispatchCompute();
            UpdateMesh();
        }

        void FixedUpdate()
        {
            UpdateColliderMesh();
        }

        void OnDestroy()
        {
            Release();
        }

        #endregion

        public void Initialize()
        {
            if (_initialized) return;

            if (!SystemInfo.supportsComputeShaders)
            {
                Debug.Log($"<color=orange>[{nameof(DynamicSkinnedMeshCollider)}] ComputeShader is not supported on this device.</color>");
                gameObject.SetActive(false);
                return;
            }

            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                Debug.Log($"<color=orange>[{nameof(DynamicSkinnedMeshCollider)}] AsyncGPUReadback is not supported on this device.</color>");
                gameObject.SetActive(false);
                return;
            }

            if (!TryGetComponent<SkinnedMeshRenderer>(out _skinnedMeshRenderer))
            {
                Debug.Log($"<color=orange>[{nameof(DynamicSkinnedMeshCollider)}] SkinnedMeshRenderer is not found.</color>");
                return;
            }

            // NOTE:
            // Internal_Create is not allowed to be called from a MonoBehaviour constructor (or instance field initializer). 
            _mesh = new Mesh();
            _mesh.name = "ColliderMesh";

            _skinnedMeshRenderer.BakeMesh(_mesh);

            var bonesPerVertex = _skinnedMeshRenderer.sharedMesh.GetBonesPerVertex();
            var boneWeights = _skinnedMeshRenderer.sharedMesh.GetAllBoneWeights();
            _mesh.SetBoneWeights(bonesPerVertex, boneWeights);

            _meshCollider = gameObject.GetOrAddComponent<MeshCollider>();
            _meshCollider.sharedMesh = _mesh;

            var vertexCount = _mesh.vertexCount;

            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, _mesh.GetVertexAttributeFormat(VertexAttribute.Position), 3)
            };
            _mesh.SetVertexBufferParams(vertexCount, layout);

            // Compute Shader
            uint threadGroupSizeX = 0;
            uint threadGroupSizeY = 0;
            uint threadGroupSizeZ = 0;
            _computeShader = ComputeShader.Instantiate(Resources.Load<ComputeShader>(ShaderName));
            _kernelId = _computeShader.FindKernel(KernelName);
            _computeShader.GetKernelThreadGroupSizes(_kernelId, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
            _dispatchCount = Mathf.CeilToInt(vertexCount / (float)threadGroupSizeX);

            // Initialize mesh vertex array
            _vertexDataArray = new NativeArray<VertexData>(vertexCount, Allocator.Temp);
            for (int i = 0; i < vertexCount; i++)
            {
                _vertexDataArray[i] = new VertexData()
                {
                    Position = _mesh.vertices[i]
                };
            }

            // Initialize compute buffer
            _outputVertexBuffer = new ComputeBuffer(vertexCount, 3 * 4); // 3 * 4bytes = sizeof(Vector3)
            if (_vertexDataArray.IsCreated) _outputVertexBuffer.SetData(_vertexDataArray);

            RequestMeshDataReadback();

            _initialized = true;
        }

        void DispatchCompute()
        {
            // Acquire mesh GPU vertex buffer
            var gpuVertexBuffer = _skinnedMeshRenderer.GetVertexBuffer();
            if (gpuVertexBuffer == null)
            {
                return;
            }

            // Execute the compute shader to get vertex positions
            _computeShader.SetMatrix("LocalToWorld", _skinnedMeshRenderer.worldToLocalMatrix * _skinnedMeshRenderer.rootBone.localToWorldMatrix);
            _computeShader.SetFloat("GpuVertexBufferStride", gpuVertexBuffer.stride);
            _computeShader.SetBuffer(_kernelId, "GpuVertexBuffer", gpuVertexBuffer);
            _computeShader.SetBuffer(_kernelId, "OutputVertexBuffer", _outputVertexBuffer);
            _computeShader.Dispatch(_kernelId, _dispatchCount, 1, 1);

            gpuVertexBuffer.Release();
        }

        void UpdateMesh()
        {
            if (_readbackRequest.done && !_readbackRequest.hasError)
            {
                _vertexDataArray = _readbackRequest.GetData<VertexData>();

                _mesh.MarkDynamic();
                _mesh.SetVertexBufferData(_vertexDataArray, 0, 0, _vertexDataArray.Length);
                _meshUpdated = true;

                _bakeMeshJob = new BakeMeshJob(_mesh.GetInstanceID()).Schedule();
            }
        }

        void UpdateColliderMesh()
        {
            if (_meshUpdated && _bakeMeshJob.IsCompleted)
            {
                _bakeMeshJob.Complete();
                _meshCollider.sharedMesh = _mesh;
                RequestMeshDataReadback();
            }
        }

        void RequestMeshDataReadback()
        {
            _readbackRequest = AsyncGPUReadback.Request(_outputVertexBuffer);
            _meshUpdated = false;
        }

        void Release()
        {
            if (_computeShader != null)
            {
                Destroy(_computeShader);
            }

            _outputVertexBuffer?.Release();

            _skinnedMeshRenderer = null;
            _meshCollider = null;
            _mesh = null;
        }
    }
}
