using Game;
using Networking;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HealthDisplay : MonoBehaviour
    {
        private Client client;
        private GameManager gameManager;

        public TextMeshProUGUI healthText;

        public Gradient healthColor;

        private void Start()
        {
            client = FindObjectOfType<NetworkManager>().Client;
            gameManager = FindObjectOfType<GameManager>();

            if (client == null)
            {
                Debug.LogError("No NetworkManager found.");
                return;
            }

            client.SelfHurt += OnSelfHurt;
            client.PlayerRespawn += OnPlayerRespawn;
        }

        private void OnPlayerRespawn(object sender, PlayerInfo pl)
        {
            if (pl.Id == client.LocalPlayer.Id) SetHealth(pl.Health);
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
            var maxHp = gameManager.currentGameMode ? gameManager.currentGameMode.maxHealth : 100;

            if (maxHp == 0)
            {
                gameObject.SetActive(false);
                return;
            }

            healthText.text = hp.ToString();
            healthText.color = healthColor.Evaluate(hp / (float) maxHp);
        }
    }
}