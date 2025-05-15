using UnityEngine;
using Random = UnityEngine.Random;

namespace LayerLab.Casual2DCharacters.Forge
{
    public class SoundList
    {
        public const string Ambient = "amb";
        public const string ButtonArrow = "btn_arrow";
        public const string ButtonClose = "btn_close";
        public const string ButtonDefault = "btn_default";
        public const string ButtonEye = "btn_eye";
        public const string ButtonRandom = "btn_random";

        public static string StepRight ()
        {
            return $"character_step_right{Random.Range(1, 4)}";
        }
        
        public static string StepLeft ()
        {
            return $"character_step_left{Random.Range(1, 4)}";
        }
    }
    
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        private AudioSource _audioEffect;
        private AudioSource _audioAmb;
        private int _soundIndex;
        
        private void Awake()
        {
            Instance = this;
            _audioEffect ??= gameObject.AddComponent<AudioSource>();
            _audioAmb ??= gameObject.AddComponent<AudioSource>();
        }

        private void Start()
        {
            PlayBGM(SoundList.Ambient);
        }

        /// <summary>
        /// 배경음악 재생
        /// Play background music
        /// </summary>
        /// <param name="soundName">사운드 이름 / Sound name</param>
        public void PlayBGM(string soundName)
        {
            var clip = Resources.Load<AudioClip>($"@Sound-Forge/{soundName}");
            if(clip == null) return;
            _audioAmb.clip = clip;
            _audioAmb.loop = true;
            _audioAmb.Play();
        }
        
        /// <summary>
        /// 발자국 사운드 재생
        /// Play footstep sound
        /// </summary>
        public void PlayStepSound()
        {
            _soundIndex++;
            PlaySound(_soundIndex % 2 == 0 ? SoundList.StepLeft() : SoundList.StepRight());
        }
        
        /// <summary>
        /// 사운드 효과 재생
        /// Play sound effect (OneShot)
        /// </summary>
        /// <param name="soundName">사운드 이름 / Sound name</param>
        public void PlaySound(string soundName)
        {
            var clip = Resources.Load<AudioClip>($"@Sound-Forge/{soundName}");
            if(clip == null) return;
            _audioEffect.PlayOneShot(clip);
        }
    }
}