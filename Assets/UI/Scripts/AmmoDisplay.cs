using Game;
using Networking;
using TMPro;
using UnityEngine;

namespace UI
{
    public class AmmoDisplay : MonoBehaviour
    {
        public TextMeshProUGUI ammoText;

        private Client client;
        private GameManager gameManager;
        private PlayerMechanics mechanics;

        private byte lastAmmoCount;

        private void Start()
        {
            client = FindObjectOfType<NetworkManager>().Client;
            gameManager = FindObjectOfType<GameManager>();

            if (client == null) Debug.LogError("No NetworkManager found.");
        }

        private void Update()
        {
            if (client.LocalPlayer == null || !client.LocalPlayer.PlayerObject) return;

            if (!mechanics)
                mechanics = client.LocalPlayer.PlayerObject.GetComponent<PlayerMechanics>();

            var wep = mechanics.CurrentWeapon;

            if (!wep)
                return;

            if (wep.Ammo == lastAmmoCount) return;

            SetAmmo(wep.Ammo);
        }

        private void SetAmmo(byte ammo)
        {
            lastAmmoCount = ammo;
            ammoText.text = ammo == 0 ? "" : ammo.ToString();
        }
    }
}