using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace MainMenu
{
    public class OptionsMenu : MonoBehaviour
    {
        public AudioMixer audioMixer;

        public string masterVolumeParameter;
        public Slider masterVolumeSlider;
        public string musicVolumeParameter;
        public Slider musicVolumeSlider;
        public string soundVolumeParameter;
        public Slider soundVolumeSlider;

        private void Start()
        {
            if (audioMixer == null) return;

            if (masterVolumeSlider != null && !string.IsNullOrEmpty(masterVolumeParameter))
            {
                audioMixer.GetFloat(masterVolumeParameter, out var value);
                masterVolumeSlider.value = DecibelToLinear(value);
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null && !string.IsNullOrEmpty(musicVolumeParameter))
            {
                audioMixer.GetFloat(musicVolumeParameter, out var value);
                musicVolumeSlider.value = DecibelToLinear(value);
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (soundVolumeSlider != null && !string.IsNullOrEmpty(soundVolumeParameter))
            {
                audioMixer.GetFloat(soundVolumeParameter, out var value);
                soundVolumeSlider.value = DecibelToLinear(value);
                soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
            }
        }

        public void OnClickOk()
        {
            Destroy(gameObject);
        }

        private void OnMasterVolumeChanged(float value)
        {
            audioMixer.SetFloat(masterVolumeParameter, LinearToDecibel(value));
        }

        private void OnMusicVolumeChanged(float value)
        {
            audioMixer.SetFloat(musicVolumeParameter, LinearToDecibel(value));
        }

        private void OnSoundVolumeChanged(float value)
        {
            audioMixer.SetFloat(soundVolumeParameter, LinearToDecibel(value));
        }

        private static float DecibelToLinear(float db)
        {
            return Mathf.Pow(10.0f, db / 20.0f);
        }

        private static float LinearToDecibel(float linear)
        {
            if (linear == 0f)
                return -120f;

            return Mathf.Log10(linear) * 20f;
        }
    }
}
