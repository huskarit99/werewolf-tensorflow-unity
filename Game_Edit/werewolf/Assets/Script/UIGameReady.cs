using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UIGameReady : NetworkBehaviour
{
    public GameObject GameReadyButton; // Nút ấn sẵn sàng
    bool SyncIsReady;
    [SyncVar(hook = nameof(OnIsReadyChange))]
    bool isReady = false;  // Kiểm tra sẵn sàng
    bool isStart = false;

    public void ShowReadyButton(bool isShow)
    {
        GameReadyButton.SetActive(isShow);
    }
    public bool GetIsReady()
    {
        return isReady;
    }

    public void SetIsReady(bool _isReady)
    {
        isReady = _isReady;
    }

    void OnIsReadyChange(bool _old, bool _new)
    {
        SyncIsReady = isReady;
    }

    public bool GetIsStart()
    {
        return isStart;
    }

    public void SetIsStart(bool _isStart)
    {
        isStart = _isStart;
    }

    public void ReadyButton()
    {
        isReady = true;
        GameReadyButton.SetActive(false);
    }
}
