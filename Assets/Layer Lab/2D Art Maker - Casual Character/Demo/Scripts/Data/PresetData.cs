using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayerLab.Casual2DCharacters.Forge
{
    [CreateAssetMenu(fileName = "PresetData", menuName = "Character/PresetData")]
    public class PresetData : ScriptableObject
    {
        public List<PresetItem> presetItems = new();

        public void SavePreset(int index, Dictionary<PartsType, int> itemList, Dictionary<string, Color> colorData)
        {
            presetItems.RemoveAll(p => p.index == index);
            presetItems.Add(new PresetItem(index, itemList, colorData));
        }

        public Dictionary<PartsType, int> LoadPreset(int index)
        {
            var preset = presetItems.Find(p => p.index == index);
            return preset != null ? new Dictionary<PartsType, int>(preset.itemList) : new Dictionary<PartsType, int>();
        }

        public Dictionary<string, Color> LoadPresetColors(int index)
        {
            var preset = presetItems.Find(p => p.index == index);
            return preset != null ? new Dictionary<string, Color>(preset.colorData) : new Dictionary<string, Color>();
        }

        public void ClearPreset(int index)
        {
            presetItems.RemoveAll(p => p.index == index);
        }
    }

    [Serializable]
    public class PresetItem
    {
        public int index;
        public List<PartItem> parts = new();
        public List<ColorItem> colors = new();

        public Dictionary<PartsType, int> itemList
        {
            get
            {
                Dictionary<PartsType, int> dict = new();
                foreach (var part in parts)
                {
                    dict[part.partType] = part.value;
                }
                return dict;
            }
        }

        public Dictionary<string, Color> colorData
        {
            get
            {
                Dictionary<string, Color> dict = new();
                foreach (var color in colors)
                {
                    dict[color.slotName] = color.color;
                }
                return dict;
            }
        }

        public PresetItem(int index, Dictionary<PartsType, int> itemList, Dictionary<string, Color> colorData)
        {
            this.index = index;
            foreach (var kvp in itemList)
            {
                parts.Add(new PartItem(kvp.Key, kvp.Value));
            }
            
            if (colorData != null)
            {
                foreach (var kvp in colorData)
                {
                    colors.Add(new ColorItem(kvp.Key, kvp.Value));
                }
            }
        }
    }

    [Serializable]
    public class PartItem
    {
        public PartsType partType;
        public int value;

        public PartItem(PartsType partType, int value)
        {
            this.partType = partType;
            this.value = value;
        }
    }

    [Serializable]
    public class ColorItem
    {
        public string slotName;
        public Color color;

        public ColorItem(string slotName, Color color)
        {
            this.slotName = slotName;
            this.color = color;
        }
    }
}