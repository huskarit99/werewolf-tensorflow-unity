using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameTurn : MonoBehaviour
{
    public Text TurnText; // hiển thị tên lượt chơi

    public void SetTurnText(string _turn)
    {
        if (TurnText != null)
        {
            TurnText.text = _turn;
        }
    }

    public void SetDefaultTurnText()
    {
        if (TurnText != null)
        {
            TurnText.text = "";
        }
    }

}
