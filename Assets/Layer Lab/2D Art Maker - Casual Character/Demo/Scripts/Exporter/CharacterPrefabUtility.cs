using UnityEditor;
using UnityEngine;

namespace LayerLab.Casual2DCharacters.Forge
{
#if UNITY_EDITOR
    public class CharacterPrefabUtility : MonoBehaviour
    {
        [ContextMenu("Create Self-Contained Character Prefab")]
        public void CreateCharacterPrefab()
        {
            
            var characterObj = Player.Instance.PartsManager.gameObject;
            if (characterObj == null)
            {
                Debug.LogError("Character Instance not found.");
                return;
            }
            
            var sourcePartsManager = Player.Instance.PartsManager;
            if (sourcePartsManager == null)
            {
                Debug.LogError("PartsManager not found on the source character.");
                return;
            }

            if (sourcePartsManager.ActiveIndices == null || sourcePartsManager.ActiveIndices.Count == 0)
            {
                Debug.LogError("No active indices found on the source PartsManager.");
                return;
            }

            var prefabObj = Instantiate(characterObj);
            prefabObj.name = characterObj.name + "_Prefab";  // null 에러 방지를 위해 이름 설정

            var prefabData = prefabObj.GetComponent<CharacterPrefabData>();
            if (prefabData == null)
            {
                prefabData = prefabObj.AddComponent<CharacterPrefabData>();
            }

            prefabData.skinParts.Clear();
            prefabData.slotColors.Clear();

            Debug.Log($"Source ActiveIndices count: {sourcePartsManager.ActiveIndices.Count}");

            
            foreach (PartsType partType in System.Enum.GetValues(typeof(PartsType)))
            {
                if (partType == PartsType.None) continue;

                if (sourcePartsManager.ActiveIndices.ContainsKey(partType))
                {
                    int index = sourcePartsManager.ActiveIndices[partType];
                    Debug.Log($"Adding part: {partType} with index: {index}");

                    var partData = new CharacterPrefabData.SkinPartData
                    {
                        partType = partType,
                        selectedIndex = index,
                        isHidden = false
                    };
                    prefabData.skinParts.Add(partData);
                }
            }

            try
            {
                var hairColor = sourcePartsManager.GetColorBySlotType("hair");
                var beardColor = sourcePartsManager.GetColorBySlotType("beard");
                var browColor = sourcePartsManager.GetColorBySlotType("brow");

                Debug.Log($"Hair color: {hairColor}, Beard color: {beardColor}, Brow color: {browColor}");

                prefabData.slotColors.Add(new CharacterPrefabData.SlotColorData
                {
                    slotName = "hair",
                    color = hairColor
                });
                prefabData.slotColors.Add(new CharacterPrefabData.SlotColorData
                {
                    slotName = "beard",
                    color = beardColor
                });
                prefabData.slotColors.Add(new CharacterPrefabData.SlotColorData
                {
                    slotName = "brow",
                    color = browColor
                });
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting colors: {e.Message}");
            }

            var prefabPath = "Assets/CharacterPrefabs/CharacterSelfContained_" + 
                System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".prefab";
            
            if (!AssetDatabase.IsValidFolder("Assets/CharacterPrefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "CharacterPrefabs");
            }

            Debug.Log($"Saving prefab with {prefabData.skinParts.Count} skin parts and {prefabData.slotColors.Count} colors");
            
            if (prefabObj == null)
            {
                Debug.LogError("Prefab object is null before saving!");
                return;
            }

            var savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabObj, prefabPath, out var success);
            
            if (success && savedPrefab != null)
            {
                Debug.Log($"The character prefab has been created: {prefabPath}");
                
                EditorUtility.SetDirty(savedPrefab);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("Failed to save prefab!");
            }

            if (Application.isPlaying)
            {
                Destroy(prefabObj);
            }
            else
            {
                DestroyImmediate(prefabObj);
            }

            AssetDatabase.Refresh();
            Selection.activeObject = savedPrefab;
            Object loadPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            EditorGUIUtility.PingObject(loadPrefab);
        }
    }
#endif
}