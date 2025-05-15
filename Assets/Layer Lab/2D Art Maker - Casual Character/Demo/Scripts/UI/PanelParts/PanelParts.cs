using UnityEngine;

namespace LayerLab.Casual2DCharacters.Forge
{
    public class PanelParts : MonoBehaviour
    {
        [field: SerializeField] public PanelPartsList PanelPartsList { get; set; }

        public Transform selectFrame;
        public Transform focusFrame;
        [field: SerializeField] public ChangeArrow ChangeArrow { get; set; }

        public Sprite[] spriteHide;
        public Sprite[] spriteSlotBg;

        private PartsSlot _selectSlot;
        public PartsSlot[] partsSlots;

        /// <summary>
        /// 초기화
        /// Initialize
        /// </summary>
        public void Init()
        {
            SetFrame(false);
            PanelPartsList.Init();

            partsSlots = transform.GetComponentsInChildren<PartsSlot>();
            foreach (var t in partsSlots) t.Init();
            DemoControl.Instance.OnPlayMode += CheckMode;
        }

        /// <summary>
        /// 플레이 모드 확인
        /// Check play mode
        /// </summary>
        /// <param name="playMode">플레이 모드 / Play mode</param>
        private void CheckMode(PlayMode playMode)
        {
            gameObject.SetActive(playMode == PlayMode.Home);
        }
        
        /// <summary>
        /// 슬롯 선택 해제
        /// Unselect slot
        /// </summary>
        public void UnselectSlot()
        {
            if (_selectSlot) _selectSlot.UnSelect();
            _selectSlot = null;
        }

        /// <summary>
        /// 슬롯 선택
        /// Select slot
        /// </summary>
        /// <param name="partsSlot">부품 슬롯 / Parts slot</param>
        public void SlotSelect(PartsSlot partsSlot)
        {
            if (_selectSlot == partsSlot) return;

            if (_selectSlot)
            {
                _selectSlot.UnSelect();
            }

            _selectSlot = partsSlot;

            ColorPresetManager.Instance.SetSelectByColor(partsSlot.PartsType, partsSlot.ItemColor);
            PanelPartsList.Show(partsSlot.PartsType, Player.Instance.PartsManager.GetCurrentSkinNames(partsSlot.PartsType).ToArray(), partsSlot.CanHide);
        }

        /// <summary>
        /// 슬롯 진입
        /// Enter slot
        /// </summary>
        /// <param name="partsSlot">부품 슬롯 / Parts slot</param>
        public void SlotEnter(PartsSlot partsSlot)
        {
            ChangeArrow.OnChangeLeft = null;
            ChangeArrow.OnChangeLeft += partsSlot.OnClickPrev;
            ChangeArrow.OnChangeRight = null;
            ChangeArrow.OnChangeRight += partsSlot.OnClickNext;
            SetFrame(true, partsSlot.transform);
        }

        /// <summary>
        /// 슬롯 탈출
        /// Exit slot
        /// </summary>
        /// <param name="partsSlot">부품 슬롯 / Parts slot</param>
        public void SlotExit(PartsSlot partsSlot)
        {
            SetFrame(false);
        }

        /// <summary>
        /// 프레임 설정
        /// Set frame
        /// </summary>
        /// <param name="isActive">활성화 여부 / Active status</param>
        /// <param name="parent">부모 트랜스폼 / Parent transform</param>
        public void SetFrame(bool isActive, Transform parent = null)
        {
            if (isActive)
            {
                focusFrame.gameObject.SetActive(true);
                focusFrame.SetParent(parent, false);
                focusFrame.SetAsLastSibling();

                ChangeArrow.SetTransform(parent);
                ChangeArrow.transform.SetAsLastSibling();

                if (selectFrame.gameObject.activeSelf) selectFrame.SetAsFirstSibling();
            }
            else
            {
                ChangeArrow.Hide();
                focusFrame.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 선택 프레임 설정
        /// Set selection frame
        /// </summary>
        /// <param name="isActive">활성화 여부 / Active status</param>
        /// <param name="parent">부모 트랜스폼 / Parent transform</param>
        public void SetSelectFrame(bool isActive, Transform parent = null)
        {
            if (isActive)
            {
                selectFrame.gameObject.SetActive(true);
                selectFrame.SetParent(parent, false);
                selectFrame.SetAsLastSibling();
            }
            else
            {
                selectFrame.gameObject.SetActive(false);
            }
        }
        
        private void OnDestroy()
        {
            DemoControl.Instance.OnPlayMode -= CheckMode;
            
        }
    }
}
