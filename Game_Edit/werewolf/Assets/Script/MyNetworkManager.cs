using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class MyNetworkManager : NetworkManager
{
    public double Radius;
    public double Distance;

    public List<GameObject> Players = new List<GameObject>();
    public override void OnStartServer()
    {
        Debug.Log("Start Server");
    }
    public override void OnStopClient()
    {
        Debug.Log("End Server");
    }
    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("Number of client = " + NetworkServer.connections.Count);
    }
}
