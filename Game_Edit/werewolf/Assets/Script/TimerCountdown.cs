using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerCountdown : MonoBehaviour
{
    GameObject TextDisplay; // hiện thị thời gian trên màn hình
    int secondsLeft = 0; // thời gian còn lại
    bool takingAway = false; // 
    // Start is called before the first frame update
    void Start()
    {
        TextDisplay = GameObject.FindGameObjectWithTag(Tags_4_Object.Vote_Time);
        TextDisplay.GetComponent<Text>().text = "" + secondsLeft;
    }

    // Update is called once per frame
    void Update()
    {
        if(takingAway == false && secondsLeft > 0)
        {
            StartCoroutine(TimerTake());
        }
        else if(secondsLeft == 0)
        {
            TextDisplay.GetComponent<Text>().text = "";
        }
    }

    IEnumerator TimerTake() // giảm 1 giây sử dụng hàm waitforsecond sau đó giảm secondLeft đi 1 đơn vị
    {
        takingAway = true;
        yield return new WaitForSeconds(1);
        secondsLeft--;
        TextDisplay.GetComponent<Text>().text = ""+secondsLeft;
        takingAway = false;
    }

    public int getSecondsLeft() 
    {
        return secondsLeft;
    }

    public void setSecondsLeft(int seconds)
    {
        secondsLeft = seconds + 1;
    }
}
