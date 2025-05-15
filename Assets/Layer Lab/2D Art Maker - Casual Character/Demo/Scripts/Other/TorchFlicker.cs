using UnityEngine;

namespace LayerLab.Casual2DCharacters.Forge
{
    public class TorchFlicker : MonoBehaviour
    {
        public Light torchLight;
        public float minIntensity = 1.5f;
        public float maxIntensity = 2.5f;
        public float flickerSpeed = 3f;
        public float moveAmount = 0.1f;

        private Vector3 _initialPosition;

        
        /// <summary>
        /// 초기화 및 시작 위치 저장
        /// Initialize and save starting position
        /// </summary>
        private void Start()
        {
            _initialPosition = transform.position;
        }

        /// <summary>
        /// 불꽃 깜빡임 및 위치 변경 업데이트
        /// Update torch flicker and position movement
        /// </summary>
        private void Update()
        {
            torchLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.PerlinNoise(Time.time * flickerSpeed, 0) + 1) / 2);
            transform.position = _initialPosition + new Vector3(Mathf.PerlinNoise(0, Time.time * flickerSpeed) * moveAmount - moveAmount / 2, Mathf.PerlinNoise(Time.time * flickerSpeed, 0) * moveAmount - moveAmount / 2, 0);
        }
    }
}