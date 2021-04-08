using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class MyNetworkManager : NetworkManager
{
    public double Radius;
    public double Distance;
    public GameObject CentralPoint;
    public List<PlayerNetworkBehavior> ListPlayers = new List<PlayerNetworkBehavior>();
    public override void OnStartServer()
    {
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
        playerPrefab.GetComponent<PlayerNetworkBehavior>().VoteText.SetActive(false);
        var player = GameObject.Instantiate(playerPrefab, new Vector3(0, 0, 800), Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player);
    }

}
