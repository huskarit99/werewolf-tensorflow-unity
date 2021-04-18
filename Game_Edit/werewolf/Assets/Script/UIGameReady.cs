using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class UIGameReady : NetworkBehaviour
{
    public GameObject GameReadyButton; //  Button sẵn sàng
    public GameObject GameReadyPanel; // Panel sẵn sàng
    bool IsReady = false; // Trạng thái sẵn sàng

    public void ShowReadyButton(bool isShow)  // hiện hoặc ẩn button
    {
        if (GameReadyButton != null)
        {
            GameReadyButton.SetActive(isShow);
        }
    }

    public void ShowReadyPanel(bool isShow) // Hiện hoặc ẩn panel
    {
        if (GameReadyPanel != null)
        {
            GameReadyPanel.SetActive(isShow);
        }
    }
    public bool GetIsReady()
    {
        return IsReady;
    }

    public void SetIsReady(bool _isReady)
    {
        IsReady = _isReady;
    }

    public void ReadyButton() // Hành động ấn button
    {
        if (GameReadyButton != null)
        {
            IsReady = true; // Chuyển trạng thái sẵn sàng thành true
            GameReadyButton.SetActive(false); // Ẩn button
        }
    }
}
