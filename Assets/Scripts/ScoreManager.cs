using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public GameObject[] playerList;
    private List <Player> playerListD;

    public TextMeshProUGUI[] playerNamesTMP;
    public TextMeshProUGUI[] playerPointsTMP;

    public GameObject UIScorePrefab;

    

    // Start is called before the first frame update
    void Start()
    {
        PlayerJoin();
        SetPlayerNames();
        UpdateScoreText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerJoin()
    {
        //playerList = GameObject.FindGameObjectsWithTag("Player");
        //playerListD.Add(new Player());
    }

    public void UpdateScoreText()
    {
//        for(int i = 0; i < playerList.Length; i++)
//        {
//            playerPointsTMP[i].text = playerList[i].GetComponent<PlayerMechanics>().points.ToString();
//        }
    }

    private void SetPlayerNames()
    {
//        for (int i = 0; i < playerList.Length; i++)
//        {
//            playerNamesTMP[i].text = "P" + (i+1);
//        }
    }
    public enum Points
    {
        Kill,
        Death
    }
    internal class Score
    {
        private readonly Dictionary<Points, int> points = new Dictionary<Points, int>();

        public Score()
        {
            foreach (Points point in Enum.GetValues(typeof(Points)))
            {
                points.Add(point, 0);
            }
        }
        public int getTypeOfScore(Points point)
        {
            return points[point];
        }
        public void increaseTypeOfScore(Points point, int amount = 1)
        {
            points[point] += amount;
        }
    }

    internal class Player
    {
        public string Name { get; set; }
        public Score Score { get; set; }

        public Player(string name = "Anonymous")
        {
            Name = name;
            Score = new Score();
        }

        public int getTypeOfScore(Points point)
        {
            return Score.getTypeOfScore(point);
        }

        public void changeScore(Points point, int amount)
        {
            Score.increaseTypeOfScore(point, amount);
        }

        public void Death()
        {
            Score.increaseTypeOfScore(Points.Death);
        }
        public void Kill()
        {
            Score.increaseTypeOfScore(Points.Kill);
        }
    }
}
