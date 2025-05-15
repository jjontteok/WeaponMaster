using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LayerLab.Casual2DCharacters.Forge
{
    public class PresetSlot : MonoBehaviour, IPointerClickHandler
    {
        [field: SerializeField] private PartsManager PartsManager { get; set; }
        [SerializeField] private bool useSave;
        private int _slotIndex;
        
        private readonly Color _defaultHairColor = new Color(0.5f, 0.5f, 0.5f);
        private readonly Color _defaultBeardColor = new Color(0.5f, 0.5f, 0.5f);
        private readonly Color _defaultBrowColor = new Color(0.5f, 0.5f, 0.5f);
        
        /// <summary>
        /// 초기화
        /// Initialize
        /// </summary>
        /// <param name="index">인덱스 / Index</param>
        public void Init(int index)
        {
            _slotIndex = index;
            PartsManager = transform.GetComponentInChildren<PartsManager>();
            PartsManager.Init();
            LoadData();
        }

        /// <summary>
        /// 프리셋 데이터 로드
        /// Load preset data
        /// </summary>
        private void LoadData()
        {
            var dic = DemoControl.Instance.PresetData.LoadPreset(_slotIndex);
            if (dic != null && dic.Count > 0) 
            {
                PartsManager.SetSkinActiveIndex(dic);
            }
            
            var colorData = DemoControl.Instance.PresetData.LoadPresetColors(_slotIndex);
            if (colorData != null && colorData.Count > 0)
            {
                if (colorData.TryGetValue("hair", out Color hairColor))
                {
                    PartsManager.ChangeHairColor(hairColor);
                }
                else
                {
                    PartsManager.ChangeHairColor(_defaultHairColor);
                }
                
                if (colorData.TryGetValue("beard", out Color beardColor))
                {
                    PartsManager.ChangeBeardColor(beardColor);
                }
                else
                {
                    PartsManager.ChangeBeardColor(_defaultBeardColor);
                }
                
                if (colorData.TryGetValue("brow", out Color browColor))
                {
                    PartsManager.ChangeBrowColor(browColor);
                }
                else
                {
                    PartsManager.ChangeBrowColor(_defaultBrowColor);
                }
            }
            else
            {
                PartsManager.ChangeHairColor(_defaultHairColor);
                PartsManager.ChangeBeardColor(_defaultBeardColor);
                PartsManager.ChangeBrowColor(_defaultBrowColor);
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left: //프리셋 데이터 장착

                    DemoControl.Instance.PanelParts.PanelPartsList.OnClick_Close();
                    
                    ApplyAllPartsToCharacter();
                    
                    var colorData = LoadColorDataFromPreset();
                    if (colorData != null && colorData.Count > 0)
                    {
                        if (colorData.TryGetValue("hair", out Color hairColor))
                        {
                            Player.Instance.PartsManager.ChangeHairColor(hairColor);
                            Player.Instance.PartsManager.OnColorChange.Invoke(PartsType.Hair_Short, hairColor);
                        }
                        
                        if (colorData.TryGetValue("beard", out Color beardColor))
                        {
                            Player.Instance.PartsManager.ChangeBeardColor(beardColor);
                            Player.Instance.PartsManager.OnColorChange.Invoke(PartsType.Beard, hairColor);
                        }
                        
                        if (colorData.TryGetValue("brow", out Color browColor))
                        {
                            Player.Instance.PartsManager.ChangeBrowColor(browColor);
                            Player.Instance.PartsManager.OnColorChange.Invoke(PartsType.Brow, hairColor);
                        }
                    }
                    else
                    {
                        Player.Instance.PartsManager.ChangeHairColor(PartsManager.GetColorBySlotType("hair"));
                        Player.Instance.PartsManager.ChangeBeardColor(PartsManager.GetColorBySlotType("beard"));
                        Player.Instance.PartsManager.ChangeBrowColor(PartsManager.GetColorBySlotType("brow"));
                    }
                    break;
                    
                case PointerEventData.InputButton.Right: //프리셋에 저장
                    if (!useSave) return;
                    
                    ApplyAllPartsFromCharacterToPreset();
                    
                    PartsManager.ChangeHairColor(Player.Instance.PartsManager.GetColorBySlotType("hair"));
                    PartsManager.ChangeBeardColor(Player.Instance.PartsManager.GetColorBySlotType("beard"));
                    PartsManager.ChangeBrowColor(Player.Instance.PartsManager.GetColorBySlotType("brow"));
                    
                    SavePresetData();
                    break;
            }
        }
        
        /// <summary>
        /// 프리셋에서 색상 데이터 로드
        /// Load color data from preset
        /// </summary>
        /// <returns>색상 데이터 / Color data</returns>
        private Dictionary<string, Color> LoadColorDataFromPreset()
        {
            return DemoControl.Instance.PresetData.LoadPresetColors(_slotIndex);
        }
        
        /// <summary>
        /// 프리셋에서 캐릭터로 모든 부품 적용
        /// Apply all parts from preset to character
        /// </summary>
        private void ApplyAllPartsToCharacter()
        {
            Player.Instance.PartsManager.EquipParts(PartsType.Back, PartsManager.GetCurrentPartIndex(PartsType.Back));
            Player.Instance.PartsManager.EquipParts(PartsType.Beard, PartsManager.GetCurrentPartIndex(PartsType.Beard));
            Player.Instance.PartsManager.EquipParts(PartsType.Boots, PartsManager.GetCurrentPartIndex(PartsType.Boots));
            Player.Instance.PartsManager.EquipParts(PartsType.Bottom, PartsManager.GetCurrentPartIndex(PartsType.Bottom));
            Player.Instance.PartsManager.EquipParts(PartsType.Brow, PartsManager.GetCurrentPartIndex(PartsType.Brow));
            Player.Instance.PartsManager.EquipParts(PartsType.Eyes, PartsManager.GetCurrentPartIndex(PartsType.Eyes));
            Player.Instance.PartsManager.EquipParts(PartsType.Gloves, PartsManager.GetCurrentPartIndex(PartsType.Gloves));
            Player.Instance.PartsManager.EquipParts(PartsType.Hair_Short, PartsManager.GetCurrentPartIndex(PartsType.Hair_Short));
            Player.Instance.PartsManager.EquipParts(PartsType.Helmet, PartsManager.GetCurrentPartIndex(PartsType.Helmet));
            Player.Instance.PartsManager.EquipParts(PartsType.Mouth, PartsManager.GetCurrentPartIndex(PartsType.Mouth));
            Player.Instance.PartsManager.EquipParts(PartsType.Eyewear, PartsManager.GetCurrentPartIndex(PartsType.Eyewear));
            Player.Instance.PartsManager.EquipParts(PartsType.Gear_Left, PartsManager.GetCurrentPartIndex(PartsType.Gear_Left));
            Player.Instance.PartsManager.EquipParts(PartsType.Gear_Right, PartsManager.GetCurrentPartIndex(PartsType.Gear_Right));
            Player.Instance.PartsManager.EquipParts(PartsType.Top, PartsManager.GetCurrentPartIndex(PartsType.Top));
            Player.Instance.PartsManager.EquipParts(PartsType.Skin, PartsManager.GetCurrentPartIndex(PartsType.Skin));
        }
        
        /// <summary>
        /// 캐릭터에서 프리셋으로 모든 부품 적용
        /// Apply all parts from character to preset
        /// </summary>
        private void ApplyAllPartsFromCharacterToPreset()
        {
            PartsManager.EquipParts(PartsType.Back, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Back));
            PartsManager.EquipParts(PartsType.Beard, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Beard));
            PartsManager.EquipParts(PartsType.Boots, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Boots));
            PartsManager.EquipParts(PartsType.Bottom, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Bottom));
            PartsManager.EquipParts(PartsType.Brow, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Brow));
            PartsManager.EquipParts(PartsType.Eyes, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Eyes));
            PartsManager.EquipParts(PartsType.Gloves, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Gloves));
            PartsManager.EquipParts(PartsType.Hair_Short, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Hair_Short));
            PartsManager.EquipParts(PartsType.Helmet, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Helmet));
            PartsManager.EquipParts(PartsType.Mouth, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Mouth));
            PartsManager.EquipParts(PartsType.Eyewear, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Eyewear));
            PartsManager.EquipParts(PartsType.Gear_Left, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Gear_Left));
            PartsManager.EquipParts(PartsType.Gear_Right, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Gear_Right));
            PartsManager.EquipParts(PartsType.Top, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Top));
            PartsManager.EquipParts(PartsType.Skin, Player.Instance.PartsManager.GetCurrentPartIndex(PartsType.Skin));
        }
        
        /// <summary>
        /// 프리셋 저장
        /// Save preset data
        /// </summary>
        private void SavePresetData()
        {
            var colorData = new Dictionary<string, Color>();
            var hairColor = Player.Instance.PartsManager.GetColorBySlotType("hair");
            var beardColor = Player.Instance.PartsManager.GetColorBySlotType("beard");
            var browColor = Player.Instance.PartsManager.GetColorBySlotType("brow");
            
            colorData.Add("hair", hairColor);
            colorData.Add("beard", beardColor);
            colorData.Add("brow", browColor);
            
            DemoControl.Instance.PresetData.SavePreset(_slotIndex, Player.Instance.PartsManager.ActiveIndices, colorData);
        }
    }
}