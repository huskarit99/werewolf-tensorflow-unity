using System.Collections;
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
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        //--- Set VoteText cho người chơi
        var player = GameObject.Instantiate(playerPrefab, new Vector3(0, 0, 800), Quaternion.identity);
        player.GetComponent<PlayerNetworkBehavior>().index = NetworkServer.connections.Count;
        player.GetComponent<PlayerNetworkBehavior>().VoteText.SetActive(false); 
        NetworkServer.AddPlayerForConnection(conn, player);
    }

}
