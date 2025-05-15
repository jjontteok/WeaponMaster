using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LayerLab.Casual2DCharacters.Forge
{
    public class PanelPartsList : MonoBehaviour
    {
        [field: SerializeField] private PartsListSlot PartsListSlot { get; set; }
        private readonly List<PartsListSlot> _activePartsList = new();
        
        [SerializeField] private TMP_Text textTitle;
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject colorPalette;
        [SerializeField] private GameObject objButtonReset;
        [SerializeField] private Image imgSelectFrame;
        
        private bool IsShow => gameObject.activeSelf;
        private RectTransform _rect;
        private PartsType _activePartsType;
        private Vector3 _targetPos;
        private Vector3 _startPos;
        private Vector3 _velocity = Vector3.zero;
        
        
        #region Public
        
        /// <summary>
        /// 초기화
        /// Initialize
        /// </summary>
        public void Init()
        {
            PartsListSlot.gameObject.SetActive(false);
            _startPos = transform.localPosition;
            _rect = GetComponent<RectTransform>();

            
            Player.Instance.PartsManager.OnChangedParts += ChangePartType;
            Close();
        }
        
        /// <summary>
        /// 부품 목록 표시
        /// Show parts list
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <param name="parts">부품 배열 / Parts array</param>
        /// <param name="canHide">숨김 가능 여부 / Can hide</param>
        public void Show(PartsType partsType, string[] parts, bool canHide)
        {
            _activePartsType = partsType;
            Close();

            if (_activePartsList.Count < parts.Length)
            {
                var count = parts.Length - _activePartsList.Count;
                for (var i = 0; i < count; i++)
                {
                    var partsSlot = Instantiate(PartsListSlot, contentParent);
                    _activePartsList.Add(partsSlot);
                }
            }

            foreach (var t in _activePartsList) t.gameObject.SetActive(false);

            for (var i = 0; i < parts.Length; i++)
            {
                _activePartsList[i].SetSlot(this, DemoControl.Instance.GetSprite(parts[i]), i);
                _activePartsList[i].gameObject.SetActive(true);
            }

            gameObject.SetActive(true);
            textTitle.text = partsType.ToString();
            _targetPos = _startPos;

            if (DemoControl.CanChangeColor(partsType))
            {
                ChangeColorList(ColorPresetManager.Instance.GetColorByType(partsType));
                ColorPresetManager.Instance.SetPresetColor(partsType);
                colorPalette.SetActive(true);
            }
            else
            {
                colorPalette.SetActive(false);
                ChangeColorList(Color.white);
            }

            Player.Instance.PartsManager.OnColorChange -= OnChangeColor;
            Player.Instance.PartsManager.OnColorChange += OnChangeColor;

            if (canHide)
            {
                objButtonReset.SetActive(true);
                objButtonReset.transform.SetAsFirstSibling();
            }
            else
            {
                objButtonReset.SetActive(false);
            }

            SetFrame();
        }

        /// <summary>
        /// 슬롯 선택
        /// Select slot
        /// </summary>
        /// <param name="slot">슬롯 / Slot</param>
        public void SelectSlot(PartsListSlot slot)
        {
            Player.Instance.PartsManager.EquipParts(_activePartsType, slot.SlotIndex);
            StartCoroutine(SetFrameCo());
        }
        
        /// <summary>
        /// 닫기 버튼 클릭
        /// Click close button
        /// </summary>
        public void OnClick_Close()
        {
            Close();
            AudioManager.Instance.PlaySound(SoundList.ButtonClose);
            DemoControl.Instance.PanelParts.UnselectSlot();
        }

        /// <summary>
        /// 리셋 버튼 클릭
        /// Click reset button
        /// </summary>
        public void OnClick_Reset()
        {
            Player.Instance.PartsManager.EquipParts(_activePartsType, -1);
            SetFrame();
        }
        #endregion

        
        
        
        
        #region Private
        
        /// <summary>
        /// 부품 유형 변경
        /// Change part type
        /// </summary>
        /// <param name="type">부품 유형 / Part type</param>
        /// <param name="index">인덱스 / Index</param>
        private void ChangePartType(PartsType type, int index)
        {
            if(_activePartsType != type) return;
            SetFrame();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnClick_Close();
            }

            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, _targetPos, ref _velocity, 0.1f);
        }
        
        /// <summary>
        /// 색상 변경
        /// Change color
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <param name="color">색상 / Color</param>
        private void OnChangeColor(PartsType partsType, Color color)
        {
            if (DemoControl.CanChangeColor(partsType))
            {
                ChangeColorList(color);
            }
        }

        /// <summary>
        /// 색상 목록 변경
        /// Change color list
        /// </summary>
        /// <param name="color">색상 / Color</param>
        private void ChangeColorList(Color color)
        {
            foreach (var t in _activePartsList)
            {
                t.ChangeColor(color);
            }
        }

        /// <summary>
        /// 프레임 설정
        /// Set frame
        /// </summary>
        private void SetFrame()
        {
            if(!IsShow) return;
            StartCoroutine(SetFrameCo());
        }
        
        /// <summary>
        /// 프레임 설정 코루틴
        /// Set frame coroutine
        /// </summary>
        /// <returns>코루틴 / Coroutine</returns>
        private IEnumerator SetFrameCo()
        {
            yield return null;
            
            var slotIndex = Player.Instance.PartsManager.GetCurrentPartIndex(_activePartsType);
            if (slotIndex >= 0)
            {
                imgSelectFrame.transform.localPosition = _activePartsList[slotIndex].transform.localPosition;
            }
            
            imgSelectFrame.gameObject.SetActive(Player.Instance.PartsManager.GetCurrentPartIndex(_activePartsType) >= 0);
        }

        /// <summary>
        /// 패널 닫기
        /// Close panel
        /// </summary>
        private void Close()
        {
            Player.Instance.PartsManager.OnColorChange -= OnChangeColor;
            _targetPos.x = _rect.sizeDelta.x;
            transform.localPosition = _targetPos;
            gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            Player.Instance.PartsManager.OnColorChange -= OnChangeColor;
            Player.Instance.PartsManager.OnChangedParts -= ChangePartType;
            
        }
        #endregion
    }
}