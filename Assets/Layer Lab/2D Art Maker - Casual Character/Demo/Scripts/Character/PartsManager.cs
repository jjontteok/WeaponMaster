using System;
using System.Collections.Generic;
using System.Linq;
using Spine;
using Spine.Unity;
using Spine.Unity.AttachmentTools;
using UnityEngine;
using AnimationState = Spine.AnimationState;
using Random = UnityEngine.Random;

namespace LayerLab.Casual2DCharacters.Forge
{
    public enum PartsType
    {
        None,
        Back,
        Beard,
        Boots,
        Bottom,
        Brow,
        Eyes,
        Gloves,
        Hair_Short,
        Hair_Hat,
        Helmet,
        Mouth,
        Eyewear,
        Gear_Left,
        Gear_Right,
        Top,
        Skin,
    }

    public class PartsManager : MonoBehaviour
    {
        [SpineSkin] public List<string> back = new();
        [SpineSkin] public List<string> beard = new();
        [SpineSkin] public List<string> boots = new();
        [SpineSkin] public List<string> bottom = new();
        [SpineSkin] public List<string> brow = new();
        [SpineSkin] public List<string> eye = new();
        [SpineSkin] public List<string> gloves = new();
        [SpineSkin] public List<string> hairShort = new();
        [SpineSkin] public List<string> hairHat = new();
        [SpineSkin] public List<string> helmet = new();
        [SpineSkin] public List<string> mouth = new();
        [SpineSkin] public List<string> eyewear = new();
        [SpineSkin] public List<string> gearLeft = new();
        [SpineSkin] public List<string> gearRight = new();
        [SpineSkin] public List<string> top = new();
        [SpineSkin] public List<string> skin = new();
        public Dictionary<PartsType, int> ActiveIndices { get; private set; } = new();
        public Material runtimeMaterial;
        public Texture2D runtimeAtlas;
        
        public Action<PartsType, int> OnChangedParts { get; set; }
        public Action<PartsType, bool> OnPartsVisibilityChanged { get; set; }
        public Action<PartsType, Color> OnColorChange { get; set; }
        
        private readonly Dictionary<PartsType, bool> _hideStatus = new();
        private Dictionary<PartsType, List<string>> _skinTypeToListMapping;
        private ISkeletonAnimation _skeletonComponent;
        private Skeleton _skeleton;
        private AnimationState _animationState;
        private Dictionary<string, Color> CustomSlotColors { get; } = new ();
        private Skin _characterSkin;
        private bool _hasCustomColors = false;
        [SerializeField] private SkeletonAnimation skeletonAnimation;
        [SerializeField] private SkeletonGraphic skeletonGraphic;
        [field: SerializeField] private bool IsOptimizeSkin { get; set; }
        
        private void OnValidate()
        {
            if (skeletonAnimation == null && skeletonGraphic == null)
            {
                skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
                if (skeletonAnimation == null)
                {
                    skeletonGraphic = GetComponentInChildren<SkeletonGraphic>();
                }
            }
        }

        
        /// <summary>
        /// 초기화
        /// Initialization
        /// </summary>
        public void Init()
        {
            SetSkin();
            
            if (skeletonAnimation != null)
            {
                _skeletonComponent = skeletonAnimation;
                _skeleton = skeletonAnimation.Skeleton;
                _animationState = skeletonAnimation.AnimationState;
            }
            else if (skeletonGraphic != null)
            {
                _skeletonComponent = skeletonGraphic;
                _skeleton = skeletonGraphic.Skeleton;
                _animationState = skeletonGraphic.AnimationState;
            }
            else
            {
                Debug.LogError("PartsManager에 SkeletonAnimation 또는 SkeletonGraphic이 없습니다!");
                return;
            }

            InitializeMappings();
            InitializeSkinIndices();
            UpdateCharacter();
        }

        
        /// <summary>
        /// 스킨 인덱스 초기화
        /// Initialize skin indices
        /// </summary>
        private void InitializeMappings()
        {
            // 스파인 매핑 재설정
            // Reset Spine Skin Type Mapping
            _skinTypeToListMapping = new Dictionary<PartsType, List<string>>()
            {
                { PartsType.Back, back },
                { PartsType.Beard, beard },
                { PartsType.Boots, boots },
                { PartsType.Bottom, bottom },
                { PartsType.Brow, brow },
                { PartsType.Eyes, eye },
                { PartsType.Gloves, gloves },
                { PartsType.Hair_Short, hairShort },
                { PartsType.Hair_Hat, hairHat },
                { PartsType.Helmet, helmet },
                { PartsType.Mouth, mouth },
                { PartsType.Eyewear, eyewear },
                { PartsType.Gear_Left, gearLeft },
                { PartsType.Gear_Right, gearRight },
                { PartsType.Top, top },
                { PartsType.Skin, skin }
            };

            foreach (PartsType partsType in Enum.GetValues(typeof(PartsType)))
            {
                if (partsType != PartsType.None)
                {
                    _hideStatus[partsType] = false;
                }
            }
        }

        /// <summary>
        /// 스킨 인덱스 초기화
        /// Initialize skin indices
        /// </summary>
        private void InitializeSkinIndices()
        {
            foreach (PartsType skinType in Enum.GetValues(typeof(PartsType)))
            {
                if (skinType != PartsType.None && _skinTypeToListMapping.ContainsKey(skinType))
                {
                    var skinList = _skinTypeToListMapping[skinType];
                    ActiveIndices[skinType] = skinList.Count > 0 ? 0 : -1;
                }
            }
        }

        /// <summary>
        /// 스킨 활성화 인덱스 설정
        /// Set skin activation index
        /// </summary>
        /// <param name="indexList">인덱스 목록 / Index list</param>
        public void SetSkinActiveIndex(Dictionary<PartsType, int> indexList)
        {
            if (indexList.Count <= 0) return;
            ActiveIndices = indexList;
            UpdateCharacter();
        }

        /// <summary>
        /// 부품 유형이 빈 슬롯인지 확인
        /// Check if the part type is an empty slot
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <returns>빈 슬롯 여부 / Empty slot status</returns>
        public bool IsEmptyItemByType(PartsType partsType)
        {
            return GetCurrentPartIndex(partsType) < 0 ||
                   (_skinTypeToListMapping.ContainsKey(partsType) && _skinTypeToListMapping[partsType].Count == 0);
        }

        /// <summary>
        /// 현재 부품 인덱스 반환
        /// Return the current part index
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <returns>부품 인덱스 / Part index</returns>
        public int GetCurrentPartIndex(PartsType partsType)
        {
            return ActiveIndices.ContainsKey(partsType) ? ActiveIndices[partsType] : -1;
        }

        /// <summary>
        /// 현재 스킨 이름 목록 반환
        /// Return the current list of skin names
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <returns>스킨 이름 목록 / Skin name list</returns>
        public List<string> GetCurrentSkinNames(PartsType partsType)
        {
            if (partsType == PartsType.None || !_skinTypeToListMapping.ContainsKey(partsType))
                return null;

            return _skinTypeToListMapping[partsType];
        }

        /// <summary>
        /// 현재 부품 이름 반환
        /// Return the current part name
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <returns>부품 이름 / Part name</returns>
        public string GetCurrentPartsName(PartsType partsType)
        {
            if (partsType == PartsType.None || !_skinTypeToListMapping.ContainsKey(partsType))
                return string.Empty;

            var skinList = _skinTypeToListMapping[partsType];
            var index = ActiveIndices[partsType];

            return (index >= 0 && index < skinList.Count) ? skinList[index] : string.Empty;
        }

        
        /// <summary>
        /// 스킨 데이터 설정
        /// Set skin data
        /// </summary>
        public void SetSkin()
        {
            if (skeletonAnimation)
            {
                _skeleton =  skeletonAnimation.Skeleton;   
            }

            if (skeletonGraphic)
            {
                _skeleton =  skeletonGraphic.Skeleton;
            }
            
            if (_skeleton == null || _skeleton.Data == null)
            {
                
                Debug.LogError("No skeleton or skeleton data!");
                return;
            }

            foreach (PartsType skinType in Enum.GetValues(typeof(PartsType)))
            {
                switch (skinType)
                {
                    case PartsType.None: break;
                    case PartsType.Back: back = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Beard: beard = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Boots: boots = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Bottom: bottom = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Brow: brow = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Eyes: eye = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Gloves: gloves = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Hair_Short: hairShort = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Hair_Hat: hairHat = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Helmet: helmet = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Mouth: mouth = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Eyewear: eyewear = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Gear_Left: gearLeft = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Gear_Right: gearRight = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Top: top = GetCategorySkins(_skeleton.Data, skinType); break;
                    case PartsType.Skin: skin = GetCategorySkins(_skeleton.Data, skinType); break;
                }
            }
        }

        /// <summary>
        /// 카테고리별 스킨 목록 가져오기
        /// Get list of skins by category
        /// </summary>
        /// <param name="data">스켈레톤 데이터 / Skeleton data</param>
        /// <param name="category">카테고리 / Category</param>
        /// <returns>스킨 목록 / Skin list</returns>
        private List<string> GetCategorySkins(SkeletonData data, PartsType category)
        {
            return (from s in data.Skins where s.Name.StartsWith($"{category.ToString().ToLower()}") select s.Name).ToList();
        }

        /// <summary>
        /// 랜덤 부품 생성
        /// Generate random parts
        /// </summary>
        public void RandomParts()
        {
            var allParts = DemoControl.Instance.PanelParts.partsSlots;
            for (int i = 0; i < allParts.Length; i++)
            {
                if (allParts[i].CanHide)
                {
                    if (allParts[i].PartsType == PartsType.None) continue;
                    var partsIndex = Random.value < 0.3f ? -1 : Random.Range(0, GetCurrentSkinNames(allParts[i].PartsType).Count);
                    ActiveIndices[allParts[i].PartsType] = partsIndex;
                    OnChangedParts?.Invoke(allParts[i].PartsType, partsIndex);
                }
                else
                {
                    if (allParts[i].PartsType == PartsType.None) continue;

                    var index = Random.Range(0, GetCurrentSkinNames(allParts[i].PartsType).Count);
                    ActiveIndices[allParts[i].PartsType] = index;
                    OnChangedParts?.Invoke(allParts[i].PartsType, index);
                }
            }

            UpdateCharacter();
        }

        
        /// <summary>
        /// 부품 장착
        /// Equip part
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <param name="index">부품 인덱스 / Part index</param>
        public void EquipParts(PartsType partsType, int index)
        {
            ActiveIndices[partsType] = index;
            if (partsType == PartsType.Helmet)
            {
                if (index >= 0 && hairHat.Count > 0) ActiveIndices[PartsType.Hair_Hat] = 0; 
            }
            
            OnChangedParts?.Invoke(partsType, GetCurrentPartIndex(partsType));
            UpdateCharacter();
        }

        
        private void LateUpdate()
        {
            if (_hasCustomColors && _animationState != null && _animationState.GetCurrent(0) != null)
            {
                ApplyCustomColors();
            }
        }

        
        /// <summary>
        /// 아이템 숨기기
        /// Hide item
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <param name="isHide">숨김 여부 / Hide status</param>
        public void SetHideItem(PartsType partsType, bool isHide)
        {
            if (partsType != PartsType.None)
            {
                OnPartsVisibilityChanged?.Invoke(partsType, isHide);
                _hideStatus[partsType] = isHide;
                UpdateCharacter();
            }
        }

        /// <summary>
        /// 캐릭터 업데이트
        /// Update character
        /// </summary>
        public void UpdateCharacter()
        {
            UpdateCharacterSkin();
            UpdateCombinedSkin();
            if (IsOptimizeSkin) OptimizeSkin();
        }


        /// <summary>
        /// 캐릭터 스킨 업데이트
        /// Update character skin
        /// </summary>
        private void UpdateCharacterSkin()
        {
            if (_skeleton == null || _skeleton.Data == null) return;

            var skeletonData = _skeleton.Data;
            _characterSkin = new Skin("character-base");

           
            
            bool isHelmetEquipped = !IsEmptyItemByType(PartsType.Helmet) && !_hideStatus[PartsType.Helmet];

            ActiveIndices[PartsType.Hair_Hat] = ActiveIndices[PartsType.Hair_Short];
            
            foreach (var skinType in _skinTypeToListMapping.Keys)
            {
                if (skinType == PartsType.None) continue;
                if (skinType == PartsType.Hair_Short && isHelmetEquipped) continue;
                if (skinType == PartsType.Hair_Hat && !isHelmetEquipped) continue;
                
                
                var index = ActiveIndices[skinType];
                var isHidden = _hideStatus[skinType];
                var skinList = _skinTypeToListMapping[skinType];

                if (index >= 0 && index < skinList.Count && !isHidden)
                {
                    var skinName = skinList[index];
                    var skinData = skeletonData.FindSkin(skinName);
                    if (skinData != null)
                    {
                        _characterSkin.AddSkin(skinData);
                    }
                }
            }
            
            
        }

        
        /// <summary>
        /// 합성된 스킨 업데이트
        /// Update combined skin
        /// </summary>
        private void UpdateCombinedSkin()
        {
            if (_skeleton == null) return;

            Skin resultCombinedSkin = new("character-combined");
            resultCombinedSkin.AddSkin(_characterSkin);
            _skeleton.SetSkin(resultCombinedSkin);
            _skeleton.SetSlotsToSetupPose();

            if (_hasCustomColors)
            {
                ApplyCustomColors();
            }
        }
        
        
        /// <summary>
        /// 색상 적용
        /// Apply colors
        /// </summary>
        private void ApplyCustomColors()
        {
            if (_skeleton == null) return;

            foreach (var pair in CustomSlotColors)
            {
                var slot = _skeleton.FindSlot(pair.Key);
                if (slot != null)
                {
                    slot.SetColor(pair.Value);
                }
            }
        }

        /// <summary>
        /// 다음 아이템
        /// Next item
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        public void NextItem(PartsType partsType)
        {
            if (partsType == PartsType.None || !_skinTypeToListMapping.ContainsKey(partsType))
                return;
            
            OnChangedParts?.Invoke(partsType, GetCurrentPartIndex(partsType));
            
            var skinList = _skinTypeToListMapping[partsType];
            if (skinList.Count == 0) return;

            var canHide = IsPartCanHide(partsType);

            if (canHide)
            {
                if (ActiveIndices[partsType] >= skinList.Count - 1)
                    ActiveIndices[partsType] = -1;
                else
                    ActiveIndices[partsType] += 1;
            }
            else
            {
                ActiveIndices[partsType] = (ActiveIndices[partsType] + 1) % skinList.Count;
            }

            UpdateCharacter();
        }

        /// <summary>
        /// 이전 아이템
        /// Previous item
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        public void PrevItem(PartsType partsType)
        {
            if (partsType == PartsType.None || !_skinTypeToListMapping.ContainsKey(partsType))
                return;

            OnChangedParts?.Invoke(partsType, GetCurrentPartIndex(partsType));
            
            var skinList = _skinTypeToListMapping[partsType];
            if (skinList.Count == 0) return;

            bool canHide = IsPartCanHide(partsType);

            if (canHide)
            {
                if (ActiveIndices[partsType] <= -1)
                    ActiveIndices[partsType] = skinList.Count - 1;
                else
                    ActiveIndices[partsType] -= 1;
            }
            else
            {
                ActiveIndices[partsType] = (ActiveIndices[partsType] - 1 + skinList.Count) % skinList.Count;
            }

            UpdateCharacter();
        }

        /// <summary>
        /// 숨김 처리 가능 여부 확인
        /// Check if part can be hidden
        /// </summary>
        /// <param name="partsType">부품 유형 / Parts type</param>
        /// <returns>숨김 가능 여부 / Hideable status</returns>
        private static bool IsPartCanHide(PartsType partsType)
        {
            var partsSlots = DemoControl.Instance.PanelParts.partsSlots;
            foreach (var slot in partsSlots)
            {
                if (slot.PartsType == partsType)
                {
                    return slot.CanHide;
                }
            }

            return false;
        }

        /// <summary>
        /// 스킨 최적화
        /// Optimize skin
        /// </summary>
        public void OptimizeSkin()
        {
            if (_skeleton == null || _skeletonComponent == null) return;

            var previousSkin = _skeleton.Skin;
            if (runtimeMaterial)
                Destroy(runtimeMaterial);
            if (runtimeAtlas)
                Destroy(runtimeAtlas);

            if (skeletonAnimation != null)
            {
                var repackedSkin = previousSkin.GetRepackedSkin("Repacked skin", skeletonAnimation.SkeletonDataAsset.atlasAssets[0].PrimaryMaterial, out runtimeMaterial, out runtimeAtlas);
                previousSkin.Clear();

                _skeleton.Skin = repackedSkin;
                _skeleton.SetSlotsToSetupPose();
                _animationState.Apply(_skeleton);

                AtlasUtilities.ClearCache();
                Resources.UnloadUnusedAssets();
            }
            else if (skeletonGraphic != null)
            {
                var repackedSkin = previousSkin.GetRepackedSkin("Repacked skin", skeletonGraphic.SkeletonDataAsset.atlasAssets[0].PrimaryMaterial, out runtimeMaterial, out runtimeAtlas);
                previousSkin.Clear();

                _skeleton.Skin = repackedSkin;
                _skeleton.SetSlotsToSetupPose();
                _animationState.Apply(_skeleton);

                AtlasUtilities.ClearCache();
                Resources.UnloadUnusedAssets();
            }
        }

        /// <summary>
        /// 머리카락 색상 변경
        /// Change hair color
        /// </summary>
        /// <param name="color">색상 / Color</param>
        public void ChangeHairColor(Color color)
        {
            var hairSlots = GetSlotsWithPrefix("hair");
            foreach (var slot in hairSlots)
            {
                string slotName = slot.Data.Name;
                slot.SetColor(color);
                CustomSlotColors[slotName] = color;
            }

            var hHairSlots = GetSlotsWithPrefix("helmet_hair");
            foreach (var slot in hHairSlots)
            {
                string slotName = slot.Data.Name;
                slot.SetColor(color);
                CustomSlotColors[slotName] = color;
            }
            
            _hasCustomColors = true;
        }

        /// <summary>
        /// 수염 색상 변경
        /// Change beard color
        /// </summary>
        /// <param name="color">색상 / Color</param>
        public void ChangeBeardColor(Color color)
        {
            var beardSlots = GetSlotsWithPrefix("beard");
            foreach (var slot in beardSlots)
            {
                string slotName = slot.Data.Name;
                slot.SetColor(color);
                CustomSlotColors[slotName] = color;
            }

            _hasCustomColors = true;
        }

        /// <summary>
        /// 눈썹 색상 변경
        /// Change eyebrow color
        /// </summary>
        /// <param name="color">색상 / Color</param>
        public void ChangeBrowColor(Color color)
        {
            var browSlots = GetSlotsWithPrefix("brow");
            foreach (var slot in browSlots)
            {
                string slotName = slot.Data.Name;
                slot.SetColor(color);
                CustomSlotColors[slotName] = color;
            }

            _hasCustomColors = true;
        }

        /// <summary>
        /// 슬롯 유형별 색상 가져오기
        /// Get color by slot type
        /// </summary>
        /// <param name="slotName">슬롯 이름 / Slot name</param>
        /// <returns>색상 / Color</returns>
        public Color GetColorBySlotType(string slotName)
        {
            return CustomSlotColors[slotName];
        }

        /// <summary>
        /// 스켈레톤 업데이트
        /// Update skeleton
        /// </summary>
        private void UpdateSkeleton()
        {
            if (_skeleton == null || _animationState == null) return;

            _skeleton.SetSlotsToSetupPose();
            _animationState.Apply(_skeleton);
        }

        /// <summary>
        /// 특정 접두사를 가진 슬롯 찾기
        /// Find slots with a specific prefix
        /// </summary>
        /// <param name="prefix">접두사 / Prefix</param>
        /// <returns>슬롯 목록 / Slot list</returns>
        private List<Slot> GetSlotsWithPrefix(string prefix)
        {
            if (_skeleton == null) return new List<Slot>();

            var result = new List<Slot>();
            foreach (var slot in _skeleton.Slots)
            {
                if (slot.Data.Name.ToLower().StartsWith(prefix.ToLower()))
                {
                    result.Add(slot);
                }
            }

            return result;
        }

             
        /// <summary>
        /// 특정 이름의 애니메이션 재생
        /// Play an animation with a specific name
        /// </summary>
        /// <param name="animationName">애니메이션 이름 / Animation name</param>
        public void PlayAnimation(string animationName)
        {
            skeletonAnimation.AnimationState.SetAnimation(0, animationName, true);
        }
        
        public Skeleton GetSkeleton() => skeletonAnimation.Skeleton;
        public SkeletonAnimation GetSkeletonAnimation() => skeletonAnimation;
   
    }
}