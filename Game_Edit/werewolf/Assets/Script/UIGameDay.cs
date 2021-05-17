using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameDay : MonoBehaviour
{
    public Text DayText;
    public void SetDayText(int _day)
    {
        if (DayText != null)
        {
            DayText.text = "Day: " + _day;
        }
    }
}
