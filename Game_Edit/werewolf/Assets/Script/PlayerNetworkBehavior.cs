using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerNetworkBehavior : NetworkBehaviour
{

    public double Radius;
    public double Distance;
    public Animator AnimPlayer;  // Hành động của nhân vật
    public Camera CameraPlayer; // Camera theo nhân vật
    public GameObject VoteText; // Số vote của nhân vật
    Vector3 target;
    DieAfterTime DieAfterTime; // Chết sau bao nhiêu giây

    public GameObject CentralPoint;
    bool IsDefault;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");
        if (isLocalPlayer)
        {
            IsDefault = true;
            Cmd_SetupPlayer("Minh Hoang 9", 9,0);
            Cmd_SetupPosition(9, 10);
            DieAfterTime = FindObjectOfType<DieAfterTime>();
            // // định danh id cho player Player(Clone)
            string _ID = "Player" + netId;
            transform.name = _ID;
            this.transform.LookAt(CentralPoint.transform);
        }
    }
    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority) { return; }  // kiểm tra quyền client
        if (isLocalPlayer)
        {
            VoteText.SetActive(!IsDefault);

            float moveHor = Input.GetAxis("Horizontal");
            float moveVer = Input.GetAxis("Vertical");
            var movement = new Vector3(moveHor, 0, moveVer);
            transform.position += movement;
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Vote");
                Vote();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                CancelVote(Param_4_Anim.VoteLeft);
            }
            else
            {
                if (IsDefault)
                {
                    this.transform.LookAt(CentralPoint.transform);
                }
            }
        }
    }
    #region Client
    #endregion

    #region Set up character
    /// <summary>
    /// Các hàm set up nhân vật game
    /// </summary>
    [Command]
    void Cmd_SetupPlayer(string _name ,int _index,int _votes)
    {
        playerName = _name;
        index = _index.ToString();
        votes = _votes;
        Rpc_SetupVotes4Player();
    }
    [ClientRpc]
    void Rpc_SetupVotes4Player()
    {
        Debug.Log(votes);
        if (votes == 0)
        {
            VoteText.SetActive(false);
        }
        else
        {
            VoteText.SetActive(true);
        }
    }

    [Command]
    void Cmd_SetupPosition(int _index,int total)
    {
        var default_angle = 360 / total;
        var angle = Math.PI * default_angle * _index / 180;
        playerPosition = new Vector3(Convert.ToSingle(Radius * Math.Sin(angle)),
                                    0,
                                    Convert.ToSingle(Distance + Radius * Math.Cos(angle)));
    }
    #endregion

    #region NameTag
    /// <summary>
    /// Các hàm thay đổi của tên và đặc điểm nhân vật 
    /// </summary>
    //--- Thay đổi tên nhân vật
    public TextMesh playerNameText;
    [SyncVar(hook = nameof(OnNameChange))]
    string playerName;
    void OnNameChange(string _old, string _new)
    {
        playerNameText.text = playerName;
    }
    //--- Thay đổi số thứ tự nhân vật
    public TextMesh playerIndexText;
    [SyncVar(hook = nameof(OnIndexChange))]
    string index;
    void OnIndexChange(string _old,string _new)
    {
        playerIndexText.text = index;
    }
    //-- Thay đổi vote của nhân vật
    public TextMesh playerVotesText;
    [SyncVar(hook = nameof(OnVotesChange))]
    int votes;
    void OnVotesChange(int _old,int _new)
    {
        playerVotesText.text = votes.ToString();
    }

    [SyncVar(hook =nameof(OnPositionChange))]
    Vector3 playerPosition;
    void OnPositionChange(Vector3 _old, Vector3 _new)
    {
        this.gameObject.transform.position = playerPosition;
    }
    #endregion

    #region Action
    void Vote()
    {
        IsDefault = false;
        Ray ray = CameraPlayer.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            target = hit.point;
            if (hit.transform.tag.Equals(Tags_4_Object.Player))
            {
                // thực hiện hành động vote
                AnimPlayer.SetBool(Param_4_Anim.VoteLeft, true);
                this.transform.LookAt(new Vector3(target.x,0,target.z));
                var _target = hit.collider.gameObject;
                Debug.Log(_target.GetComponent<PlayerNetworkBehavior>().votes);
            }
            else
            {
                CancelVote(Tags_4_Object.Player);
            }
        }
    }
    void CancelVote(string param)
    {
        IsDefault = true;
        DieAfterTime.SetNamePlayer_SecondsLeft(null, 0); // gán tên nhân vật và thời gian còn lại
        AnimPlayer.SetBool(param, false); // bỏ thực hiện hành động vote 
    }
    #endregion
}
