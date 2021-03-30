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
    Vector3 defaultPosition;
    DieAfterTime DieAfterTime; // Chết sau bao nhiêu giây
    // Start is called before the first frame update
    void Start()
    {
        SetupPlayer("Minh Hoang", 0,10);
        defaultPosition = gameObject.transform.rotation.eulerAngles;

        DieAfterTime = FindObjectOfType<DieAfterTime>();
        // // định danh id cho player Player(Clone)
        string _ID = "Player" + GetComponent<NetworkIdentity>().netId; 
        transform.name = _ID;
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
    private void LateUpdate()
    {
        var angle = (target.x - gameObject.transform.position.x)%90;
        var neck = AnimPlayer.GetBoneTransform(HumanBodyBones.Neck);
        neck.transform.Rotate(angle/2, 0, 0);
        var upperChest = AnimPlayer.GetBoneTransform(HumanBodyBones.UpperChest);
        upperChest.transform.Rotate(angle / 2, 0, 0);
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
        VotesText.SetActive(false);
    }

    //--- Đồng bộ số votes
    public GameObject VotesText;
    public TextMesh playerVotesText;
    [SyncVar(hook = nameof(OnVotesChange))]
    int Votes;
    void OnVotesChange(int _old,int _new)
    {
        playerVotesText.text = Votes.ToString();
    }
    #endregion


    #region Action
    void Vote()
    {
        Ray ray = CameraPlayer.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            target = hit.point;
            if (hit.transform.tag.Equals(Tags_4_Object.Player))
            {
                AnimPlayer.SetBool(Param_4_Anim.VoteLeft, true);
                DieAfterTime.SetNamePlayer_SecondsLeft(hit.collider.name, 5); // gán tên nhân vật và số thời gian còn lại
                // thực hiện hành động vote
            }
        }
        //transform.LookAt(target); // xoay nhân vật theo mục tiêu của con trỏ

    }
    void CancelVote(string param)
    {
        DieAfterTime.SetNamePlayer_SecondsLeft(null, 0); // gán tên nhân vật và thời gian còn lại
        AnimPlayer.SetBool(param, false); // bỏ thực hiện hành động vote 
    }
    #endregion
}
