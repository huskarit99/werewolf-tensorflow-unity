﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;
using Socket.Quobject.SocketIoClientDotNet.Client;
using Socket.Newtonsoft.Json;

public class MyNetworkManager : NetworkManager
{
    public double Radius;
    public double Distance;
    public GameObject CentralPoint;
    public List<string> roles;
    public QSocket socket { get; set; }

    public override void OnStartServer()
    {
        DontDestroyOnLoad(this);
        //--- Set role cho người chơi
        roles = new List<string>(){
                                  Role4Player.Seer,
                                  //Role4Player.Guard,
                                  Role4Player.Wolf,
                                  //Role4Player.Witch,
                                  //Role4Player.Hunter
                                   };
        Debug.Log("Start Server");
    }
    public override void OnStopServer()
    {
        Debug.Log("Stop Server");
    }
    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
    }
    public override void OnStartHost()
    {
        base.OnStartHost();

    }
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //--- Set VoteText cho người chơi
        var player = GameObject.Instantiate(playerPrefab, new Vector3(0, 0, 800), Quaternion.identity);
        player.GetComponent<PlayerNetworkBehavior>().index = NetworkServer.connections.Count;
        player.GetComponent<PlayerNetworkBehavior>().VoteText.SetActive(false); 
        NetworkServer.AddPlayerForConnection(conn, player);
    }
    private string RandomRole4Player(List<string> _roles, out List<string> _arr)
    {
        if (_roles.Count > 0)
        {
            var _index = UnityEngine.Random.Range(0, _roles.Count - 1);
            var item = _roles[_index];
            _roles.RemoveAt(_index);
            _arr = _roles;
            return item;
        }
        _arr = _roles;
        return Role4Player.Human;
    }

}
