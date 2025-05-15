using UnityEngine;
using UnityEngine.UI;

namespace LayerLab.Casual2DCharacters.Forge
{
    public class PartsListSlot : MonoBehaviour
    {
        public int SlotIndex { get; private set; }
        
        [SerializeField] private Image imageItem;
        private PanelPartsList _panelPartsList;

        /// <summary>
        /// 아이템 색 변경
        /// </summary>
        /// <param name="color"></param>
        public void ChangeColor(Color color)
        {
            imageItem.color = color;
        }

        /// <summary>
        /// 프리셋 슬롯 설정
        /// </summary>
        /// <param name="panelPartsList"></param>
        /// <param name="sprite"></param>
        /// <param name="index"></param>
        public void SetSlot(PanelPartsList panelPartsList, Sprite sprite, int index)
        {
            SlotIndex = index;
            _panelPartsList = panelPartsList;
            imageItem.sprite = sprite;
            imageItem.SetNativeSize();
        }

        /// <summary>
        /// 프리셋 슬롯 선택
        /// </summary>
        public void SelectSlot()
        {
            AudioManager.Instance.PlaySound(SoundList.ButtonDefault);
            _panelPartsList.SelectSlot(this);
        }
    }
}
