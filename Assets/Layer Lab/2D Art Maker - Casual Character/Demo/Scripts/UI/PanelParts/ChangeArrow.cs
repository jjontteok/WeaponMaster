using System;
using UnityEngine;

namespace LayerLab.Casual2DCharacters.Forge
{
    public class ChangeArrow : MonoBehaviour
    {
        public Action OnChangeLeft;
        public Action OnChangeRight;

        public void SetTransform(Transform t)
        {
            transform.SetParent(t, false);
            transform.SetAsFirstSibling();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnClickLeft()
        {
            AudioManager.Instance.PlaySound(SoundList.ButtonArrow);
            OnChangeLeft?.Invoke();
        }

        public void OnClickRight()
        {
            AudioManager.Instance.PlaySound(SoundList.ButtonArrow);
            OnChangeRight?.Invoke();
        }
    }

}