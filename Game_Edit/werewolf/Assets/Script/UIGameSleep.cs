using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameSleep : MonoBehaviour
{
    public GameObject GameSleepPanel; // Panel Sleep

    public void ShowSleepPanel(bool isShow) // Hiện hoặc ẩn panel
    {
        if (GameSleepPanel!= null)
        {
            GameSleepPanel.SetActive(isShow);
        }
    }
}
