using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class OptionsMenu : MonoBehaviour
    {
        public PreferencesSetter preferencesSetter;

        public TMP_InputField nameField;
        public Slider sensitivitySlider;

        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider effectsVolumeSlider;

        public GameObject firstSelected;

        internal Preferences Preferences { get; set; }

        private void Start()
        {
            if (preferencesSetter == null)
                throw new ArgumentException("OptionsMenu requires AudioPreferencesSetter");

            masterVolumeSlider.value = Preferences.MasterVolume;
            masterVolumeSlider.onValueChanged.AddListener(preferencesSetter.SetMasterVolume);

            musicVolumeSlider.value = Preferences.MusicVolume;
            musicVolumeSlider.onValueChanged.AddListener(preferencesSetter.SetMusicVolume);

            effectsVolumeSlider.value = Preferences.EffectsVolume;
            effectsVolumeSlider.onValueChanged.AddListener(preferencesSetter.SetEffectsVolume);

            nameField.text = Preferences.Name;
            nameField.onEndEdit.AddListener(OnEndTypingName);

            sensitivitySlider.value = Preferences.Sensitivity;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        private void OnEnable()
        {
            var es = FindObjectOfType<EventSystem>();
            if (!es || !firstSelected) return;

            es.SetSelectedGameObject(firstSelected);
        }

        private void OnEndTypingName(string _)
        {
            Preferences.Name = nameField.text;
        }

        private void OnSensitivityChanged(float _)
        {
            preferencesSetter.SetSensitivity(sensitivitySlider.value);
        }

        public void OnClickOk()
        {
            Preferences.Save();
            gameObject.SetActive(false);
        }
    }
}