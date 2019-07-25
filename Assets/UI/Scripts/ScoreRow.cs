using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI
{
    public class ScoreRow : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        [FormerlySerializedAs("scoreText")]
        public TextMeshProUGUI killsText;
        public TextMeshProUGUI deathsText;

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

        public void UpdateKills(short kills)
        {
            killsText.text = kills.ToString();
        }

        public void UpdateDeaths(short deaths)
        {
            deathsText.text = deaths.ToString();
        }
    }
}