using DG.Tweening;
using Networking;
using Networking.Packets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class KillFeed : MonoBehaviour
    {
        public TextMeshProUGUI fragText;
        public TextMeshProUGUI deathText;

        private Client client;

        private void Start()
        {
            client = FindObjectOfType<NetworkManager>().Client;

            if (client == null) return;

            client.OwnKill += OnOwnKill;
            client.PlayerDeath += OnDeath;
            client.LevelChanging += (sender, s) => ResetFeed();
        }

        private void ResetFeed()
        {
            fragText.text = "";
            deathText.text = "";
        }

        private void OnOwnKill(object sender, PlayerInfo target)
        {
            fragText.text = $"You killed {target.Name}!";
            StartFade(fragText);
        }

        private void OnDeath(object sender, PlayerDeath death)
        {
            if (client.PlayerId != death.playerId) return;

            var killer = client.Players[death.killerId];

            deathText.text = $"Killed by {(killer == null ? "<Player>" : killer.Name)}!";
            StartFade(deathText);
        }

        private void StartFade(Graphic text)
        {
            var col = text.color;
            col.a = 255;
            text.color = col;

            text.DOFade(0f, 3);
        }
    }
}