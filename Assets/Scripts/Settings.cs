using UnityEngine;

public class Settings : MonoBehaviour
{
    private void Awake()
    {
        QualitySettings.maxQueuedFrames = 0;
        Application.targetFrameRate = 200;

        Destroy(this);
    }
}