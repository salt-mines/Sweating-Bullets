using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public GameObject[] playerList;

    public TextMeshProUGUI[] playerNamesTMP;
    public TextMeshProUGUI[] playerPointsTMP;

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
        playerList = GameObject.FindGameObjectsWithTag("Player");
        SetPlayerNames();
    }

    public void UpdateScoreText()
    {
        for(int i = 0; i < playerList.Length; i++)
        {
            playerPointsTMP[i].text = playerList[i].GetComponent<PlayerMechanics>().points.ToString();
        }
    }

    private void SetPlayerNames()
    {
        for (int i = 0; i < playerList.Length; i++)
        {
            playerNamesTMP[i].text = "P" + (i+1);
        }
    }
}
