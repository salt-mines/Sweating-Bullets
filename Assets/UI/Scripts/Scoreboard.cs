using System.Collections.Generic;
using Networking;
using Networking.Packets;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Scoreboard : MonoBehaviour
    {
        private readonly List<ScoreRow> playersSorted = new List<ScoreRow>(8);

        [Tooltip("Prefab for each player's score row.")]
        public ScoreRow scoreRowPrefab;

        [Tooltip("Layout group where score rows are created.")]
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

            // Hook into client events so we can properly update the scoreboard
            cl.PlayerJoined += (sender, info) => { AddPlayer(info); };
            cl.PlayerLeft += (sender, info) => { RemovePlayer(info.Id); };
            cl.PlayerSentPreferences += (sender, info) => { UpdatePlayerPref(info); };
            cl.PlayerSentInfo += (sender, info) => { UpdatePlayerInfo(info); };
            cl.PlayerDeath += (sender, info) => { UpdatePlayerDeath(info); };

            cl.LevelChanging += (sender, s) => ResetBoard();
            cl.LevelLoaded += (sender, s) => RefreshBoard();

            // Add local player separately since its creation doesn't raise an event.
            //if (cl.LocalPlayer != null) AddPlayer(cl.LocalPlayer);
        }

        private void LateUpdate()
        {
            // Sort scoreboard if any changes have happened.
            if (dirty)
                SortScores();
        }

        private void ResetBoard()
        {
            for (var i = 0; i < playerList.transform.childCount; i++)
                Destroy(playerList.transform.GetChild(i).gameObject);
        }

        private void RefreshBoard()
        {
            foreach (var pl in networkManager.Client.Players)
                if (pl != null)
                    AddPlayer(pl);
        }

        private void AddPlayer(PlayerInfo player)
        {
            foreach (RectTransform tr in playerList.transform)
                if (tr.GetComponent<ScoreRow>().PlayerInfo?.Id == player.Id)
                    return;

            var row = Instantiate(scoreRowPrefab.gameObject, playerList.transform).GetComponent<ScoreRow>();
            row.PlayerInfo = player;

            row.UpdateColor(player.Color);
            row.UpdateName(player.Name);
            row.UpdateKills(player.Kills);
            row.UpdateDeaths(player.Deaths);
        }

        private void RemovePlayer(byte id)
        {
            foreach (RectTransform tr in playerList.transform)
                if (tr.GetComponent<ScoreRow>().PlayerInfo.Id == id)
                    Destroy(tr.gameObject);
        }

        private void UpdatePlayerPref(PlayerPreferences info)
        {
            foreach (RectTransform tr in playerList.transform)
            {
                var player = tr.GetComponent<ScoreRow>();
                if (!player || player.PlayerInfo.Id != info.playerId) continue;

                player.UpdateColor(info.color);
                player.UpdateName(info.name);
            }
        }

        private void UpdatePlayerInfo(PlayerExtraInfo info)
        {
            foreach (RectTransform tr in playerList.transform)
            {
                var player = tr.GetComponent<ScoreRow>();
                if (!player || player.PlayerInfo.Id != info.playerId) continue;

                player.UpdateKills(info.kills);
                player.UpdateDeaths(info.deaths);
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
                if (player && player.PlayerInfo.Id == info.playerId)
                    player.UpdateDeaths(info.playerDeaths);
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
}