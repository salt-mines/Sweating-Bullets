using Networking;
using TMPro;
using UnityEngine;

namespace UI
{
    public class ScoreRow : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI scoreText;

        private PlayerInfo playerInfo;

        public PlayerInfo PlayerInfo
        {
            get => playerInfo;
            set => UpdateName((playerInfo = value).Name);
        }

        public void UpdateName(string plName)
        {
            nameText.text = plName;
        }
    }
}