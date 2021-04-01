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
    public GameObject VoteText; // Số vote của nhân vật
    Vector3 target;
    Vector3 defaultPosition;
    DieAfterTime DieAfterTime; // Chết sau bao nhiêu giây

    public GameObject CentralPoint;
    MyNetworkManager Room;
    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            SetupPlayer("Minh Quoc", 1, 10, 0);
            defaultPosition = gameObject.transform.rotation.eulerAngles;

            DieAfterTime = FindObjectOfType<DieAfterTime>();
            // // định danh id cho player Player(Clone)
            string _ID = "Player" + GetComponent<NetworkIdentity>().netId;
            transform.name = _ID;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!hasAuthority) { return;  }  // kiểm tra quyền client
        if (isLocalPlayer)
        {
            UpdateVotes(votes);
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vote();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CancelVote(Param_4_Anim.VoteLeft);
        }
    }

    #region Client
    #endregion

    #region Set up character
    /// <summary>
    /// Các hàm set up nhân vật game
    /// </summary>
    [Command]
    void SetupPlayer(string _name ,int _index, int total,int _votes)
    {
        name = _name;
        index = _index.ToString();
        SetupPosition(_index, total);
        SetupVoteText(_votes);
    }
    void SetupPosition(int _index,int total)
    {
        var default_angle = 360 / total;
        var angle = Math.PI * default_angle * _index / 180;
        if (isLocalPlayer)
        {
            Radius += Distance4Main;
        }
        playerPosition = new Vector3(Convert.ToSingle(Radius * Math.Sin(angle)),
                                    0,
                                    Convert.ToSingle(Distance + Radius * Math.Cos(angle)));
        var centralPoint = GameObject.FindGameObjectWithTag(Tags_4_Object.CentralPoint).transform;
        this.gameObject.transform.LookAt(centralPoint);
    }
    void SetupVoteText(int _votes)
    {
        votes = _votes;
        VoteText.SetActive(false);
    }
    #endregion

    #region NameTag
    /// <summary>
    /// Các hàm thay đổi của tên và đặc điểm nhân vật 
    /// </summary>
    //--- Thay đổi tên nhân vật
    public TextMesh playerNameText;
    [SyncVar(hook = nameof(CmdNameChange))]
    string name;
    void CmdNameChange(string _old, string _new)
    {
        playerNameText.text = name;
    }
    //--- Thay đổi số thứ tự nhân vật
    public TextMesh playerIndexText;
    [SyncVar(hook = nameof(CmdIndexChange))]
    string index;
    void CmdIndexChange(string _old,string _new)
    {
        playerIndexText.text = index;
    }
    //-- Thay đổi vote của nhân vật
    public TextMesh playerVotesText;
    [SyncVar(hook = nameof(CmdVotesChange))]
    int votes;
    void CmdVotesChange(int _old,int _new)
    {
        playerVotesText.text = votes.ToString();
    }

    [SyncVar(hook =nameof(CmdPositionChange))]
    Vector3 playerPosition;
    void CmdPositionChange(Vector3 _old, Vector3 _new)
    {
        this.gameObject.transform.position = playerPosition;
    }
    #endregion

    #region Action
    [Command]
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
    [Command]
    void UpdateVotes(int _votes)
    {
        this.votes += _votes;
        if (votes == 0)
        {
            this.VoteText.SetActive(false);
        }
        else
        {
            this.VoteText.SetActive(true);
        }
    }
    #endregion
}
