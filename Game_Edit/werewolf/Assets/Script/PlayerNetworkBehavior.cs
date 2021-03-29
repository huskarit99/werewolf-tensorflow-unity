using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerNetworkBehavior : NetworkBehaviour
{

    public double Radius;
    public double Distance;
    public double Distance4Main;
    public Animator AnimPlayer;  // Hành động của nhân vật
    public Camera CameraPlayer; // Camera theo nhân vật
    Vector3 target;
    // Start is called before the first frame update
    void Start()
    {
        SetupPlayer("Minh Hoang", 0,10);
    }

    // Update is called once per frame
    void Update()
    {
        if(!hasAuthority) { return;  }  // kiểm tra quyền client
        if (Input.GetMouseButtonDown(0))
        {
            Vote();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CancelVote(Param_4_Anim.VoteLeft);
        }
    }
    #region Set up character
    /// <summary>
    /// Các hàm set up nhân vật game
    /// </summary>
    [Command]
    void SetupPlayer(string _name ,int _index, int total)
    {
        name = _name;
        index = _index.ToString();
        SetupPosition(_index, total);
        SetupNameTag();
    }
    void SetupPosition(int _index,int total)
    {
        var default_angle = 360 / total;
        var angle = Math.PI * default_angle * _index / 180;
        if (isLocalPlayer)
        {
            Radius += Distance4Main;
        }
        var result = new Vector3(Convert.ToSingle(Radius * Math.Sin(angle)),
                                 0,
                                 Convert.ToSingle(Distance + Radius * Math.Cos(angle)));
        transform.position = result;
        var centralPoint = GameObject.FindGameObjectWithTag(Tags_4_Object.CentralPoint);
        transform.LookAt(centralPoint.transform);
    }
    #endregion

    #region NameTag
    /// <summary>
    /// Các hàm thay đổi của tên và đặc điểm nhân vật 
    /// </summary>
    //--- Thay đổi tên nhân vật
    public TextMesh playerNameText;
    [SyncVar(hook = nameof(OnNameChange))]
    string name;
    void OnNameChange(string _old, string _new)
    {
        playerNameText.text = name;
    }
    //--- Thay đổi số thứ tự nhân vật
    public TextMesh playerIndexText;
    [SyncVar(hook = nameof(OnIndexChange))]
    string index;
    void OnIndexChange(string _old,string _new)
    {
        playerIndexText.text = index;
    }

    void SetupNameTag()
    {
        var nameTag = GameObject.FindGameObjectsWithTag(Tags_4_Object.NameTag);
        foreach(var item in nameTag)
        {
            item.transform.LookAt(Vector3.zero);
        }
    }
    #endregion


    #region Action
    #endregion
    void Vote()
    {
        Ray ray = CameraPlayer.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            target = hit.point;
        }
        transform.LookAt(target); // xoay nhân vật theo mục tiêu của con trỏ
        AnimPlayer.SetBool(Param_4_Anim.VoteLeft, true); // thực hiện hành động vote
    }
    void CancelVote(string param)
    {
        AnimPlayer.SetBool(param, false); // bỏ thực hiện hành động vote 
    }
}
