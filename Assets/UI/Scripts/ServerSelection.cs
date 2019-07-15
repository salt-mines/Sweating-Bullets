using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [Serializable]
    public class JoinEvent : UnityEvent<ServerInfo>
    {
    }

    public class ServerSelection : MonoBehaviour
    {
        public TMP_InputField inputField;
        public TextMeshProUGUI statusText;
        public ScrollRect serverList;

        public GameObject firstSelected;

        public RectTransform serverInfoPrefab;

        public Color errorColor = Color.red;

        public JoinEvent onJoin;

        private MenuClient menuClient;

        private Task<IPAddress[]> resolveTask;
        private IPEndPoint waitingForResponse;

        public MenuClient MenuClient
        {
            get => menuClient;
            set => (menuClient = value).ServerDiscovered += OnServerFound;
        }

        private void Start()
        {
            inputField.onSelect.AddListener(FieldSelected);

            if (onJoin == null)
                onJoin = new JoinEvent();
        }

        private void OnEnable()
        {
            var es = FindObjectOfType<EventSystem>();
            if (!es || !firstSelected) return;

            es.SetSelectedGameObject(firstSelected);
        }

        private void Update()
        {
            if (resolveTask == null || !resolveTask.IsCompleted) return;

            ResolveCompleted();
            resolveTask = null;
        }

        private void OnDestroy()
        {
            MenuClient.ServerDiscovered -= OnServerFound;
        }

        public void FieldSelected(string text)
        {
            inputField.GetComponent<Image>().color = inputField.colors.selectedColor;
        }

        public void OnScan()
        {
            foreach (Transform o in serverList.content.transform) Destroy(o.gameObject);

            MenuClient.DiscoverLocalServers();
        }

        public void OnJoin()
        {
            var host = string.IsNullOrEmpty(inputField.text) ? "localhost" : inputField.text;
            inputField.interactable = false;

            SetStatus(false, "Finding host...");

            resolveTask = Dns.GetHostAddressesAsync(host);
        }

        private void ResolveCompleted()
        {
            inputField.interactable = true;

            if (resolveTask.Status == TaskStatus.Faulted)
            {
                inputField.GetComponent<Image>().color = errorColor;
                SetStatus(true, "Error finding host");
                return;
            }

            var ips = resolveTask.Result;
            if (ips == null || ips.Length <= 0) return;

            foreach (var ip in ips)
            {
                if (ip.AddressFamily != AddressFamily.InterNetwork) continue;

                waitingForResponse = new IPEndPoint(ip, Constants.AppPort);
                MenuClient.DiscoverServer(waitingForResponse);
                SetStatus(false, "Waiting for server response...");
                break;
            }
        }

        public void OnCancel()
        {
            gameObject.SetActive(false);
        }

        private void SetStatus(bool error, string status)
        {
            statusText.color = error ? errorColor : Color.black;
            statusText.text = status;
        }

        private void OnServerFound(object sender, ServerInfo info)
        {
            if (waitingForResponse != null && waitingForResponse.Equals(info.IP))
            {
                OnServerResponse(info);
                return;
            }

            var si = Instantiate(serverInfoPrefab, serverList.content);
            si.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"IP: {info.IP.Address}";
            si.GetComponent<Button>().onClick.AddListener(delegate { inputField.text = info.IP.Address.ToString(); });
        }

        private void OnServerResponse(ServerInfo info)
        {
            onJoin.Invoke(info);
        }
    }
}