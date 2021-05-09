using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class UIGameWin : NetworkBehaviour
{
    public GameObject GameWinText; // Hiển thị thông báo player chiến thắng

    public void ShowWinText(string _playerWin) // gán chuỗi vào text
    {
        if(GameWinText != null)
        {
            GameWinText.SetActive(true);
            GameWinText.GetComponent<Text>().text = _playerWin;
        }
    }
}
