using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class LevelInfo : MonoBehaviour
{
    public string levelName;
    public string levelAuthor;
    public string levelVersion;

    [Tooltip("Spectator camera of the level. Optional.")]
    public Camera spectatorCamera;

    [Tooltip("Parent object of spawnpoint objects.")]
    public GameObject spawnPointParent;

    [Tooltip("Parent object of player objects.")]
    public GameObject playerParent;

    [Tooltip("Parent object of dynamic objects, like bullets.")]
    public GameObject dynamicObjectParent;

    [Tooltip("Audio mixer snapshot to load when level is loaded.")]
    public AudioMixerSnapshot levelSnapshot;

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(levelName))
            throw new ArgumentException("Level Name must not be empty");
    }

    private void Start()
    {
        if (SceneManager.sceneCount > 1 && spectatorCamera)
            spectatorCamera.gameObject.SetActive(false);

        if (levelSnapshot)
            levelSnapshot.TransitionTo(0);
    }

    private void OnDestroy()
    {
        DOTween.KillAll();
    }
}