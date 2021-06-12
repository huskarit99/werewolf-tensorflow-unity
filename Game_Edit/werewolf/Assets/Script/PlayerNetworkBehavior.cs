using Mirror;
using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using Socket.Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Socket.Quobject.SocketIoClientDotNet.Client;
public class ConnectServer
{
    public string Username { get; set; }
}
public class DetectFinger
{
    public string Username { get; set; }
    public string ResultDetect { get; set; }
}
public class Player
{
    public string Username { get; set; }
    public string Fullname { get; set; }
}
public class DetailRoom
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Wolf { get; set; }
    public string Witch { get; set; }
    public string Guard { get; set; }
    public string Hunter { get; set; }
    public List<Player> Member { get; set; }
}
public partial class PlayerNetworkBehavior : NetworkBehaviour
{
    public QSocket socket { get; set; }
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
    public string Role = Role4Player.Human;
    [SyncVar]
    public bool IsKing;
    [SyncVar]
    public bool IsReady = false; // Trạng thái sẵn sàng của player
    [SyncVar]
    public bool IsStart = false; // Trạng thái bắt đầu để kiểm tra tất cả player đã sẵn sàng hay chưa  
    [SyncVar]
    public bool IsSkipVote = false; // Trạng thái skip vote của player

    string IndexOfPlayerVoted = string.Empty; // Số thứ tự player bị vote


    Vector3 target;
    DieAfterTime DieAfterTime; // Chết sau bao nhiêu giây
    UIGameVote UIGameVote; // UI hiển thị thời gian để vote
    public int GetTimeVote()
    {
        if (UIGameVote == null)
        {
            return 0;
        }
        return UIGameVote.getSecondsLeft();
    }

    UIGameVoted UIGameVoted; // UI hiển thị số lượng bị vote
    UIGameReady UIGameReady; // UI hiển thị button ready
    UIGameSleep UIGameSleep; // UI hiển thị panel sleep
    UIGameTurn UIGameTurn; // UI hiển thị tên của lượt chơi
    UIGameWin UIGameWin; // UI hiển thị khi player giành chiến thắng
    UIGameRole UIGameRole; // UI Hiển thị vai trò của player
    UIGameDay UIGameDay; // UI hiển thị ngày
    UIGameListPlayer UIGameListPlayer; // UI hiển thị danh sách người chơi

    public GameObject CentralPoint;
    bool IsDefault;
    GameObject VotedTarget = null;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start");
        this.socket = IO.Socket("https://werewolf-tensorflow-server.herokuapp.com");
        this.socket.On("server:detail-room", data => {
            DetailRoom detailRoom = (DetailRoom)JsonConvert.DeserializeObject<DetailRoom>(data.ToString());
            Debug.Log("Id : " + detailRoom.Id);
            Debug.Log("Name : " + detailRoom.Name);
            Debug.Log("Wolf : " + detailRoom.Wolf);
            Debug.Log("Witch : " + detailRoom.Witch);
            Debug.Log("Guard : " + detailRoom.Guard);
            Debug.Log("Hunter : " + detailRoom.Hunter);
            Debug.Log("Member : " + detailRoom.Member[0].Username + " " + detailRoom.Member[0].Fullname);
        });
        if (isLocalPlayer)
        {
            IsDefault = true;
            //Cmd_SetupPlayer("Minh Huy");

            DieAfterTime = FindObjectOfType<DieAfterTime>();
            UIGameVoted = FindObjectOfType<UIGameVoted>();
            UIGameVote = FindObjectOfType<UIGameVote>();
            UIGameReady = FindObjectOfType<UIGameReady>(); 
            UIGameSleep = FindObjectOfType<UIGameSleep>();
            UIGameTurn = FindObjectOfType<UIGameTurn>();
            UIGameWin = FindObjectOfType<UIGameWin>();
            UIGameRole = FindObjectOfType<UIGameRole>();
            UIGameDay = FindObjectOfType<UIGameDay>();
            UIGameListPlayer = FindObjectOfType<UIGameListPlayer>();
            // // định danh id cho player Player(Clone)
            this.transform.LookAt(CentralPoint.transform);
        }
    }

    #region Set up character
    /// <summary>
    /// Các hàm set up nhân vật game
    /// </summary>
    [Command]
    void Cmd_SetupPlayer(string _name)
    {
        playerName = _name;
    }

    [Command]
    void Cmd_SetupPosition(int _index)
    {
        var total = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player).Length;
        if (total > 0)
        {
            var default_angle = 360 / total;
            var angle = Math.PI * default_angle * (_index - 1) / 180;
            this.transform.position = new Vector3(Convert.ToSingle(Radius * Math.Sin(angle)),
                                        0,
                                        Convert.ToSingle(Distance + Radius * Math.Cos(angle)));
            Rpc_SetupPosition(_index);
        }
    }
    [ClientRpc]
    void Rpc_SetupPosition(int _index)
    {
        var total = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player).Length;
        if (total > 0)
        {
            var default_angle = 360 / total;
            var angle = Math.PI * default_angle * (_index - 1) / 180;
            this.transform.position = new Vector3(Convert.ToSingle(Radius * Math.Sin(angle)),
                                        0,
                                        Convert.ToSingle(Distance + Radius * Math.Cos(angle)));
        }
    }
    #endregion

    #region NameTag
    /// <summary>
    /// Các hàm thay đổi của tên và đặc điểm nhân vật 
    /// </summary>
    //--- Thay đổi tên nhân vật
    public TextMesh playerNameText;
    [SyncVar(hook = nameof(OnNameChange))]
    public string playerName;
    void OnNameChange(string _old, string _new)
    {
        playerNameText.text = playerName;
    }
    //--- Thay đổi số thứ tự nhân vật
    public TextMesh playerIndexText;
    [SyncVar(hook = nameof(OnIndexChange))]
    public int index;
    void OnIndexChange(int _old,int _new)
    {
        playerIndexText.text = index.ToString();
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

    #endregion

    #region Action
    private bool IsProcessVoting = false;
    GameObject Vote()
    {
        IsDefault = false;
        /*Ray ray = CameraPlayer.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            target = hit.point;
            if (hit.transform.tag.Equals(Tags_4_Object.Player) && !AnimPlayer.GetBool(Param_4_Anim.VoteYourSelf))
            {
                CheckVote(VotedTarget);
                var _target = hit.collider.gameObject;
                Debug.Log(_target.GetComponent<NetworkIdentity>().netId.ToString());
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
        }*/
        if (this.IndexOfPlayerVoted == "Dislike") // Hành động bỏ vote
        {
            CheckVote(VotedTarget);
            this.IndexOfPlayerVoted = string.Empty;
            CancelVote(VotedTarget);
            VotedTarget = null;
        }
        else if (this.IndexOfPlayerVoted == "Like" && !AnimPlayer.GetBool(Param_4_Anim.VoteLeft) && !AnimPlayer.GetBool(Param_4_Anim.VoteYourSelf)) // hành động bỏ qua lượt vote
        {
            CheckVote(VotedTarget);
            this.IndexOfPlayerVoted = string.Empty;
            Cmd_SkipVote(true);
            VotedTarget = null;
        }
        else if (this.index.ToString() == this.IndexOfPlayerVoted) // Hành động player tự vote chính mình 
        {
            this.IndexOfPlayerVoted = string.Empty;
            CheckVote(VotedTarget);
            Cmd_UpdateVotes(gameObject.GetComponent<NetworkIdentity>(), true);
            AnimPlayer.SetBool(Param_4_Anim.VoteLeft, false);
            AnimPlayer.SetBool(Param_4_Anim.VoteYourSelf, true);
            this.IndexOfPlayerVoted = string.Empty;
            VotedTarget = this.gameObject;
            return gameObject;
        }
        else // Hành động player vote 
        {
            if (VotedTarget != null)
            {
                CheckVote(VotedTarget);
            }
            var player = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player)
            .Where(t => t.GetComponent<PlayerNetworkBehavior>().index == Int32.Parse(this.IndexOfPlayerVoted)).FirstOrDefault();
            this.IndexOfPlayerVoted = string.Empty;
            if (player != null)
            {
                Cmd_UpdateVotes(player.GetComponent<NetworkIdentity>(), true);
                AnimPlayer.SetBool(Param_4_Anim.VoteYourSelf, false);
                AnimPlayer.SetBool(Param_4_Anim.VoteLeft, true);
                this.transform.LookAt(new Vector3(player.transform.position.x, 0, player.transform.position.z));
                this.IndexOfPlayerVoted = string.Empty;
                VotedTarget = player;
                Debug.Log(VotedTarget);
                return player;
            }
            else // Nếu ko tìm thấy player thì sẽ trả về mục tiêu vote trước đó
            {
                StopDetecting = false;
                this.IndexOfPlayerVoted = string.Empty;
                return VotedTarget;
            }
        }
        StopDetecting = false;
        this.IndexOfPlayerVoted = string.Empty;
        return null;
    }

    void CheckVote(GameObject _votedTarget)
    {
        if (AnimPlayer.GetBool(Param_4_Anim.VoteLeft) || AnimPlayer.GetBool(Param_4_Anim.VoteYourSelf))
        {
            if (_votedTarget != null)
            {
                var _votes = _votedTarget.GetComponent<PlayerNetworkBehavior>().votes;
                if (_votes > 0)
                {
                    Cmd_UpdateVotes(_votedTarget.GetComponent<NetworkIdentity>(), false);
                }
            }
        } 
        VotedTarget = null;
    }

    void CancelVote(GameObject _votedTarget)
    {
        IsDefault = true;
        if (AnimPlayer.GetBool(Param_4_Anim.VoteLeft) || AnimPlayer.GetBool(Param_4_Anim.VoteYourSelf))
        {
            if (_votedTarget != null)
            {
                var _votes = _votedTarget.GetComponent<PlayerNetworkBehavior>().votes;
                if (_votes > 0)
                {
                    Cmd_UpdateVotes(_votedTarget.GetComponent<NetworkIdentity>(), false);
                }
            }
        }
        AnimPlayer.SetBool(Param_4_Anim.VoteLeft, false); // bỏ thực hiện hành động vote
        AnimPlayer.SetBool(Param_4_Anim.VoteYourSelf, false); // bỏ thực hiện hành động tự vote
        VotedTarget = null;
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
        var currentScene = SceneManager.GetActiveScene();
        if (players.Length > 0)
        {
            var _player = players.Where(t => t.GetComponent<NetworkIdentity>().netId == _target.netId).FirstOrDefault();
            if (_player != null)
            {
                if (_isAddVote)
                {
                    if (IsKing == true && currentScene.name == GameScene.SampleScene)
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
                    if (IsKing == true && currentScene.name == GameScene.SampleScene)
                    {
                        _player.GetComponent<PlayerNetworkBehavior>().votes -= 1.5;
                    }
                    else
                    {
                        _player.GetComponent<PlayerNetworkBehavior>().votes -= 1; 
                    }
                    if (_player.GetComponent<PlayerNetworkBehavior>().votes < 0)
                    {
                        _player.GetComponent<PlayerNetworkBehavior>().votes = 0; 
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
                Debug.Log(_votes);
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
    #endregion
}