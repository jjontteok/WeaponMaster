using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;

namespace LayerLab.Casual2DCharacters.Forge
{
    public class AnimationController : MonoBehaviour
    {
        public static AnimationController Instance { get; private set; }

        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text animationNameText;

        private SkeletonAnimation _skeletonAnimation;
        private readonly List<string> _animationNames = new();
        private int _currentAnimationIndex = 0;

        private void Awake()
        {
            Instance = this;
        }

        
        /// <summary>
        /// 초기화
        /// Initialize
        /// </summary>
        /// <param name="skeletonAnimation">스켈레톤 애니메이션 / Skeleton animation</param>
        public void Init()
        {
            _skeletonAnimation = Player.Instance.PartsManager.GetSkeletonAnimation();
            LoadAnimationNames();

            prevButton.onClick.AddListener(PlayPreviousAnimation);
            nextButton.onClick.AddListener(PlayNextAnimation);

            if (_animationNames.Count > 0)
            {
                PlayAnimationByName("Idle");
            }
        }

        /// <summary>
        /// 애니메이션 이름 로드
        /// Load animation names
        /// </summary>
        private void LoadAnimationNames()
        {
            _animationNames.Clear();

            var skeletonData = _skeletonAnimation.Skeleton.Data;
            for (int i = 0; i < skeletonData.Animations.Count; i++)
            {
                _animationNames.Add(skeletonData.Animations.Items[i].Name);
            }
        }

        /// <summary>
        /// 애니메이션 재생
        /// Play animation
        /// </summary>
        /// <param name="index">애니메이션 인덱스 / Animation index</param>
        private void PlayAnimation(int index)
        {
            if (_animationNames.Count == 0) return;

            _currentAnimationIndex = Mathf.Clamp(index, 0, _animationNames.Count - 1);

            var animationName = _animationNames[_currentAnimationIndex];
            
            Player.Instance.PartsManager.PlayAnimation(animationName);
            UpdateAnimationNameText();
        }

        /// <summary>
        /// 다음 애니메이션 재생
        /// Play next animation
        /// </summary>
        public void PlayNextAnimation()
        {
            var nextIndex = (_currentAnimationIndex + 1) % _animationNames.Count;
            PlayAnimation(nextIndex);
        }

        /// <summary>
        /// 이전 애니메이션 재생
        /// Play previous animation
        /// </summary>
        public void PlayPreviousAnimation()
        {
            var prevIndex = (_currentAnimationIndex - 1 + _animationNames.Count) % _animationNames.Count;
            PlayAnimation(prevIndex);
        }

        /// <summary>
        /// UI 텍스트 업데이트
        /// Update UI text
        /// </summary>
        private void UpdateAnimationNameText()
        {
            if (animationNameText == null || _animationNames.Count <= 0) return;

            var animationName = _animationNames[_currentAnimationIndex];
            animationNameText.text = animationName;
        }

        /// <summary>
        /// 특정 이름의 애니메이션 재생
        /// Play animation by name
        /// </summary>
        /// <param name="animationName">애니메이션 이름 / Animation name</param>
        public void PlayAnimationByName(string animationName)
        {
            var index = _animationNames.IndexOf(animationName);
            if (index >= 0)
            {
                PlayAnimation(index);
            }
            else
            {
                Debug.LogWarning($"Animation not found: {animationName}");
            }
        }

        /// <summary>
        /// 애니메이션 이름 목록 가져오기
        /// Get animation name list
        /// </summary>
        /// <returns>애니메이션 이름 목록 / Animation name list</returns>
        public List<string> GetAnimationNames()
        {
            return new List<string>(_animationNames);
        }

        // 현재 재생 중인 애니메이션 이름 가져오기
        public string GetCurrentAnimationName()
        {
            if (_animationNames.Count > 0 && _currentAnimationIndex >= 0 && _currentAnimationIndex < _animationNames.Count)
            {
                return _animationNames[_currentAnimationIndex];
            }

            return string.Empty;
        }

        /// <summary>
        /// 현재 재생 중인 애니메이션 이름 가져오기
        /// Get current animation name
        /// </summary>
        /// <returns>애니메이션 이름 / Animation name</returns>
        public string CurrentAnimation => _skeletonAnimation.AnimationState.Tracks.Items[0].Animation.Name;
        
    }
}