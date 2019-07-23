using Networking;
using Networking.Packets;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public ScoreRow scoreRowPrefab;

    public LayoutGroup playerList;

    private NetworkManager networkManager;

    private void Awake()
    {
        networkManager = FindObjectOfType<NetworkManager>();
    }

    private void Start()
    {
        var cl = networkManager.Client;
        if (cl == null) return;

        cl.PlayerJoined += (sender, info) => { AddPlayer(info); };
        cl.PlayerLeft += (sender, info) => { RemovePlayer(info.Id); };
        cl.PlayerSentPreferences += (sender, info) => { UpdatePlayerName(info); };
        cl.PlayerSentInfo += (sender, info) => { UpdatePlayerInfo(info); };
        cl.PlayerDeath += (sender, info) => { UpdatePlayerDeath(info); };

        if (cl.LocalPlayer != null) AddPlayer(cl.LocalPlayer);
    }

    public void AddPlayer(PlayerInfo player)
    {
        var row = Instantiate(scoreRowPrefab.gameObject, playerList.transform);
        row.GetComponent<ScoreRow>().PlayerInfo = player;
    }

    public void RemovePlayer(byte id)
    {
        foreach (RectTransform tr in playerList.transform)
            if (tr.GetComponent<ScoreRow>().PlayerInfo.Id == id)
                Destroy(tr.gameObject);
    }

    public void UpdatePlayerName(PlayerPreferences info)
    {
        foreach (RectTransform tr in playerList.transform)
        {
            var player = tr.GetComponent<ScoreRow>();
            if (player && player.PlayerInfo.Id == info.playerId)
                player.UpdateName(info.name);
        }
    }

    public void UpdatePlayerInfo(PlayerExtraInfo info)
    {
        foreach (RectTransform tr in playerList.transform)
        {
            var player = tr.GetComponent<ScoreRow>();
            if (player && player.PlayerInfo.Id == info.playerId)
                player.UpdateKills(info.kills);
        }
    }

    public void UpdatePlayerDeath(PlayerDeath info)
    {
        foreach (RectTransform tr in playerList.transform)
        {
            var player = tr.GetComponent<ScoreRow>();
            if (player && player.PlayerInfo.Id == info.killerId)
                player.UpdateKills(info.killerKills);
        }
    }
}