using UnityEngine;

namespace DynamicSkinnedMeshCollision.Samples
{
    public sealed class MeshSurfacePointAnchorObject : MonoBehaviour
    {
        public MeshSurfacePointAnchor PointAnchor { get; set; }

        void Update()
        {
            transform.position = PointAnchor?.Position ?? Vector3.zero;
        }
    }
}
