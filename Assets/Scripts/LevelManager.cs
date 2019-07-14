using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager
{
    public List<string> AvailableLevels { get; } = new List<string>();
    
    public string StartingLevel { get; set; }
    public string CurrentLevel { get; private set; }

    public event EventHandler<string> LevelChanging;

    public LevelManager(IEnumerable<SceneReference> levels)
    {
        foreach (var sc in levels)
        {
            AvailableLevels.Add(Path.GetFileNameWithoutExtension(sc.ScenePath));
        }

        StartingLevel = AvailableLevels[0];

        if (SceneManager.sceneCount <= 1) return;
        
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var sc = SceneManager.GetSceneAt(i);

            if (IsValidLevel(sc.name))
                CurrentLevel = sc.name;
        }
    }

    public bool IsValidLevel(string level) => AvailableLevels.Contains(level);

    public void ChangeLevel(string level)
    {
        if (!IsValidLevel(level))
        {
            Debug.LogError($"Tried to change to invalid level \"{level}\"");
            return;
        }
        
        Debug.Log($"Changing to level {level}");
        
        LevelChanging?.Invoke(this, level);
        CurrentLevel = level;
    }
}