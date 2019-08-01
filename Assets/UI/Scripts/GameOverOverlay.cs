using TMPro;
using UnityEngine;

namespace UI
{
    public class GameOverOverlay : MonoBehaviour
    {
        public float changeTime;

        public TextMeshProUGUI winnerText;
        public TextMeshProUGUI changeTimeText;

        private float timeToChange;

        private void OnEnable()
        {
            timeToChange = Time.time + changeTime;
        }

        private void Update()
        {
            changeTimeText.text = $"Map changing in {timeToChange - Time.time:F1}...";
        }

        public void SetWinner(string name)
        {
            winnerText.text = $"Winner is {name}!";
        }
    }
}