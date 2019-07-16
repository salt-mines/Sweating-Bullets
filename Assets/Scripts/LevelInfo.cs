using System;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public string levelName;
    public string levelAuthor;
    public string levelVersion;

    public GameObject spawnPointParent;

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(levelName))
            throw new ArgumentException("Level Name must not be empty");
    }
}