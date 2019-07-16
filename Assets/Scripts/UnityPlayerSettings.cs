using UnityEngine;

public class UnityPlayerSettings : MonoBehaviour
{
    private void Awake()
    {
        QualitySettings.maxQueuedFrames = 0;
        Application.targetFrameRate = 200;

        Destroy(this);
    }
}