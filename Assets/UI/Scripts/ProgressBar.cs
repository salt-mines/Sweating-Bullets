using UnityEngine;

namespace UI
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 1)]
        private float progress;

        public RectTransform progressBar;

        public float Progress
        {
            get => progress;
            set => SetProgress(value);
        }

        // Start is called before the first frame update
        private void OnValidate()
        {
            SetProgress(progress);
        }

        private void SetProgress(float newProgress)
        {
            progress = newProgress;

            var newScale = progressBar.localScale;
            newScale.x = progress;
            progressBar.localScale = newScale;
        }
    }
}