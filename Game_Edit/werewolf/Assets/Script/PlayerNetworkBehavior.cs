using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public class PlayerNetworkBehavior : NetworkBehaviour
{

    public double Radius;
    public double Distance;

    public Animator AnimPlayer;  // Hành động của nhân vật
    public Camera CameraPlayer; // Camera theo nhân vật
    public GameObject NameTag;
    public GameObject VoteText; // Số vote của nhân vật
    public GameObject NameText; // Tên của nhân vật
    public GameObject IndexText; // Index của nhân vật

    Vector3 target;
    DieAfterTime DieAfterTime; // Chết sau bao nhiêu giây
    UIGameVote UIGameVote; // Thời gian để vote

    public GameObject CentralPoint;
    bool IsDefault;
    GameObject VotedTarget = null;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");
        if (isLocalPlayer)
        {
            IsDefault = true;
            Cmd_SetupPlayer("Thai Hoc", 3);
            Cmd_SetupPosition(3, 4);
            //Cmd_UpdateVotes(this.GetComponent<NetworkIdentity>(), 0);

            DieAfterTime = FindObjectOfType<DieAfterTime>();
            UIGameVote = FindObjectOfType<UIGameVote>();
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
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        foreach(var player in players)
        {
            player.GetComponent<PlayerNetworkBehavior>().NameTag.transform.LookAt(this.CameraPlayer.transform);
        }
        if (isLocalPlayer)
        {
            this.NameTag.SetActive(false);
            float moveHor = Input.GetAxis("Horizontal");
            float moveVer = Input.GetAxis("Vertical");
            var movement = new Vector3(moveHor, 0, moveVer);
            transform.position += movement;
            if (Input.GetMouseButtonDown(0) && UIGameVote.getSecondsLeft()>0)
            {
                Debug.Log("Vote");
                VotedTarget = Vote();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                CancelVote(VotedTarget);
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
    void Cmd_SetupPlayer(string _name ,int _index)
    {
        playerName = _name;
        index = _index.ToString();
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
    int votes = 0;
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
    GameObject Vote()
    {
        IsDefault = false;
        Ray ray = CameraPlayer.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            target = hit.point;
            if (hit.transform.tag.Equals(Tags_4_Object.Player))
            {
                var _target = hit.collider.gameObject;
                Cmd_UpdateVotes(_target.GetComponent<NetworkIdentity>(), true);
                // thực hiện hành động vote
                AnimPlayer.SetBool(Param_4_Anim.VoteLeft, true);
                this.transform.LookAt(new Vector3(target.x, 0, target.z));
                return _target;
            }
            else
            {
                CancelVote(VotedTarget);
            }
        }
        return null;
    }
    void CancelVote(GameObject _votedTarget)
    {
        IsDefault = true;
        if (AnimPlayer.GetBool(Param_4_Anim.VoteLeft))
        {
            var _votes = _votedTarget.GetComponent<PlayerNetworkBehavior>().votes;
            if (_votes > 0)
            {
                Cmd_UpdateVotes(_votedTarget.GetComponent<NetworkIdentity>(), false);
            }   
        }
        AnimPlayer.SetBool(Param_4_Anim.VoteLeft, false); // bỏ thực hiện hành động vote
    }

    [Command]
    void Cmd_UpdateVotes(NetworkIdentity _target,bool _isAddVote)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        if (players.Length > 0)
        {
            var _player = players.Where(t => t.GetComponent<NetworkIdentity>().netId == _target.netId).FirstOrDefault();
            if (_player != null)
            {
                if (_isAddVote)
                {
                    _player.GetComponent<PlayerNetworkBehavior>().votes++;
                }
                else
                {
                    _player.GetComponent<PlayerNetworkBehavior>().votes--;
                }
                if (_player.GetComponent<PlayerNetworkBehavior>().votes == 0)
                {
                    _player.GetComponent<PlayerNetworkBehavior>().VoteText.SetActive(false);
                }
                else
                {
                    _player.GetComponent<PlayerNetworkBehavior>().VoteText.SetActive(true);
                }
                var _votes = _player.GetComponent<PlayerNetworkBehavior>().votes;
                _player.GetComponent<PlayerNetworkBehavior>().playerVotesText.text = _votes.ToString();
                Debug.Log("Server: " + _votes.ToString() + " " + _player.GetComponent<PlayerNetworkBehavior>().playerVotesText.text);
                Rpc_UpdateVotes(_target,_isAddVote,_votes);
            }
        }
    }
    [ClientRpc]
    void Rpc_UpdateVotes(NetworkIdentity _target, bool _isAddVote,int _votes)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        if (players.Length > 0)
        {
            var _player = players.Where(t => t.GetComponent<NetworkIdentity>().netId == _target.netId).FirstOrDefault();
            if (_player != null)
            {
                _player.GetComponent<PlayerNetworkBehavior>().votes = _votes;
                _player.GetComponent<PlayerNetworkBehavior>().playerVotesText.text = _votes.ToString();
                if (_player.GetComponent<PlayerNetworkBehavior>().votes == 0)
                {
                    _player.GetComponent<PlayerNetworkBehavior>().VoteText.SetActive(false);
                }
                else
                {
                    _player.GetComponent<PlayerNetworkBehavior>().VoteText.SetActive(true);
                }
            }
        }
    }
    #endregion
}
