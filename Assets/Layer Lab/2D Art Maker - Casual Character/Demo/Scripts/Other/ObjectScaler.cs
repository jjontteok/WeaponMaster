using UnityEngine;

namespace LayerLab.Casual2DCharacters.Forge
{
    public class ObjectScaler : MonoBehaviour
    {
        private void Start()
        {
            var screenRatio = (float)Screen.width / Screen.height;
            const float targetRatio = 1920f / 1080f;
            var scale = screenRatio / targetRatio;
            transform.localScale = scale >= 1f ? new Vector3(scale, scale, 1f) : new Vector3(1f, 1f, 1f);
        }
    }
}
