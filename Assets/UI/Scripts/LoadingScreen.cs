using TMPro;
using UnityEngine;

namespace UI
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField]
        public ProgressBar progressBar;

        [SerializeField]
        public TextMeshProUGUI loadingStatusText;

        [SerializeField]
        public TextMeshProUGUI connectingStatusText;

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
}