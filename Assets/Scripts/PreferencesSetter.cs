using System;
using Game;
using UnityEngine;
using UnityEngine.Audio;

public class PreferencesSetter : MonoBehaviour
{
    public AudioMixer audioMixer;

    public string masterVolumeParameter;
    public string musicVolumeParameter;
    public string effectsVolumeParameter;

    internal Preferences Preferences { get; set; }

    private void Start()
    {
        if (string.IsNullOrEmpty(masterVolumeParameter))
            throw new ArgumentException("AudioPreferencesSetter requires masterVolumeParameter");
        if (string.IsNullOrEmpty(musicVolumeParameter))
            throw new ArgumentException("AudioPreferencesSetter requires musicVolumeParameter");
        if (string.IsNullOrEmpty(effectsVolumeParameter))
            throw new ArgumentException("AudioPreferencesSetter requires effectsVolumeParameter");

        SetMasterVolume(Preferences.MasterVolume);
        SetMusicVolume(Preferences.MusicVolume);
        SetEffectsVolume(Preferences.EffectsVolume);

        SetSensitivity(Preferences.Sensitivity);
    }

    public void SetSensitivity(float value)
    {
        GetComponent<GameInput>().mouseSensitivity = value;
        Preferences.Sensitivity = value;
    }

    public void SetMasterVolume(float value)
    {
        audioMixer.SetFloat(masterVolumeParameter, Utils.LinearToDecibel(value));
        Preferences.MasterVolume = value;
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat(musicVolumeParameter, Utils.LinearToDecibel(value));
        Preferences.MusicVolume = value;
    }

    public void SetEffectsVolume(float value)
    {
        audioMixer.SetFloat(effectsVolumeParameter, Utils.LinearToDecibel(value));
        Preferences.EffectsVolume = value;
    }
}