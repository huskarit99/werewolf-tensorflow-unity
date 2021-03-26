using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyNetworkManager : NetworkManager
{
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
        Debug.Log("Client Connect");
    }
}
