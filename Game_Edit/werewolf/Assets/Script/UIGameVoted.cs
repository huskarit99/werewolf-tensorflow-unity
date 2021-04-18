using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameVoted : MonoBehaviour
{
    public Text VotedText;
    public void SetVotedText(double voted)
    {
        if (VotedText != null)
        {
            VotedText.text = "Voted: " + voted;
        }
    }
    public void SetDefaultVotedText()
    {
        if (VotedText != null)
        {
            VotedText.text = "";
        }
    }
}
