using UnityEngine;
using UnityEngine.EventSystems;

namespace DynamicSkinnedMeshCollision.Samples
{
    public sealed class SimpleCameraPoseController : MonoBehaviour
    {
        [SerializeField] Camera _camera;
        [SerializeField] Vector3 _moveScaleFactor = new Vector3(0.1f, 0.1f, 0.01f);
        [SerializeField] float _rotationScaleFactor = 3.0f;

        void Awake()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }
        }

        void Update()
        {
            if (_camera == null) return;

            // NOTE: Enable the scene view camera controller when the pointer is not over any UI element.
            if (EventSystem.current.IsPointerOverGameObject()) return;

            var input = ProcessMouseInput();

            if (input.ButtonType == MouseButtonType.Right)
            {
                Rotate(new Vector2(input.DeltaX, -input.DeltaY) * _rotationScaleFactor);
            }

            var deltaPosition = new Vector3(
                -input.DeltaX * _moveScaleFactor.x,
                -input.DeltaY * _moveScaleFactor.y,
                input.ScrollDelta * _moveScaleFactor.z);

            if (input.ButtonType != MouseButtonType.Middle)
            {
                deltaPosition.x = 0;
                deltaPosition.y = 0;
            }

            Move(deltaPosition);
        }

        public void SetCamera(Camera camera)
        {
            _camera = camera;
        }

        private void Move(Vector3 deltaPosition)
        {
            var right = _camera.transform.right * deltaPosition.x;
            var up = _camera.transform.up * deltaPosition.y;
            var forward = _camera.transform.forward * deltaPosition.z;
            _camera.transform.position += (forward + right + up);
        }

        private void Rotate(Vector2 deltaAngle)
        {
            _camera.transform.RotateAround(_camera.transform.position, Vector3.up, deltaAngle.x);
            _camera.transform.RotateAround(_camera.transform.position, _camera.transform.right, deltaAngle.y);
        }

        private MouseInput ProcessMouseInput()
        {
            var buttonType = MouseButtonType.None;

            if (Input.GetMouseButton(0))
            {
                buttonType = MouseButtonType.Left;
            }
            else if (Input.GetMouseButton(1))
            {
                buttonType = MouseButtonType.Right;
            }
            else if (Input.GetMouseButton(2))
            {
                buttonType = MouseButtonType.Middle;
            }

            return new MouseInput()
            {
                ButtonType = buttonType,
                DeltaX = Input.GetAxis("Mouse X"),
                DeltaY = Input.GetAxis("Mouse Y"),
                ScrollDelta = Input.mouseScrollDelta.y
            };
        }

        struct MouseInput
        {
            public MouseButtonType ButtonType;
            public float DeltaX;
            public float DeltaY;
            public float ScrollDelta;
        }

        enum MouseButtonType
        {
            None,
            Left,
            Middle,
            Right,
        }
    }
}
