using System;
using UnityEngine;

namespace LayerLab.Casual2DCharacters.Forge
{
    public enum PlayMode
    {
        None,
        Home,
        Experience
    }

    public class DemoControl : MonoBehaviour
    {
        public static DemoControl Instance { get; private set; }
        public Action<PlayMode> OnPlayMode { get; set; }
        public PlayMode CurrentPlayMode { get; set; } 

        [field: SerializeField] public PanelParts PanelParts { get; set; }
        [field: SerializeField] public PanelPreset PanelPreset { get; set; }
        [field: SerializeField] public PresetData PresetData { get; set; } // ScriptableObject 참조

        public static bool CanChangeColor(PartsType partsType) => partsType is PartsType.Hair_Short or PartsType.Brow or PartsType.Beard;
        
        [SerializeField] private Sprite[] sprites;
        [SerializeField] private GameObject buttonHome, buttonRandomParts, buttonExperience;
        [SerializeField] private GameObject buttonMouseMove;
        
        private const string PathDiscord = "https://discord.gg/qCsVSHHcY7";
        private const string PathFacebook = "https://www.facebook.com/layerlab";
        private const string PathYoutube = "https://www.youtube.com/@LayerlabGames";
        private const string PathAssetStore = "https://assetstore.unity.com/publishers/5232";
        private const string PathAsset = "https://assetstore.unity.com/packages/2d/characters/2d-art-maker-casual-characters-316781";
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ChangeMode(PlayMode.Home);
            Init();
        }

        
      
       
        /// <summary>
        /// 초기화
        /// Initialize
        /// </summary>
        public void Init()
        {
            Player.Instance.PartsManager.Init();
            CameraControl.Instance.Init();
            Player.Instance.Init();
            PanelParts.Init();
            PanelPreset.Init();
            AnimationController.Instance.Init();
            ColorPresetManager.Instance.Init();
        }
        
        /// <summary>
        /// 플레이 모드 변경
        /// Change play mode
        /// </summary>
        /// <param name="playMode">플레이 모드 / Play mode</param>
        public void ChangeMode(PlayMode playMode)
        {
            if (CurrentPlayMode == playMode) return;
            CurrentPlayMode = playMode;
            OnPlayMode?.Invoke(playMode);

            switch (playMode)
            {
                case PlayMode.Home:
                    buttonMouseMove.SetActive(false);
                    buttonRandomParts.SetActive(true);
                    buttonExperience.SetActive(true);
                    buttonHome.SetActive(false);
                    break;
                case PlayMode.Experience:
                    buttonMouseMove.SetActive(true);
                    buttonRandomParts.SetActive(false);
                    buttonExperience.SetActive(false);
                    buttonHome.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// 스프라이트 가져오기
        /// Get sprite
        /// </summary>
        /// <param name="name">스프라이트 이름 / Sprite name</param>
        /// <returns>스프라이트 / Sprite</returns>
        public Sprite GetSprite(string name)
        {
            foreach (var t in sprites)
            {
                if (t.name == name.Split("/")[1]) return t;
            }

            return null;
        }
        
        /// <summary>
        /// 랜덤 부품 버튼 클릭
        /// Click random parts button
        /// </summary>
        public void OnClick_RandomParts()
        {
            AudioManager.Instance.PlaySound(SoundList.ButtonRandom);
            PanelParts.PanelPartsList.OnClick_Close();
            Player.Instance.PartsManager.RandomParts();
            ColorPresetManager.Instance.SetRandomAllColor();
        }

        /// <summary>
        /// 체험하기 버튼 클릭
        /// Click experience button
        /// </summary>
        public void OnClick_Experience()
        {
            Player.Instance.SetCollider(true);
            AudioManager.Instance.PlaySound(SoundList.ButtonDefault);
            ChangeMode(PlayMode.Experience);
        }

        /// <summary>
        /// 홈 버튼 클릭
        /// Click home button
        /// </summary>
        public void OnClick_Home()
        {
            Player.Instance.SetCollider(false);
            AudioManager.Instance.PlaySound(SoundList.ButtonDefault);
            ChangeMode(PlayMode.Home);
        }

        public void OnClickSNS_Discord()
        {
            AudioManager.Instance.PlaySound(SoundList.ButtonDefault);
            Application.OpenURL(PathDiscord);
        }

        public void OnClickSNS_Facebook()
        {
            AudioManager.Instance.PlaySound(SoundList.ButtonDefault);
            Application.OpenURL(PathFacebook);
        }

        public void OnClickSNS_Youtube()
        {
            AudioManager.Instance.PlaySound(SoundList.ButtonDefault);
            Application.OpenURL(PathYoutube);
        }

        public void OnClickSNS_AssetStore()
        {
            AudioManager.Instance.PlaySound(SoundList.ButtonDefault);
            Application.OpenURL(PathAssetStore);
        }

        public void OnClickSNS_Asset()
        {
            AudioManager.Instance.PlaySound(SoundList.ButtonDefault);
            Application.OpenURL(PathAsset);
        }
    }
}