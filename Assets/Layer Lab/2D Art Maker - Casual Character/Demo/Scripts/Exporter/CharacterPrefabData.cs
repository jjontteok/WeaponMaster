 using System;
 using System.Collections.Generic;
 using LayerLab.Casual2DCharacters.Forge;
 using Spine.Unity;
 using UnityEngine;

 namespace LayerLab.Casual2DCharacters.Forge
 {
     public class CharacterPrefabData : MonoBehaviour
     {
         [Serializable]
         public class SkinPartData
         {
             public PartsType partType;
             public int selectedIndex;
             public bool isHidden;
         }

         [Serializable]
         public class SlotColorData
         {
             public string slotName;
             public Color color;
         }

         [SerializeField] public List<SkinPartData> skinParts = new ();
         [SerializeField] public List<SlotColorData> slotColors = new ();

         private PartsManager partsManager;

         private void Awake()
         {
             partsManager = GetComponentInChildren<PartsManager>();
         }

         private void Start()
         {
             ApplySavedSkinData();
         }

         /// <summary>
         /// 저장된 스킨 데이터 적용
         /// Apply Saved Skin Data
         /// </summary>
         public void ApplySavedSkinData()
         {
             if (partsManager == null) return;

             partsManager.Init();

             var activeIndices = new Dictionary<PartsType, int>();

             foreach (var partData in skinParts)
             {
                 activeIndices[partData.partType] = partData.selectedIndex;
                 partsManager.SetHideItem(partData.partType, partData.isHidden);
             }

             partsManager.SetSkinActiveIndex(activeIndices);

             foreach (var colorData in slotColors)
             {
                 if (colorData.slotName.StartsWith("hair"))
                 {
                     partsManager.ChangeHairColor(colorData.color);
                 }
                 else if (colorData.slotName.StartsWith("beard"))
                 {
                     partsManager.ChangeBeardColor(colorData.color);
                 }
                 else if (colorData.slotName.StartsWith("brow"))
                 {
                     partsManager.ChangeBrowColor(colorData.color);
                 }
             }
         }

         /// <summary>
         /// 현재 스킨 데이터를 컴포넌트에 저장
         /// Save current skin data to the component
         /// </summary>
         public void SaveCurrentSkinData()
         {
             if (partsManager == null) return;

             skinParts.Clear();
             slotColors.Clear();

             foreach (var kvp in partsManager.ActiveIndices)
             {
                 bool isHidden = IsPartHidden(kvp.Key);

                 var partData = new SkinPartData
                 {
                     partType = kvp.Key,
                     selectedIndex = kvp.Value,
                     isHidden = isHidden
                 };
                 skinParts.Add(partData);
             }

             AddColorData("hair", partsManager.GetColorBySlotType("hair"));
             AddColorData("beard", partsManager.GetColorBySlotType("beard"));
             AddColorData("brow", partsManager.GetColorBySlotType("brow"));
         }

         private bool IsPartHidden(PartsType partType)
         {
             var hideStatusField = partsManager.GetType()
                 .GetField("_hideStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

             if (hideStatusField != null)
             {
                 var hideStatus = (Dictionary<PartsType, bool>)hideStatusField.GetValue(partsManager);
                 return hideStatus.ContainsKey(partType) && hideStatus[partType];
             }

             return false;
         }

         private void AddColorData(string slotPrefix, Color color)
         {
             slotColors.Add(new SlotColorData
             {
                 slotName = slotPrefix,
                 color = color
             });
         }
     }
 }