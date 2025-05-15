using System;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LayerLab.Casual2DCharacters.Forge
{
    public class NPCMovement : MonoBehaviour
    {
        private enum ENPCState
        {
            Idle,
            Move
        }

        [SerializeField] private ENPCState currentState = ENPCState.Idle;
        [SerializeField] private float minStateTime = 1f;
        [SerializeField] private float maxStateTime = 3f;

        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float maxMoveDistance = 5f;

        [SerializeField] private bool showMoveRange = true;

        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _stateTimer;
        private SkeletonAnimation _skeletonAnimation;

        private void Start()
        {
            _skeletonAnimation = transform.GetComponentInChildren<SkeletonAnimation>();
            _startPosition = transform.position;
            SetRandomState();
        }

        private void Update()
        {
            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0) SetRandomState();
            if(currentState == ENPCState.Move) MoveToTarget();
        }

        /// <summary>
        /// 랜덤 상태 전환
        /// Set random state
        /// </summary>
        private void SetRandomState()
        {
            currentState = (ENPCState)Random.Range(0, 2);
            _stateTimer = Random.Range(minStateTime, maxStateTime);

            if (currentState != ENPCState.Move) return;
            _skeletonAnimation.AnimationState.SetAnimation(0, "Walk", true);
            SetRandomTargetPosition();
        }

        /// <summary>
        /// 랜덤 좌표 이동 설정
        /// Set a random target location
        /// </summary>
        private void SetRandomTargetPosition()
        {
            var randomDirection = Random.insideUnitCircle.normalized;
            var randomDistance = Random.Range(1f, maxMoveDistance);
            var newTargetPosition = _startPosition + new Vector3(randomDirection.x, randomDirection.y, 0) * randomDistance;
            var distanceFromStart = Vector3.Distance(newTargetPosition, _startPosition);
            if (distanceFromStart <= maxMoveDistance)
            {
                _targetPosition = newTargetPosition;
            }
            else
            {
                _targetPosition = _startPosition + (newTargetPosition - _startPosition).normalized * maxMoveDistance;
            }
        }

        /// <summary>
        /// 목표 좌표로 이동
        /// Move to target location
        /// </summary>
        private void MoveToTarget()
        {
            var direction = _targetPosition - transform.position;
            var distance = direction.magnitude;

            _skeletonAnimation.transform.localScale = direction.x switch
            {
                > 0 => new Vector3(1f, 1f, 1f),
                < 0 => new Vector3(-1f, 1f, 1f),
                _ => _skeletonAnimation.transform.localScale
            };

            if (distance > 0.1f)
            {
                transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            }
            else
            {
                currentState = ENPCState.Idle;
                _skeletonAnimation.AnimationState.SetAnimation(0, "Idle", true);
                _stateTimer = Random.Range(minStateTime, maxStateTime);
            }
        }
    }
}