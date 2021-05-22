using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;


public class UIGameVote : NetworkBehaviour
{
    public Text TextDisplay; // hiện thị thời gian trên màn hình
    [SyncVar(hook = nameof(OnSecondsChanged))]
    int secondsLeft = 100; // thời gian còn lại
    void OnSecondsChanged(int _old, int _new)
    {
        TextDisplay.text = secondsLeft.ToString();
    }

    int secondsWait = 0; // thời gian chờ
    bool takingAway = false; // 

    [SyncVar]
    private bool IsReady4ResetTime = true;
    public bool GetReady4ResetTime()
    {
        return this.IsReady4ResetTime;
    }
    public void SetReady4ResetTime(bool _new)
    {
        this.IsReady4ResetTime = _new;
    }

    [SyncVar]
    private bool IsAllVote = false; // Trạng thái tất cả player đều vote
    public bool GetAllVote()
    {
        return this.IsAllVote;
    }
    public void SetAllVote(bool _vote)
    {
        this.IsAllVote = _vote;
    }

    // Start is called before the first frame update
    void Start()
    {
        //TextDisplay = GameObject.FindGameObjectWithTag(Tags_4_Object.Vote_Time);
        //TextDisplay.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        /*if (takingAway == false && secondsLeft > 0 && secondsWait == 0 && wait == false)
        {
            StartCoroutine(TimerTake());
        }
        else if (secondsLeft == 0 && wait == false) // Thiết lập thời gian chờ
        {
            secondsWait = 5;
            wait = true;
        }
        else if (takingAway == false && secondsWait > 0 && wait == true)
        {
            TextDisplay.text = "";
            StartCoroutine(WaitingTime());
        }
        else if (secondsWait == 0 && secondsLeft == 0 && wait == true) // Thiết lập thời gian vote
        {
            secondsLeft = 30;
            wait = false;
        }*/
        if (takingAway == false && secondsLeft > 0)
        {
            StartCoroutine(TimerTake());
        }
        else if(secondsLeft == 0)
        {
            TextDisplay.text = "";
        }
    }

    IEnumerator TimerTake() // giảm 1 giây sử dụng hàm waitforsecond sau đó giảm secondLeft đi 1 đơn vị
    {
        takingAway = true;
        yield return new WaitForSeconds(1);
        secondsLeft--;
        TextDisplay.text = secondsLeft.ToString();
        takingAway = false;
    }
    IEnumerator WaitingTime() // giảm 1 giây sử dụng hàm waitforsecond sau đó giảm secondWait đi 1 đơn vị 
    {
        takingAway = true;
        yield return new WaitForSeconds(1);
        secondsWait--;
        takingAway = false;
    }

    public int getSecondsLeft()
    {
        return secondsLeft;
    }
    public void setSecondsLeft(int seconds) // Thiết lập time
    {
        secondsLeft = seconds;
        IsReady4ResetTime = false;
    }
}
