using Networking;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HealthDisplay : MonoBehaviour
    {
        private Client client;

        public TextMeshProUGUI healthText;

        public Gradient healthColor;

        private void Start()
        {
            client = FindObjectOfType<NetworkManager>().Client;

            if (client == null)
            {
                Debug.LogError("No NetworkManager found.");
                return;
            }

            client.SelfHurt += OnSelfHurt;
            client.PlayerRespawn += OnPlayerRespawn;

            SetHealth(100);
        }

        private void OnPlayerRespawn(object sender, PlayerInfo pl)
        {
            if (pl.Id == client.LocalPlayer.Id)
            {
                SetHealth(pl.Health);
            }
        }

        private void OnSelfHurt(object sender, byte damage)
        {
            var hp = client.LocalPlayer.Health;
            if (hp < damage)
                hp = 0;
            else
                hp -= damage;

            SetHealth(hp);
        }

        private void SetHealth(byte hp)
        {
            healthText.text = hp.ToString();
            healthText.color = healthColor.Evaluate(hp / 100f);
        }
    }
}