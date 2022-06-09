using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class ConnectingSceneController : MonoBehaviour
{
    public Text message;

    private GameClient client;

    void Start()
    {
        message.text = "Connecting...";

        client = GetComponent<GameClient>();
        client.OnConnect += OnConnect;
        client.OnJoin += OnJoin;

        // if (!client.Connected)
        // {
        //     client.Connect();
        // }
        // else
        // {
        //     OnConnect(this, null);
        // }

        client.Connect();
    }

    void Update(){
        print(client.Connected)
;        if(Input.GetKeyDown(KeyCode.J)){
            client.getA();
        }
         if(Input.GetKeyDown(KeyCode.K)){
            client.Join();
        }
    }

    void OnConnect(object sender, EventArgs e)
    {
        message.text = "Finding a game...";

        if (!client.Joined)
        {
            client.Join();
        }
        else
        {            
            OnJoin(this, null);
        }
    }

    void OnJoin(object sender, EventArgs e)
    {
        message.text = "Joined! Finding another player...";

        client.OnGamePhaseChange += GamePhaseChangeHandler;
    }

    private void GamePhaseChangeHandler(object sender, string phase)
    {
        if (phase == "Recruit")
        {
            SceneManager.LoadScene("GameScene");
        }
    }

    private void OnDestroy()
    {
        if (client != null)
        {
            client.OnConnect -= OnConnect;
            client.OnJoin -= OnJoin;
            client.OnGamePhaseChange -= GamePhaseChangeHandler;
        }
    }
}

