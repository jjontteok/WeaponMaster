using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace LayerLab.Casual2DCharacters.Forge
{
    public class ColorPresetManager : MonoBehaviour
    {
        [Serializable]
        public class ColorPreset
        {
            public Color color;
            public Button button;
        }
        
        public static ColorPresetManager Instance { get; set; }
        [SerializeField] private List<ColorPreset> colorPresets = new();
        [SerializeField] private Image selectedPresetIndicator;
        
        private int _selectedHairPresetIndex;
        private int _selectedBeardPresetIndex;
        private int _selectedBrowPresetIndex;
        private PartsType _currentPart;
        
        
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
            InitializePresetButtons();
            ApplyPresetColor(PartsType.Hair_Short, Random.Range(0, colorPresets.Count));
            ApplyPresetColor(PartsType.Brow, Random.Range(0, colorPresets.Count));
            ApplyPresetColor(PartsType.Beard, Random.Range(0, colorPresets.Count));
        }
        
        
        /// <summary>
        /// 타입 값으로 색상 가져오기
        /// Get color by part type
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <returns>색상 / Color</returns>
        public Color GetColorByType(PartsType partsType)
        {
            switch (partsType)
            {
                case PartsType.Hair_Short: return colorPresets[_selectedHairPresetIndex].color;
                case PartsType.Beard: return colorPresets[_selectedBeardPresetIndex].color;
                case PartsType.Brow: return colorPresets[_selectedBrowPresetIndex].color;
            }

            return colorPresets[0].color;
        }
    

        /// <summary>
        /// 타입별 색상 프리셋 설정
        /// Set color preset by part type
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        public void SetPresetColor(PartsType partsType)
        {
            _currentPart = partsType;
            UpdateSelectedIndicator(partsType);
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 색상별 선택 설정
        /// Set selection by color
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <param name="color">색상 / Color</param>
        public void SetSelectByColor(PartsType partsType, Color color)
        {
            ApplyPresetColor(partsType, GetIndexByColor(color));
        }

        /// <summary>
        /// 모든 부품에 랜덤 색상 설정
        /// Set random colors for all parts
        /// </summary>
        public void SetRandomAllColor()
        {
            ApplyPresetColor(PartsType.Hair_Short, Random.Range(0, colorPresets.Count));
            ApplyPresetColor(PartsType.Brow, Random.Range(0, colorPresets.Count));
            ApplyPresetColor(PartsType.Beard, Random.Range(0, colorPresets.Count));
        }

        /// <summary>
        /// 색상으로 인덱스 가져오기
        /// Get index by color
        /// </summary>
        /// <param name="color">색상 / Color</param>
        /// <returns>인덱스 / Index</returns>
        private int GetIndexByColor(Color color)
        {
            for (int i = 0; i < colorPresets.Count; i++)
            {
                if (colorPresets[i].color == color)
                {
                    return i;
                }
            }

            return 0;
        }
        
        /// <summary>
        /// 프리셋 버튼 초기화
        /// Initialize preset buttons
        /// </summary>
        private void InitializePresetButtons()
        {
            for (int i = 0; i < colorPresets.Count; i++)
            {
                var index = i;

                var buttonImage = colorPresets[i].button.GetComponent<Image>();
                if (buttonImage != null) buttonImage.color = colorPresets[i].color;

                colorPresets[i].button.onClick.AddListener(() => ApplyPresetColor(_currentPart, index));
            }
        }

        /// <summary>
        /// 프리셋 색상 적용
        /// Apply preset color
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <param name="presetIndex">프리셋 인덱스 / Preset index</param>
        private void ApplyPresetColor(PartsType partsType, int presetIndex)
        {
            if (presetIndex < 0 || presetIndex >= colorPresets.Count)
                return;

            switch (partsType)
            {
                case PartsType.Hair_Short: _selectedHairPresetIndex = presetIndex; break;
                case PartsType.Beard: _selectedBeardPresetIndex = presetIndex; break;
                case PartsType.Brow: _selectedBrowPresetIndex = presetIndex; break;
            }

            UpdateSelectedIndicator(partsType);
            var selectedColor = colorPresets[presetIndex].color;
            Player.Instance.PartsManager.OnColorChange?.Invoke(partsType, selectedColor); 

            switch (partsType)
            {
                case PartsType.Hair_Short: Player.Instance.PartsManager.ChangeHairColor(selectedColor); break;
                case PartsType.Beard: Player.Instance.PartsManager.ChangeBeardColor(selectedColor); break;
                case PartsType.Brow: Player.Instance.PartsManager.ChangeBrowColor(selectedColor); break;
            }
        }

        /// <summary>
        /// 선택된 색상 UI 업데이트
        /// Update selected color UI
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        private void UpdateSelectedIndicator(PartsType partsType)
        {
            if (selectedPresetIndicator == null) return;

            var selectedIndex = 0;

            switch (partsType)
            {
                case PartsType.Hair_Short: selectedIndex = _selectedHairPresetIndex; break;
                case PartsType.Beard: selectedIndex = _selectedBeardPresetIndex; break;
                case PartsType.Brow: selectedIndex = _selectedBrowPresetIndex; break;
            }

            if (selectedIndex >= 0 && selectedIndex < colorPresets.Count)
            {
                var buttonRect = colorPresets[selectedIndex].button;
                selectedPresetIndicator.transform.SetParent(buttonRect.transform, false);
                selectedPresetIndicator.transform.localPosition = Vector3.zero;
                selectedPresetIndicator.gameObject.SetActive(true);

            }
            else
            {
                selectedPresetIndicator.gameObject.SetActive(false);
            }
        }

    }
}