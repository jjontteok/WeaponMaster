using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LayerLab.Casual2DCharacters.Forge
{
    public class PanelPreset : MonoBehaviour
    {
        private List<PresetSlot> _presetSlots = new();

        public void Init()
        {
            var index = 0;
            _presetSlots = transform.GetComponentsInChildren<PresetSlot>().ToList();
            foreach (var t in _presetSlots)
            {
                t.Init(index);
                index++;
            }
            
          
            DemoControl.Instance.OnPlayMode += CheckMode;
        }

        private void CheckMode(PlayMode playMode)
        {
            gameObject.SetActive(playMode == PlayMode.Home);
        }

        private void OnDestroy()
        {
            DemoControl.Instance.OnPlayMode -= CheckMode;
        }
    }
}