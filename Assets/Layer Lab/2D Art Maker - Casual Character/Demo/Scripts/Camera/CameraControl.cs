using System.Collections;
using UnityEngine;

namespace LayerLab.Casual2DCharacters.Forge
{
    [RequireComponent(typeof(Camera))]
    public class CameraControl : MonoBehaviour
    {
        public static CameraControl Instance { get; private set; }
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset;
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 10f;
        [SerializeField] private float smoothDampTime = 0.15f;

        public Camera Cam { get; private set; }
        public float targetZoom;

        private Vector2 _minBounds;
        private Vector2 _maxBounds;
        private Vector3 _currentVelocity = Vector3.zero;

        [SerializeField] private Transform boundLeft;
        [SerializeField] private Transform boundRight;
        [SerializeField] private Transform boundTop;
        [SerializeField] private Transform boundBottom;

        private void Awake()
        {
            Instance = this;
        }
        
        private void Update()
        {
            HandleZoom();
        }

        private void LateUpdate()
        {
            if (target == null) return;
            MoveCamera();
        }
        
        /// <summary>
        /// 초기화
        /// Initialize
        /// </summary>
        public void Init()
        {
            Cam = GetComponent<Camera>();
            
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }

            targetZoom = Cam.orthographicSize;
            SetBounds();
            
            var desiredPosition = target.position + (Vector3)offset;
            desiredPosition.z = transform.position.z;

            transform.position = desiredPosition;
        }

        /// <summary>
        /// 카메라 움직임 처리
        /// Handle camera movement
        /// </summary>
        private void MoveCamera()
        {
            var desiredPosition = target.position + (Vector3)offset;
            desiredPosition.z = transform.position.z;

            var vertExtent = Cam.orthographicSize;
            var horzExtent = vertExtent * Cam.aspect; 

            var minX = _minBounds.x + horzExtent;
            var maxX = _maxBounds.x - horzExtent;

            if (minX > maxX)
            {
                var centerX = (_minBounds.x + _maxBounds.x) * 0.5f;
                desiredPosition.x = centerX;
                
                _currentVelocity.x = 0;
            }
            else
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            }

            var minY = _minBounds.y + vertExtent;
            var maxY = _maxBounds.y - vertExtent;

            if (minY > maxY)
            {
                var centerY = (_minBounds.y + _maxBounds.y) * 0.5f;
                desiredPosition.y = centerY;
                _currentVelocity.y = 0;
            }
            else
            {
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
            }

            var smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, smoothDampTime);
            transform.position = smoothedPosition;
        }

        /// <summary>
        /// 줌 처리
        /// Handle zoom functionality
        /// </summary>
        private void HandleZoom()
        {
            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                var previousZoom = targetZoom;
                
                targetZoom -= scroll * zoomSpeed;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
                
                Cam.orthographicSize = targetZoom;
                
                if (Mathf.Abs(previousZoom - targetZoom) > 0.01f)
                {
                    var currentPos = transform.position;
                    var verticalExtent = Cam.orthographicSize;
                    var horizontalExtent = verticalExtent * Cam.aspect;
                    
                    var minX = _minBounds.x + horizontalExtent;
                    var maxX = _maxBounds.x - horizontalExtent;
                    var minY = _minBounds.y + verticalExtent;
                    var maxY = _maxBounds.y - verticalExtent;
                    
                    if (minX > maxX)
                    {
                        var centerX = (_minBounds.x + _maxBounds.x) * 0.5f;
                        currentPos.x = centerX;
                    }
                    else
                    {
                        currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);
                    }
                    
                    if (minY > maxY)
                    {
                        var centerY = (_minBounds.y + _maxBounds.y) * 0.5f;
                        currentPos.y = centerY;
                    }
                    else
                    {
                        currentPos.y = Mathf.Clamp(currentPos.y, minY, maxY);
                    }
                    
                    transform.position = currentPos;
                    
                    // 속도 벡터 초기화
                    _currentVelocity = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// 객체에서 카메라 경계 설정
        /// Set camera boundaries from objects
        /// </summary>
        private void SetBounds()
        {
            if (boundLeft && boundRight && boundTop && boundBottom)
            {
                _minBounds = new Vector2(boundLeft.position.x, boundBottom.position.y);
                _maxBounds = new Vector2(boundRight.position.x, boundTop.position.y);
            }
            else
            {
                Debug.LogWarning("카메라 바운드 오브젝트가 설정되지 않았습니다. Camera bounds object is not set.");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_minBounds == Vector2.zero && _maxBounds == Vector2.zero)
                return;
                
            Gizmos.color = Color.green;

            var min = new Vector3(_minBounds.x, _minBounds.y, 0);
            var max = new Vector3(_maxBounds.x, _maxBounds.y, 0);

            var bottomLeft = new Vector3(min.x, min.y, 0);
            var bottomRight = new Vector3(max.x, min.y, 0);
            var topRight = new Vector3(max.x, max.y, 0);
            var topLeft = new Vector3(min.x, max.y, 0);

            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }
    }
}