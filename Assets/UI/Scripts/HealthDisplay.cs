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
        private PlayerMechanics mechanics;

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

        private void Update()
        {
            if (client.LocalPlayer == null) return;

            if (!client.LocalPlayer.PlayerObject)
                return;

            if (!mechanics)
                mechanics = client.LocalPlayer.PlayerObject.GetComponent<PlayerMechanics>();

            if(mechanics.Health.ToString() != healthText.text.ToString())
            {
                SetHealth(mechanics.Health);
            }
        }

        private void OnPlayerRespawn(object sender, PlayerInfo pl)
        {
            if (pl.Id == client.LocalPlayer.Id) SetHealth(pl.Health);
        }

        private void OnSelfHurt(object sender, Client.DamageEventArgs dea)
        {
            var hp = client.LocalPlayer.PlayerObject.playerMechanics.Health;
            if (hp < dea.Damage)
                hp = 0;
            else
                hp -= dea.Damage;

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