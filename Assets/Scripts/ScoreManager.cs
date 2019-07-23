using System.Collections.Generic;
using Networking;
using Networking.Packets;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private readonly List<ScoreRow> playersSorted = new List<ScoreRow>(8);
    public ScoreRow scoreRowPrefab;

    public LayoutGroup playerList;

    private NetworkManager networkManager;
    private bool dirty;

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

    private void LateUpdate()
    {
        if (dirty)
            SortScores();
    }

    private void AddPlayer(PlayerInfo player)
    {
        var row = Instantiate(scoreRowPrefab.gameObject, playerList.transform);
        row.GetComponent<ScoreRow>().PlayerInfo = player;
    }

    private void RemovePlayer(byte id)
    {
        foreach (RectTransform tr in playerList.transform)
            if (tr.GetComponent<ScoreRow>().PlayerInfo.Id == id)
                Destroy(tr.gameObject);
    }

    private void UpdatePlayerName(PlayerPreferences info)
    {
        foreach (RectTransform tr in playerList.transform)
        {
            var player = tr.GetComponent<ScoreRow>();
            if (player && player.PlayerInfo.Id == info.playerId)
                player.UpdateName(info.name);
        }
    }

    private void UpdatePlayerInfo(PlayerExtraInfo info)
    {
        foreach (RectTransform tr in playerList.transform)
        {
            var player = tr.GetComponent<ScoreRow>();
            if (player && player.PlayerInfo.Id == info.playerId)
                player.UpdateKills(info.kills);
        }

        dirty = true;
    }

    private void UpdatePlayerDeath(PlayerDeath info)
    {
        foreach (RectTransform tr in playerList.transform)
        {
            var player = tr.GetComponent<ScoreRow>();
            if (player && player.PlayerInfo.Id == info.killerId)
                player.UpdateKills(info.killerKills);
        }

        dirty = true;
    }

    private void SortScores()
    {
        playersSorted.Clear();

        foreach (RectTransform tr in playerList.transform) playersSorted.Add(tr.GetComponent<ScoreRow>());

        playersSorted.Sort(KillsComparer);

        for (var i = 0; i < playersSorted.Count; i++) playersSorted[i].transform.SetSiblingIndex(i);

        LayoutRebuilder.MarkLayoutForRebuild(playerList.GetComponent<RectTransform>());

        dirty = false;
    }

    private static int KillsComparer(ScoreRow x, ScoreRow y)
    {
        return y.PlayerInfo.Kills - x.PlayerInfo.Kills;
    }
}