using TMPro;
using UI;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField]
    private ProgressBar progressBar;

    [SerializeField]
    private TextMeshProUGUI loadingStatusText;

    [SerializeField]
    private TextMeshProUGUI connectingStatusText;

    /// <summary>
    ///     Progress of loading screen in float [0, 1.0]
    /// </summary>
    public float Progress
    {
        get => progressBar.Progress;
        set => progressBar.Progress = value;
    }

    public string LoadingStatus
    {
        get => loadingStatusText.text;
        set => loadingStatusText.text = value;
    }

    public string ConnectingStatus
    {
        get => connectingStatusText.text;
        set => connectingStatusText.text = value;
    }

    public void Show(bool show, bool parentToo)
    {
        gameObject.SetActive(show);

        if (parentToo)
            transform.parent.gameObject.SetActive(show);
    }
}