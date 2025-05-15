using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LayerLab.Casual2DCharacters.Forge
{
    public class PartsSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [field: SerializeField] public PartsType PartsType { get; set; }
        private bool IsHide { get; set; }
        [field: SerializeField] public bool CanHide { get; set; }

        [SerializeField] private Image imageSlotBg;
        [SerializeField] private Image imageIconHide;
        [SerializeField] private Image imageTypeIcon;
        [SerializeField] private Image imageItem;
        public Color ItemColor { get; private set; }
        public bool IsSelect { get; set; }
        
        
        private void Awake()
        {
            // 숨김처리 가능한 유닛만 눈 아이콘 켜주기
            // Turn on the eye icon only for units that can be hidden
            imageIconHide.gameObject.SetActive(CanHide);
        }

        /// <summary>
        /// 초기화
        /// Initialize
        /// </summary>
        public void Init()
        {
            Player.Instance.PartsManager.OnColorChange += OnColorChange;
            Player.Instance.PartsManager.OnChangedParts += OnChangedItem;
            
            PartsType = (PartsType)Enum.Parse(typeof(PartsType), gameObject.name);
            SetSlot();
        }

        /// <summary>
        /// 파츠 색상 변경
        /// Change part color
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <param name="color">색상 / Color</param>
        private void OnColorChange(PartsType partsType, Color color)
        {
            if(PartsType != partsType) return;
            
            if (DemoControl.CanChangeColor(partsType))
            {
                ItemColor = color;
                imageItem.color = ItemColor;
            }
            else
            {
                ItemColor = Color.white;
                imageItem.color = ItemColor;
            }
        }

        /// <summary>
        /// 파괴될때 이벤트 제거
        /// Remove event when destroyed
        /// </summary>
        private void OnDestroy()
        {
            Player.Instance.PartsManager.OnColorChange -= OnColorChange;
            Player.Instance.PartsManager.OnChangedParts -= OnChangedItem;
        }
        
        /// <summary>
        /// 아이템 변경 시 호출
        /// Called when item changes
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <param name="index">아이템 인덱스 / Item index</param>
        private void OnChangedItem(PartsType partsType, int index)
        {
            SetSlot();
        }

        /// <summary>
        /// 슬롯 설정
        /// Configure slot
        /// </summary>
        private void SetSlot()
        {
            var isEmptySlot = Player.Instance.PartsManager.IsEmptyItemByType(PartsType);
            if (isEmptySlot) 
            {
                // 장착된 아이템이 없습니다
                // There are no equipped items
                imageIconHide.sprite = IsHide ? DemoControl.Instance.PanelParts.spriteHide[0] : DemoControl.Instance.PanelParts.spriteHide[1];
                imageSlotBg.sprite = DemoControl.Instance.PanelParts.spriteSlotBg[0];
            }
            else 
            {
                // 장착된 아이템이 있습니다
                // There is an equipped item
                SetItem();

                if (IsHide)
                {
                    imageItem.CrossFadeAlpha(0.2f, 0f, true);
                    imageSlotBg.sprite = DemoControl.Instance.PanelParts.spriteSlotBg[3];
                    imageIconHide.sprite = DemoControl.Instance.PanelParts.spriteHide[0];
                }
                else
                {
                    imageItem.CrossFadeAlpha(1f, 0f, true);
                    imageSlotBg.sprite = DemoControl.Instance.PanelParts.spriteSlotBg[1];
                    imageIconHide.sprite = DemoControl.Instance.PanelParts.spriteHide[1];
                    if (!IsHide) imageSlotBg.sprite = DemoControl.Instance.PanelParts.spriteSlotBg[2];
                }
            }

            imageTypeIcon.gameObject.SetActive(isEmptySlot);
            imageItem.gameObject.SetActive(!isEmptySlot);
            imageIconHide.SetNativeSize();
            imageIconHide.transform.SetAsLastSibling();
        }

        /// <summary>
        /// 아이템 설정
        /// Configure item
        /// </summary>
        public void SetItem()
        {
            imageItem.sprite = DemoControl.Instance.GetSprite(Player.Instance.PartsManager.GetCurrentPartsName(PartsType));
            imageItem.SetNativeSize();
        }

        /// <summary>
        /// 다음 화살표 클릭
        /// Click next arrow
        /// </summary>
        public void OnClickNext()
        {
            Player.Instance.PartsManager.NextItem(PartsType);
            SetSlot();
        }

        /// <summary>
        /// 이전 화살표 클릭
        /// Click previous arrow
        /// </summary>
        public void OnClickPrev()
        {
            Player.Instance.PartsManager.PrevItem(PartsType);
            SetSlot();
        }

        /// <summary>
        /// 눈 아이콘 클릭
        /// Click eye icon
        /// </summary>
        public void OnClickHide()
        {
            if (!CanHide) return;
            IsHide = !IsHide;

            AudioManager.Instance.PlaySound(SoundList.ButtonEye);
            Player.Instance.PartsManager.SetHideItem(PartsType, IsHide);
            SetSlot();
        }



        #region UnityEvent
        /// <summary>
        /// 슬롯 영역에 커서가 들어왔을 때
        /// When cursor enters slot area
        /// </summary>
        /// <param name="eventData">이벤트 데이터 / Event data</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            DemoControl.Instance.PanelParts.SlotEnter(this);
            SetSlot();
        }

        /// <summary>
        /// 슬롯 영역에서 커서가 나갔을 때
        /// When cursor exits slot area
        /// </summary>
        /// <param name="eventData">이벤트 데이터 / Event data</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            DemoControl.Instance.PanelParts.SlotExit(this);
            SetSlot();
        }

        /// <summary>
        /// 슬롯 클릭 시
        /// When slot is clicked
        /// </summary>
        /// <param name="eventData">이벤트 데이터 / Event data</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                IsSelect = true;
                AudioManager.Instance.PlaySound(SoundList.ButtonDefault);
                DemoControl.Instance.PanelParts.SlotSelect(this);
                DemoControl.Instance.PanelParts.SetSelectFrame(true, transform);
            }
            else
            {
                AudioManager.Instance.PlaySound(SoundList.ButtonDefault);
                Player.Instance.PartsManager.EquipParts(PartsType, -1);
            }
            SetSlot();
        }
        #endregion

        
        /// <summary>
        /// 선택 해제
        /// Deselect
        /// </summary>
        public void UnSelect()
        {
            IsSelect = false;
            DemoControl.Instance.PanelParts.SetSelectFrame(false);
            SetSlot();
        }
    }
}
