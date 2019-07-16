using System;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    [Serializable]
    public class HostGameEvent : UnityEvent<ServerConfig>
    {
    }

    public class HostGameDialog : MonoBehaviour
    {
        public TMP_InputField maxPlayerInput;
        public TMP_Dropdown levelDropdown;

        public HostGameEvent onHostGame;

        public LevelManager LevelManager { get; set; }

        private void Start()
        {
            maxPlayerInput.placeholder.GetComponent<TextMeshProUGUI>().text = Constants.MaxPlayers.ToString();
            maxPlayerInput.onSelect.AddListener(ClearError);

            levelDropdown.ClearOptions();
            levelDropdown.AddOptions(LevelManager.AvailableLevels);
        }

        private void ClearError(string contents)
        {
            maxPlayerInput.GetComponent<Image>().color = maxPlayerInput.colors.selectedColor;
        }

        public void OnClickHost()
        {
            var maxText = maxPlayerInput.text;
            var maxPlayers = Constants.MaxPlayers;

            if (!string.IsNullOrWhiteSpace(maxText))
            {
                var ok = byte.TryParse(maxText, out maxPlayers);
                if (!ok || maxPlayers < 1)
                {
                    maxPlayerInput.GetComponent<Image>().color = Color.red;
                    return;
                }
            }

            var level = levelDropdown.options[levelDropdown.value].text;
            onHostGame.Invoke(new ServerConfig {MaxPlayerCount = maxPlayers, StartingLevel = level});
        }

        public void OnClickCancel()
        {
            gameObject.SetActive(false);
        }
    }
}