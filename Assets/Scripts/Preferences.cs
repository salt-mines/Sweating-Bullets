using UnityEngine;

public class Preferences
{
    private string name = "Player";

    public string Name
    {
        get => name;
        set => name = string.IsNullOrWhiteSpace(value) ? "Player" : value;
    }

    public float Sensitivity { get; set; } = 10f;

    public float MasterVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 1.0f;
    public float EffectsVolume { get; set; } = 1.0f;

    public void Load()
    {
        Name = PlayerPrefs.GetString("PlayerName", Name);
        Sensitivity = PlayerPrefs.GetFloat("Sensitivity", Sensitivity);

        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", MasterVolume);
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", MusicVolume);
        EffectsVolume = PlayerPrefs.GetFloat("EffectsVolume", EffectsVolume);
    }

    public void Save()
    {
        PlayerPrefs.SetString("PlayerName", Name);
        PlayerPrefs.SetFloat("Sensitivity", Sensitivity);

        PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
        PlayerPrefs.SetFloat("EffectsVolume", EffectsVolume);

        PlayerPrefs.Save();
    }
}
