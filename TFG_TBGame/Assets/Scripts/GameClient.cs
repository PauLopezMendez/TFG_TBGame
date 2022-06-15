using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Colyseus;
using Colyseus.Schema;
using System;

public class GameClient : GenericSingleton<GameClient>
{
    public EventHandler OnConnect;
    public EventHandler OnClose;
    public EventHandler OnJoin;
    public EventHandler<State> OnInitialState;
    public EventHandler<object> OnMessage;
    public EventHandler<string> OnGamePhaseChange;

    private Client client;
    private Room<State> room;
    private bool initialStateReceived = false;

    public string ClientId
    {
        get { return client?.Id; }
    }

    public string SessionId
    {
        get { return room?.SessionId; }
    }

    public bool Connected
    {
        get { return ClientId != null; }
    }

    public State State
    {
        get { return room?.State; }
    }

    public bool Joined
    {
        get { return room != null && room.Connection.IsOpen; }
    }

    void OnDestroy()
    {
        
        print("OnDestroy");
        if (client != null) client.Close();
    }

    private void OnApplicationQuit()
    {
        print("OnApplicationQuit");
        if (room != null) room.Leave();
        if (client != null) client.Close();
    }

    public void Connect()
    {
        print("Connect");
        string uri = "ws://localhost:2567";
        client = new Client(uri);
        client.OnOpen += OnOpenHandler;
        client.OnClose += OnCloseHandler;
        StartCoroutine(ConnectAndListen());
    }

    public void Join()
    {
        print("Join");
        room = client.Join<State>("game");
        room.OnReadyToConnect += (sender, e) => StartCoroutine(room.Connect());
        room.OnMessage += OnMessageHandler;
        room.OnJoin += OnJoinHandler;
        room.OnStateChange += OnRoomStateChangeHandler;
    }

    public void Leave()
    {
        if (room != null) room.Leave();
        room = null;
    }

    public void InitialState (){
        room.Send(new { command = "initialState"});
    }

    public void RefreshShop (){
        room.Send(new { command = "refreshShop"});
    }

    public void DestroyRecruits(string[][] idCardsDestroy){
        room.Send(new { command = "destroyRecruits", idCardsDestroy});
    }

    public void NullRecruit(string idToNull){
        room.Send(new { command = "nullRecruit", idToNull});
    }

    public void SendRecruited (string cardId,int oldPos, int newPos){
        room.Send(new { command = "recruited", cardId, oldPos, newPos});
    }

    public void SendLearn (string cardId, int posCard, string[] tools){
        room.Send(new { command = "learn", cardId, posCard, tools});
    }
    public void SendSkip (){
        room.Send(new { command = "skip"});
    }

    // handlers

    void OnOpenHandler(object sender, EventArgs e)
    {
        OnConnect?.Invoke(this, e);
    }

    void OnCloseHandler(object sender, EventArgs e)
    {
        
        OnClose?.Invoke(this, e);
    }

    void OnJoinHandler(object sender, EventArgs e)
    {
        
        room.State.OnChange += OnStateChangeHandler;

        OnJoin?.Invoke(this, e);
    }

    void OnRoomStateChangeHandler(object sender, StateChangeEventArgs<State> e)
    {
        if (e.IsFirstState && !initialStateReceived)
        {
            initialStateReceived = true;
            OnInitialState?.Invoke(this, e.State);
        }
    }

    void OnStateChangeHandler(object sender, OnChangeEventArgs e)
    {
        if (!initialStateReceived) return;

        foreach (var change in e.Changes)
        {
            if (change.Field == "phase")
            {
                OnGamePhaseChange?.Invoke(this, (string)change.Value);
            }
        }
    }

    void OnMessageHandler(object sender, MessageEventArgs e)
    {
        var message = e.Message;

        OnMessage?.Invoke(this, message);
    }

    IEnumerator ConnectAndListen()
    {
        yield return StartCoroutine(client.Connect());

        while (true)
        {
            client.Recv();
            yield return 0;
        }
    }
}