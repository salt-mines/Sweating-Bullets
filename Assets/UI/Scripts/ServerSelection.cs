using System;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    [Serializable]
    public class JoinEvent : UnityEvent<string>
    {
    }

    public class ServerSelection : MonoBehaviour
    {
        public TMP_InputField inputField;
        public ScrollRect serverList;

        public RectTransform serverInfoPrefab;

        public JoinEvent onJoin;

        private MenuClient menuClient;

        public MenuClient MenuClient
        {
            get => menuClient;
            set => (menuClient = value).ServerDiscovered += OnServerFound;
        }

        private void Start()
        {
            if (onJoin == null)
                onJoin = new JoinEvent();
        }

        private void OnDestroy()
        {
            MenuClient.ServerDiscovered -= OnServerFound;
        }

        public void OnScan()
        {
            foreach (Transform o in serverList.content.transform)
            {
                Destroy(o.gameObject);
            }
            
            MenuClient.DiscoverLocalServers();
        }

        public void OnJoin()
        {
            onJoin.Invoke(inputField.text);
        }

        public void OnCancel()
        {
            Destroy(gameObject);
        }

        private void OnServerFound(object sender, ServerInfo info)
        {
            var si = Instantiate(serverInfoPrefab, serverList.content);
            si.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"IP: {info.IP.Address}";
            si.GetComponent<Button>().onClick.AddListener(delegate { inputField.text = info.IP.Address.ToString(); });
        }
    }
}