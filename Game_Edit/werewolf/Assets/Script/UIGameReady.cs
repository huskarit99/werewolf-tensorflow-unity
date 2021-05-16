using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class UIGameReady : NetworkBehaviour
{
    public GameObject GameReadyButton; //  Button sẵn sàng
    public GameObject GameReadyPanel; // Panel sẵn sàng
    public GameObject GameReadyInputField; // InputField nhập tên nhân vật
    public Text GameReadyInputText; // Text tên nhân vật
    bool IsReady = false; // Trạng thái sẵn sàng
    string playerName = string.Empty; // Tên player
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

    public string GetPlayerName()
    {
        return playerName;
    }

    public void ReadyButton() // Hành động ấn button
    {
        if (GameReadyButton != null && GameReadyInputField != null)
        {
            if(GameReadyInputText.GetComponent<Text>().text != string.Empty)
            {
                IsReady = true; // Chuyển trạng thái sẵn sàng thành true
                playerName = GameReadyInputText.GetComponent<Text>().text; // gán player name từ input
                GameReadyButton.SetActive(false); // Ẩn button
                GameReadyInputField.SetActive(false); // Ẩn input field
            }
        }
    }
}
