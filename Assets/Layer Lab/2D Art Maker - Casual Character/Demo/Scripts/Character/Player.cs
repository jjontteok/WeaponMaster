using System;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LayerLab.Casual2DCharacters.Forge
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Player : MonoBehaviour
    {
        public enum EPlayerState
        {
            Idle,
            Run
        }

        public EPlayerState PlayerState { get; set; }

        public static Player Instance { get; private set; }
        [field: SerializeField] public PartsManager PartsManager { get; private set; }
        
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float stoppingDistance = 0.1f;
        [SerializeField] private float rotationSpeed = 10f;

        private bool InDistance => InputDistance() <= stoppingDistance;
        [SpineEvent] private readonly string _footStepEventName = "Step";
        private Vector3 _targetPosition;
        private Vector3 _firstPosition;
        private Vector2 _moveDirection;
        private Rigidbody2D _rigidbody;
        private bool _isMoving;
        private bool _isDragging;
        private bool _canMove;

        private Collider2D _characterCollider;
        
        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 초기화
        /// Initialize
        /// </summary>
        public void Init()
        {
            _characterCollider = GetComponent<CircleCollider2D>();
            _firstPosition = transform.position;
            _targetPosition = transform.position;
            AddEvent();
            SetRigidbody();
            DemoControl.Instance.OnPlayMode -= CheckMode;
            DemoControl.Instance.OnPlayMode += CheckMode;
        }

        /// <summary>
        /// 모드 설정 확인
        /// Check mode settings
        /// </summary>
        /// <param name="playMode">플레이 모드 / Play mode</param>
        private void CheckMode(PlayMode playMode)
        {
            switch (playMode)
            {
                case PlayMode.Home: Reset(); break;
                case PlayMode.Experience: OnMove(); break;
                case PlayMode.None: break;
                default: throw new ArgumentOutOfRangeException(nameof(playMode), playMode, null);
            }
        }
        
        /// <summary>
        /// 리지드바디 설정
        /// Set rigidbody properties
        /// </summary>
        private void SetRigidbody()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.gravityScale = 0f;
            _rigidbody.freezeRotation = true;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        /// <summary>
        /// 콜라이더 활성화/비활성화 설정
        /// Set collider enable/disable
        /// </summary>
        /// <param name="isOn">활성화 여부 / Enable status</param>
        public void SetCollider(bool isOn)
        {
            _characterCollider.enabled = isOn;
        }
        

        /// <summary>
        /// 초기 위치로 리셋
        /// Reset to initial position
        /// </summary>
        public void Reset()
        {
            if (!_canMove) return;
            _canMove = false;
            _isMoving = true;
            _targetPosition = _firstPosition;
        }

        /// <summary>
        /// 이동 가능 상태로 설정
        /// Set to movable state
        /// </summary>
        public void OnMove()
        {
            _canMove = true;
        }


        private void Update()
        {
            CheckDistance();
            
            if (_rigidbody.linearVelocity.magnitude > 0 && PlayerState != EPlayerState.Run)
            {
                PlayerState = EPlayerState.Run;
                PartsManager.PlayAnimation("Run");
            }
            else if (_rigidbody.linearVelocity.magnitude <= 0 && PlayerState != EPlayerState.Idle)
            {
                PlayerState = EPlayerState.Idle;
                PartsManager.PlayAnimation("Idle");
            }

            if (!_canMove) return;
            HandleMouseInput();
        }

        private void FixedUpdate()
        {
            MoveCharacter();
        }

        
        /// <summary>
        /// 현재 위치와 목표 위치 사이의 거리 계산
        /// Calculate distance between current location and target location
        /// </summary>
        /// <returns>거리 / Distance</returns>
        private float InputDistance()
        {
            return (_targetPosition - transform.position).magnitude;
        }


        /// <summary>
        /// 마우스 입력 처리
        /// Handle mouse input
        /// </summary>
        private void HandleMouseInput()
        {
            var isOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !isOverUI)
            {
                _isDragging = true;
            }

            if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && _isDragging)
            {
                var mousePosition = CameraControl.Instance.Cam.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = transform.position.z;
                _targetPosition = mousePosition;
            }

            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            {
                _isDragging = false;
                if (!isOverUI)
                {
                    _isMoving = false;
                    _rigidbody.linearVelocity = Vector2.zero;
                }
            }
        }

        /// <summary>
        /// 캐릭터 이동 처리
        /// Process character movement
        /// </summary>
        private void MoveCharacter()
        {
            if (_isMoving)
            {
                _moveDirection = (_targetPosition - transform.position).normalized;
                _rigidbody.linearVelocity = _moveDirection * moveSpeed;
                RotateTowardsMovementDirection(_moveDirection);
            }
            else
            {
                _rigidbody.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// 목표 지점까지의 거리 확인
        /// Check distance to target point
        /// </summary>
        private void CheckDistance()
        {
            if (_isMoving && InDistance)
            {
                _isMoving = false;
                _rigidbody.linearVelocity = Vector2.zero;

                if (DemoControl.Instance.CurrentPlayMode == PlayMode.Home)
                {
                    SetCollider(true);
                    PartsManager.transform.localScale = new Vector3(1f, 1f, 1f);
                }
            }
            else if (!_isMoving && !InDistance && _isDragging)
            {
                _isMoving = true;
            }
        }

        /// <summary>
        /// 이동 방향에 따른 캐릭터 회전
        /// Rotate character according to movement direction
        /// </summary>
        /// <param name="moveDirection">이동 방향 / Movement direction</param>
        private void RotateTowardsMovementDirection(Vector2 moveDirection)
        {
            if (InDistance) return;
            if (moveDirection != Vector2.zero)
            {
                if (moveDirection.x < 0)
                {
                    PartsManager.transform.localScale = new Vector3(1f, 1f, 1f);
                }
                else
                {
                    PartsManager.transform.localScale = new Vector3(-1f, 1f, 1f);
                }
            }
        }

        /// <summary>
        /// 스파인 이벤트 추가
        /// Add spine event
        /// </summary>
        private void AddEvent()
        {
            PartsManager.GetSkeletonAnimation().AnimationState.Event -= HandleEvent;
            PartsManager.GetSkeletonAnimation().AnimationState.Event += HandleEvent;
        }

        /// <summary>
        /// 스파인 이벤트 처리
        /// Handle spine events
        /// </summary>
        /// <param name="trackEntry">트랙 엔트리 / Track entry</param>
        /// <param name="e">이벤트 / Event</param>
        private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
        {
            if (e.Data.Name == _footStepEventName)
            {
                AudioManager.Instance.PlayStepSound();
            }
        }

        private void OnDestroy()
        {
            DemoControl.Instance.OnPlayMode -= CheckMode;
            if (PartsManager)
            {
                PartsManager.GetSkeletonAnimation().AnimationState.Event -= HandleEvent;
            }
        }
    }
}
