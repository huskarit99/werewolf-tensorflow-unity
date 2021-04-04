using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class DieAfterTime : MonoBehaviour
{
    GameObject TextDisplay; // Hiển thị thời gian còn lại 
    int secondsLeft = 0;
    string NamePlayer = null; // Tên của player
    bool takingAway = false;
    // Start is called before the first frame update
    void Start()
    {
        TextDisplay = GameObject.FindGameObjectWithTag(Tags_4_Object.Vote_Time);
        TextDisplay.GetComponent<Text>().text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (takingAway == false && secondsLeft > 0 && NamePlayer!= null)
        {
            StartCoroutine(TimerTake());
        }
        else if (secondsLeft == 0)
        {
            Destroy(GameObject.Find(NamePlayer));
            SetNamePlayer_SecondsLeft(null, -1);
            TextDisplay.GetComponent<Text>().text = "";
        }
    }

    IEnumerator TimerTake() // giảm 1 giây sử dụng hàm waitforsecond sau đó giảm secondLeft đi 1 đơn vị
    {
        takingAway = true;
        yield return new WaitForSeconds(1);
        secondsLeft--;
        TextDisplay.GetComponent<Text>().text = "" + secondsLeft;
        takingAway = false;
    }

    public void SetNamePlayer_SecondsLeft(string name, int seconds) // Gán tên player và số giây
    {
        NamePlayer = name;
        secondsLeft = seconds + 1;
    }
}
