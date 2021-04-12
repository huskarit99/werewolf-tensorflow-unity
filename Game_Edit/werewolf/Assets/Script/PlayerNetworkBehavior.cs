using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

public partial class PlayerNetworkBehavior : NetworkBehaviour
{

    public double Radius;
    public double Distance;
    //--- Các thành phần của playerObject
    public Animator AnimPlayer;  // Hành động của nhân vật
    public Camera CameraPlayer; // Camera theo nhân vật
    public GameObject NameTag;
    public GameObject VoteText; // Số vote của nhân vật
    public GameObject NameText; // Tên của nhân vật
    public GameObject IndexText; // Index của nhân vật

    //--- Chức năng của player
    [SyncVar]
    public string Role;
    [SyncVar]
    public bool IsKing;

    Vector3 target;
    DieAfterTime DieAfterTime; // Chết sau bao nhiêu giây
    UIGameVote UIGameVote; // UI hiển thị thời gian để vote
    UIGameVoted UIGameVoted; // UI hiển thị số lượng bị vote

    public GameObject CentralPoint;
    bool IsDefault;
    GameObject VotedTarget = null;
    // Start is called before the first frame update
    void Start()
    {
        if (isLocalPlayer)
        {
            Debug.Log(Role);

            IsDefault = true;
            Cmd_SetupPlayer("Minh Huy", 3);
            Cmd_SetupPosition(3, 4);

            DieAfterTime = FindObjectOfType<DieAfterTime>();
            UIGameVoted = FindObjectOfType<UIGameVoted>();
            UIGameVote = FindObjectOfType<UIGameVote>();
            // // định danh id cho player Player(Clone)
            string _ID = "Player" + netId;
            transform.name = _ID;
            this.transform.LookAt(CentralPoint.transform);
        }
    }

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
    double votes = 0;
    public double GetVotes()
    {
        return this.votes;
    }
    void OnVotesChange(double _old,double _new)
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
    /// <summary>
    /// Hàm Command được call ở Client và thực hiện ở Server
    ///     1/- Tìm kiếm player bị vote thông qua netId
    ///     2/- Nếu là vote thì số votes ++, ngược lại là votes --(Giá trị vote của player ở Server)
    ///     3/- Nếu là king thì sẽ được 1.5 votes
    ///     4/- Cập nhật số vote ở Game Object ở Server
    /// </summary>
    /// <param name="_target">netId của player bị vote</param>
    /// <param name="_isAddVote">Giá trị để chỉ định thay đổi lượt vote</param>
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
                    if (IsKing == true)
                    {
                        _player.GetComponent<PlayerNetworkBehavior>().votes += 1.5;
                    }
                    else
                    {
                        _player.GetComponent<PlayerNetworkBehavior>().votes += 1;
                    }                   
                }
                else
                {
                    if (IsKing == true)
                    {
                        _player.GetComponent<PlayerNetworkBehavior>().votes -= 1.5;
                    }
                    else
                    {
                        _player.GetComponent<PlayerNetworkBehavior>().votes -= 1;
                    }
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
    /// <summary>
    /// Hàm Command được call ở Server và thực hiện ở Client
    ///     1/- Tìm kiếm player bị vote thông qua netId
    ///     2/- Nếu là vote thì số votes ++, ngược lại là votes --(Giá trị vote của player ở tất cả Client)
    ///     3/- Nếu là king thì sẽ được 1.5 votes
    ///     4/- Cập nhật số vote ở Game Object ở Client
    /// *** Sẽ được gọi ngay sau hàm Cmd_UpdateVotes
    /// </summary>
    /// <param name="_target">netId của player bị vote</param>
    /// <param name="_isAddVote">Giá trị để chỉ định thay đổi lượt vote</param>
    /// <param name="_votes">Giá trị votes ở Server</param>
    [ClientRpc]
    void Rpc_UpdateVotes(NetworkIdentity _target, bool _isAddVote,double _votes)
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

    [Command]
    public void Cmd_VoteTime(int seconds) // Thiết lập time vote từ client và đồng bộ lên server
    {
        UIGameVote = FindObjectOfType<UIGameVote>();
        UIGameVote.setSecondsLeft(seconds);
        //Rpc_VoteTime(seconds);
    }

    [ClientRpc]
    void Rpc_VoteTime(int seconds) 
    {
        UIGameVote = FindObjectOfType<UIGameVote>();
        UIGameVote.setSecondsLeft(seconds);
    }
    #endregion
}
