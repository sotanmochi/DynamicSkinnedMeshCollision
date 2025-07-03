using UnityEngine;

namespace DynamicSkinnedMeshCollision.Samples
{
    public class MeshSurfacePointTrackingSample : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private MeshSurfacePointAnchorObject _pointAnchorPrefab;

        private MeshSurfacePointAnchorObject _pointAnchorObject;

        void Start()
        {
            _pointAnchorObject = Instantiate(_pointAnchorPrefab);
            _pointAnchorObject.gameObject.SetActive(false);

            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (MeshSurfacePointTrackingProvider.Find(ray, out var pointAnchor))
                {
                    Debug.Log($"<color=cyan>Position: {pointAnchor.Position}, Mesh: {pointAnchor.Mesh.name}, TrackingOrigin: {pointAnchor.TrackingOrigin.name}</color>");
                    _pointAnchorObject.PointAnchor = pointAnchor;
                    _pointAnchorObject.gameObject.SetActive(true);
                }
                else
                {
                    _pointAnchorObject.PointAnchor = null;
                    _pointAnchorObject.gameObject.SetActive(false);
                }
            }
        }
    }
}
