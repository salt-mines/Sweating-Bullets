using System;
using System.Collections.Generic;
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
        public TMP_Dropdown modeDropdown;
        public TMP_InputField killsToWinInput;

        public HostGameEvent onHostGame;

        public Loader Loader { get; set; }

        private void Start()
        {
            maxPlayerInput.placeholder.GetComponent<TextMeshProUGUI>().text = Constants.MaxPlayers.ToString();
            maxPlayerInput.onSelect.AddListener(ClearError);

            killsToWinInput.placeholder.GetComponent<TextMeshProUGUI>().text = "30";
            killsToWinInput.onSelect.AddListener(ClearError);

            levelDropdown.ClearOptions();
            levelDropdown.AddOptions(Loader.LevelManager.AvailableLevels);

            modeDropdown.ClearOptions();
            var modes = new List<string>(Loader.availableGameModes.Count);
            foreach (var m in Loader.availableGameModes)
            {
                modes.Add(m.modeName);
            }

            modeDropdown.AddOptions(modes);
        }

        private void ClearError(string contents)
        {
            maxPlayerInput.GetComponent<Image>().color = maxPlayerInput.colors.selectedColor;
            killsToWinInput.GetComponent<Image>().color = killsToWinInput.colors.selectedColor;
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

            short killsToWin = 30;

            if (!string.IsNullOrWhiteSpace(killsToWinInput.text))
            {
                var ok = short.TryParse(killsToWinInput.text, out killsToWin);
                if (!ok)
                {
                    maxPlayerInput.GetComponent<Image>().color = Color.red;
                    return;
                }
            }

            onHostGame.Invoke(new ServerConfig
            {
                MaxPlayerCount = maxPlayers,
                StartingLevel = levelDropdown.options[levelDropdown.value].text,
                GameMode = Loader.availableGameModes[modeDropdown.value],
                KillsToWin = killsToWin
            });
        }

        public void OnClickCancel()
        {
            gameObject.SetActive(false);
        }
    }
}