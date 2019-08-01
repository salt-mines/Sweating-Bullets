using TMPro;
using UnityEngine;

namespace UI
{
    public class DeadOverlay : MonoBehaviour
    {
        public float respawnTime;

        public TextMeshProUGUI respawnTimeText;

        private float timeToRespawn;

        internal bool block = false;

        private void OnEnable()
        {
            timeToRespawn = Time.time + respawnTime;

            if (block)
                gameObject.SetActive(false);
        }

        private void Update()
        {
            respawnTimeText.text = $"Respawning in {timeToRespawn - Time.time:F1}...";
        }
    }
}