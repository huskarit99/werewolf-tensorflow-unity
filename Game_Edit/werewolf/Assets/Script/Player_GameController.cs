using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Linq;

public partial class PlayerNetworkBehavior : NetworkBehaviour
{
    public NetworkManager MyNetworkManager;

    [SyncVar]
    public int Day = 1;
    [SyncVar]
    public string Action = Action4Player.Default;

    #region State
    [SyncVar]
    public bool IsDone = false;

    [SyncVar]
    public bool IsSavedByGuard = false;
    [SyncVar]
    public bool IsSavedByWitch = false;
    [SyncVar]
    public bool IsKilledByWitch = false;
    [SyncVar]
    public bool IsKilled = false;
    [SyncVar]
    public bool IsGuilty = false;
    #endregion


    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority) { return; }  // kiểm tra quyền client
        Cmd_SetupPosition(this.index);
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        foreach (var player in players)
        {
            player.GetComponent<PlayerNetworkBehavior>().NameTag.transform.LookAt(this.CameraPlayer.transform);
            if (player.GetComponent<PlayerNetworkBehavior>().votes == 0)
            {
                player.GetComponent<PlayerNetworkBehavior>().VoteText.SetActive(false);
            }
            if (string.IsNullOrEmpty(player.GetComponent<PlayerNetworkBehavior>().Role))
            {
                player.SetActive(false);
            }
        }
        if (isLocalPlayer)
        {
            this.NameTag.SetActive(false);
            //AnimPlayer = GetComponent<Animator>();
            UIGameVoted = FindObjectOfType<UIGameVoted>();
            UIGameVote = FindObjectOfType<UIGameVote>();
            UIGameReady = FindObjectOfType<UIGameReady>();
            UIGameSleep = FindObjectOfType<UIGameSleep>();
            UIGameTurn = FindObjectOfType<UIGameTurn>();
            // Gán thuộc tính isReady trong UIGameReady vào biến IsReady, IsStart mặc định là false
            if (IsReady == false)
            {
                IsReady = UIGameReady.GetIsReady();
            }
            // Thiết lập trạng thái sẵn sàng của player và đồng bộ lên server
            if (IsReady && !IsStart)
            {
                Cmd_Ready(gameObject.GetComponent<NetworkIdentity>(), IsReady);
            }           
            if(IsReady && IsStart)
            {
                if (UIGameReady != null)
                {
                    UIGameReady.ShowReadyPanel(false); // Ẩn panel khi IsReady và IsStart bằng true
                }
                switch (Day) 
                {
                    case 1:
                        {
                            if (CheckAction4Players(Action4Player.Default))
                            {
                                if (CheckDone4Players(true))
                                {
                                    Cmd_ChangeScene(Action4Player.Default, GameScene.NightScene);
                                    SetupForNewAction(Action4Player.WolfTurn);
                                }
                                else
                                {
                                    Vote4Action(Action4Player.Default);
                                }
                            }
                            else if (CheckAction4Players(Action4Player.WolfTurn))
                            {
                                if (CheckDone4Players(true))
                                {
                                    SetupForNewAction(Action4Player.Default);
                                    Cmd_SetDay4Player(Day + 1);
                                }
                                else
                                {
                                    if (CheckRole(Role4Player.Wolf))
                                    {
                                        if (Role == Role4Player.Wolf)
                                        {
                                            UIGameSleep.ShowSleepPanel(false);
                                            Vote4Action(Action4Player.WolfTurn);
                                        }
                                        else
                                        {
                                            Cmd_SetDone4Player(true);
                                            UIGameSleep.ShowSleepPanel(true);
                                        }
                                    }
                                    else
                                    {
                                        UIGameSleep.ShowSleepPanel(true);
                                        if (UIGameVote.GetReady4ResetTime())
                                        {
                                            Cmd_VoteTime(5);
                                        }
                                        if (UIGameVote.getSecondsLeft() == 0)
                                        {
                                            Cmd_SetDone4Player(true);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case 2 :
                        {
                            if (CheckDay())
                            {
                                if (CheckAction4Players(Action4Player.Default))
                                {
                                    Cmd_ChangeScene(Action4Player.Default, GameScene.SampleScene);
                                    if (CheckDone4Players(true))
                                    {
                                        if (CheckIsGuilty())
                                        {
                                            SetupForNewAction(Action4Player.Guilty);
                                        }
                                        else
                                        {
                                            SetupForNewAction(Action4Player.VoteKing);
                                        }
                                    }
                                    else
                                    {
                                        if (CheckDeath())
                                        {
                                            StartCoroutine(DoKilledPlayer());
                                        }
                                        Vote4Action(Action4Player.Default);
                                    }
                                }
                                else if (CheckAction4Players(Action4Player.VoteKing))
                                {
                                    if (CheckKing())
                                    {
                                        SetupForNewAction(Action4Player.Guilty);
                                    }
                                    else
                                    {
                                        Vote4AKing();
                                    }
                                }
                                else if (CheckAction4Players(Action4Player.Guilty))
                                {
                                    if (CheckDone4Players(true))
                                    {
                                        if (CheckDeath())
                                        {
                                            Cmd_SetGuilty4Player(true);
                                            SetupForNewAction(Action4Player.Default);
                                        }
                                        else
                                        {
                                            Cmd_SetGuilty4Player(false);
                                            SetupForNewAction(Action4Player.GuardTurn);
                                            Cmd_ChangeScene(Action4Player.GuardTurn, GameScene.NightScene);
                                        }
                                        
                                    }
                                    else
                                    {
                                        if (CheckIsGuilty())
                                        {
                                            Cmd_SetDone4Player(true);
                                        }
                                        else
                                        {
                                            Vote4Action(Action4Player.Guilty);
                                        }
                                    }
                                }
                                else if (CheckAction4Players(Action4Player.GuardTurn))
                                {
                                    if (CheckDone4Players(true))
                                    {
                                        SetupForNewAction(Action4Player.SeerTurn);
                                    }
                                    else
                                    {
                                        if (CheckRole(Role4Player.Guard))
                                        {
                                            if (Role == Role4Player.Guard)
                                            {
                                                UIGameSleep.ShowSleepPanel(false);
                                                Vote4Action(Action4Player.GuardTurn);
                                            }
                                            else
                                            {
                                                Cmd_SetDone4Player(true);
                                                UIGameSleep.ShowSleepPanel(true);
                                            }
                                        }
                                        else
                                        {
                                            UIGameSleep.ShowSleepPanel(true);
                                            if (UIGameVote.GetReady4ResetTime())
                                            {
                                                Cmd_VoteTime(5);
                                            }
                                            else
                                            {
                                                if (UIGameVote.getSecondsLeft() == 0)
                                                {
                                                    Cmd_SetDone4Player(true);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (CheckAction4Players(Action4Player.SeerTurn))
                                {
                                    if (CheckDone4Players(true))
                                    {
                                        SetupForNewAction(Action4Player.WolfTurn);
                                    }
                                    else
                                    {
                                        if (CheckRole(Role4Player.Seer))
                                        {
                                            if (Role == Role4Player.Seer)
                                            {
                                                UIGameSleep.ShowSleepPanel(false);
                                                Vote4Action(Action4Player.SeerTurn);
                                            }
                                            else
                                            {
                                                Cmd_SetDone4Player(true);
                                                UIGameSleep.ShowSleepPanel(true);
                                            }
                                        }
                                        else
                                        {
                                            UIGameSleep.ShowSleepPanel(true);
                                            if (UIGameVote.GetReady4ResetTime())
                                            {
                                                Cmd_VoteTime(5);
                                            }
                                            else
                                            {
                                                if (UIGameVote.getSecondsLeft() == 0)
                                                {
                                                    Cmd_SetDone4Player(true);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (CheckAction4Players(Action4Player.WolfTurn))
                                {
                                    if (CheckDone4Players(true))
                                    {
                                        SetupForNewAction(Action4Player.WitchTurn);
                                    }
                                    else
                                    {
                                        if (CheckRole(Role4Player.Wolf))
                                        {
                                            if (Role == Role4Player.Wolf)
                                            {
                                                UIGameSleep.ShowSleepPanel(false);
                                                Vote4Action(Action4Player.WolfTurn);
                                            }
                                            else
                                            {
                                                Cmd_SetDone4Player(true);
                                                UIGameSleep.ShowSleepPanel(true);
                                            }
                                        }
                                        else
                                        {
                                            UIGameSleep.ShowSleepPanel(true);
                                            if (UIGameVote.GetReady4ResetTime())
                                            {
                                                Cmd_VoteTime(5);
                                            }
                                            else
                                            {
                                                if (UIGameVote.getSecondsLeft() == 0)
                                                {
                                                    Cmd_SetDone4Player(true);
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (CheckAction4Players(Action4Player.WitchTurn))
                                {
                                    if (CheckDone4Players(true))
                                    {
                                        SetupForNewAction(Action4Player.Default);
                                        Cmd_SetDay4Player(Day + 1);
                                    }
                                    else
                                    {
                                        if (CheckRole(Role4Player.Witch))
                                        {
                                            if (Role == Role4Player.Witch)
                                            {
                                                UIGameSleep.ShowSleepPanel(false);
                                                Vote4Action(Action4Player.WitchTurn);
                                            }
                                            else
                                            {
                                                Cmd_SetDone4Player(true);
                                                UIGameSleep.ShowSleepPanel(true);
                                            }
                                        }
                                        else
                                        {
                                            UIGameSleep.ShowSleepPanel(true);
                                            if (UIGameVote.GetReady4ResetTime())
                                            {
                                                Cmd_VoteTime(5);
                                            }
                                            else
                                            {
                                                if (UIGameVote.getSecondsLeft() == 0)
                                                {
                                                    Cmd_SetDone4Player(true);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        {
                        }
                        break;
                }
                if (IsDefault)
                {
                    this.transform.LookAt(CentralPoint.transform);
                }
            }
            else if (IsReady)
            {
                UIGameReady.ShowReadyPanel(true); // Hiện panel khi IsStart = false
                Cmd_Start(); // Kiểm tra player đã sẵn sàng hết thì set IsStart = true sau đó vào màn chơi
                if (IsDefault)
                {
                    this.transform.LookAt(CentralPoint.transform);
                }
            }
            else
            {
                UIGameReady.ShowReadyPanel(true); // Hiện panel khi vào game
                if (IsDefault)
                {
                    this.transform.LookAt(CentralPoint.transform);
                }
            }

            
        }
    }

    #region Check_Something
    private bool CheckKing()
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player).Where(t => !string.IsNullOrEmpty(t.GetComponent<PlayerNetworkBehavior>().Role)).ToArray();
        var _check = players.Where(t => t.GetComponent<PlayerNetworkBehavior>().IsKing == true).ToArray();
        if (_check != null && _check.Length > 0)
        {
            return true;
        }
        return false;
    }
    private bool CheckAction4Players(string _action)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player).Where(t => !string.IsNullOrEmpty(t.GetComponent<PlayerNetworkBehavior>().Role)).ToArray();
        var _check = players.Where(t => t.GetComponent<PlayerNetworkBehavior>().Action == _action).ToArray();
        if (_check != null && _check.Length == players.Length)
        {
            return true;
        }
        return false;
    }
    private bool CheckDone4Players(bool _isDone)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player).Where(t => !string.IsNullOrEmpty(t.GetComponent<PlayerNetworkBehavior>().Role)).ToArray();
        var _check = players.Where(t => t.GetComponent<PlayerNetworkBehavior>().IsDone == _isDone).ToArray();
        if (_check != null && _check.Length == players.Length)
        {
            return true;
        }
        return false;
    }
    private bool CheckVote4Player(string _role)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player).Where(t => !string.IsNullOrEmpty(t.GetComponent<PlayerNetworkBehavior>().Role)).ToArray();
        if (players.Length > 0)
        {
            if (_role == Role4Player.Human)
            {
                // Player đã vote
                var _players_Voted = players.Where(t =>
                                                    t.GetComponent<PlayerNetworkBehavior>().AnimPlayer.GetBool(Param_4_Anim.VoteLeft) == true).ToArray();
                // Player bỏ vote
                var _players_SkipVote = players.Where(t =>
                                                    t.GetComponent<PlayerNetworkBehavior>().IsSkipVote == true).ToArray();
                // Player tự vote
                var _players_VoteMySelf = players.Where(t =>
                                                    t.GetComponent<PlayerNetworkBehavior>().AnimPlayer.GetBool(Param_4_Anim.VoteYourSelf) == true).ToArray();
                if (_players_Voted.Length + _players_SkipVote.Length + _players_VoteMySelf.Length == players.Length)
                {
                    return true;
                }
            }
            else
            {
                players = players.Where(t => t.GetComponent<PlayerNetworkBehavior>().Role == _role).ToArray();
                // Player đã vote
                var _players_Voted = players.Where(t =>
                                    t.GetComponent<PlayerNetworkBehavior>().AnimPlayer.GetBool(Param_4_Anim.VoteLeft) == true &&
                                    t.GetComponent<PlayerNetworkBehavior>().Role == _role).ToArray();
                // Player bỏ vote
                var _players_SkipVote = players.Where(t =>
                                    t.GetComponent<PlayerNetworkBehavior>().IsSkipVote == true &&
                                    t.GetComponent<PlayerNetworkBehavior>().Role == _role).ToArray();
                // Player tự vote
                var _players_VoteMySelf = players.Where(t =>
                                    t.GetComponent<PlayerNetworkBehavior>().AnimPlayer.GetBool(Param_4_Anim.VoteYourSelf) == true &&
                                    t.GetComponent<PlayerNetworkBehavior>().Role == _role).ToArray();
                if (_players_Voted.Length + _players_SkipVote.Length + _players_VoteMySelf.Length == players.Length)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private bool CheckRole(string _role)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player).ToArray();
        var _check = players.Where(t => t.GetComponent<PlayerNetworkBehavior>().Role == _role).ToArray();
        if (_check != null && _check.Length > 0)
        {
            return true;
        }
        return false;
    }
    private bool CheckDay()
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player).Where(t => !string.IsNullOrEmpty(t.GetComponent<PlayerNetworkBehavior>().Role)).ToArray();
        var _check = players.Where(t => t.GetComponent<PlayerNetworkBehavior>().Day == players[0].GetComponent<PlayerNetworkBehavior>().Day).ToArray();
        if (_check != null && _check.Length > 0)
        {
            return true;
        }
        return false;
    }

    private bool CheckDeath()
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player).Where(t => !string.IsNullOrEmpty(t.GetComponent<PlayerNetworkBehavior>().Role)).ToArray();
        var _check = players.Where(t => t.GetComponent<PlayerNetworkBehavior>().IsKilled == true).ToArray();
        if (_check != null && _check.Length > 0)
        {
            return true;
        }
        return false;
    }

    private bool CheckOutOfTime()
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player).Where(t => !string.IsNullOrEmpty(t.GetComponent<PlayerNetworkBehavior>().Role)).ToArray();
        var _check = players.Where(t => t.GetComponent<PlayerNetworkBehavior>().UIGameVote.getSecondsLeft() == 0).ToArray();
        if (_check != null && players.Length == _check.Length)
        {
            return true;
        }
        return false;
    }

    private bool CheckIsGuilty()
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player).Where(t => !string.IsNullOrEmpty(t.GetComponent<PlayerNetworkBehavior>().Role)).ToArray();
        var _check = players.Where(t => t.GetComponent<PlayerNetworkBehavior>().IsGuilty == true).ToArray();
        if (_check != null && players.Length == _check.Length)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region GamePlay
    #region Vote 4 A King
    bool Vote4AKing()
    {
        if (UIGameVote.GetReady4ResetTime() == true)
        {
            Cmd_VoteTime(20);
        }
        else
        {
            if (UIGameVote.getSecondsLeft() > 0)
            {
                if (CheckVote4Player(Role4Player.Human) && !UIGameVote.GetAllVote()) // Kiểm tra tất cả player đã vote hết chưa
                {
                    Cmd_AllVoteTime(); // thời gian chờ khi player đã vote hết
                }
                else if (!UIGameVote.GetAllVote()) // Khi chưa vote có thể sử dụng các hành động ở dưới
                {
                    UIGameVoted.SetVotedText(votes); // Gán số lần bị vote 
                    if (!IsSkipVote)
                    {
                        for (int num = 0; num < 10; num++)
                        {
                            if (Input.GetKeyDown(num.ToString()))
                            {
                                VotedTarget = Vote(num);
                            }
                            if (Input.GetKeyDown(KeyCode.W) && !AnimPlayer.GetBool(Param_4_Anim.VoteLeft) && !AnimPlayer.GetBool(Param_4_Anim.VoteYourSelf))
                            {
                                Cmd_SkipVote(gameObject.GetComponent<NetworkIdentity>(), true); // Thay đổi biến IsSkipVote đồng bộ lên server
                            }
                        }
                        
                    }
                    
                }
            }
            else
            {
                UIGameVote.SetReady4ResetTime(true); 
                Cmd_SetAllVote(false); // Thiết lập lại trạng thái chưa vote của tất cả player
                Cmd_SetSkipVote(false); // Thiết lập lại trạng thái chưa skip vote của tất cả player
                UIGameVoted.SetDefaultVotedText(); // Gán mặc định khi thời gian vote kết thúc
                Cmd_Be_A_Great_King();
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Vote 4 Action
    void Vote4Action(string _action)
    {
        if (UIGameVote.GetReady4ResetTime())
        {
            if (_action == Action4Player.Guilty)
            {
                Cmd_VoteTime(90);
            }
            else if (_action == Action4Player.GuardTurn)
            {
                Cmd_VoteTime(20);
            }
            else if (_action == Action4Player.SeerTurn)
            {
                Cmd_VoteTime(20); 
            }
            else if (_action == Action4Player.WolfTurn)
            {
                Cmd_VoteTime(45);
                if (Role == Role4Player.Wolf)
                {
                    UpdateApperance(Role4Player.Wolf);
                }
            }
            else if (_action == Action4Player.WitchTurn)
            {
                Cmd_VoteTime(20);
            }
            else if (_action == Action4Player.HunterTurn)
            {
                Cmd_VoteTime(30);
            }
            else
            {
                Cmd_VoteTime(6);
            }
        }
        else
        {
            if (UIGameVote.getSecondsLeft() > 0)
            {
                var _role = Role4Player.Human;
                if (_action == Action4Player.GuardTurn)
                {
                    _role = Role4Player.Guard;
                }
                else if (_action == Action4Player.SeerTurn)
                {
                    _role = Role4Player.Seer;
                }
                else if (_action == Action4Player.WolfTurn)
                {
                    _role = Role4Player.Wolf;
                }
                else if (_action == Action4Player.WitchTurn)
                {
                    _role = Role4Player.Witch;
                }
                else if (_action == Action4Player.HunterTurn)
                {
                    _role = Role4Player.Hunter;
                }
                else if (_action == Action4Player.Default)
                {
                    _role = Role4Player.Human;
                }

                if (_action != Action4Player.Default)
                {
                    if (CheckVote4Player(_role) && !UIGameVote.GetAllVote()) // Kiểm tra tất cả player đã vote hết chưa
                    {
                        Cmd_AllVoteTime(); // thời gian chờ khi player đã vote hết
                    }
                    else if (!UIGameVote.GetAllVote()) // Khi chưa vote có thể sử dụng các hành động ở dưới
                    {
                        UIGameVoted.SetVotedText(votes); // Gán số lần bị vote 
                        if (!IsSkipVote)
                        {
                            for (int num = 0; num < 10; num++)
                            {
                                if (Input.GetKeyDown(num.ToString()))
                                {
                                    VotedTarget = Vote(num);
                                }
                                if (Input.GetKeyDown(KeyCode.W) && !AnimPlayer.GetBool(Param_4_Anim.VoteLeft) && !AnimPlayer.GetBool(Param_4_Anim.VoteYourSelf))
                                {
                                    Cmd_SkipVote(gameObject.GetComponent<NetworkIdentity>(), true); // Thay đổi biến IsSkipVote đồng bộ lên server
                                }
                            }
                            
                        }
                    }
                }
            }
            else
            {
                if (CheckOutOfTime() && CheckDone4Players(true))
                {
                      UIGameVote.SetReady4ResetTime(true);
                }
                Cmd_SetDone4Player(true);
                Cmd_SetAllVote(false); // Thiết lập lại trạng thái chưa vote của tất cả player
                Cmd_SetSkipVote(false); // Thiết lập lại trạng thái chưa skip vote của tất cả player
                UIGameVoted.SetDefaultVotedText(); // Gán mặc định khi thời gian vote kết 
                if (_action == Action4Player.Guilty)
                {
                    Cmd_KillPlayer();
                }
                else if (_action == Action4Player.GuardTurn)
                {
                    Cmd_SaveByGuard(VotedTarget.GetComponent<NetworkIdentity>());
                }
                else if (_action == Action4Player.SeerTurn)
                {
                    ShowRole(VotedTarget);
                }
                else if (_action == Action4Player.WolfTurn)
                {
                    Cmd_KillPlayer();
                }
                else if (_action == Action4Player.WitchTurn)
                {
                    Cmd_KillPlayer();
                }
                else if (_action == Action4Player.HunterTurn)
                {
                    Cmd_KillPlayer();
                }
            }
        }
    }
    #endregion
    void SetupForNewAction(string _newAction)
    {
        UIGameTurn.SetTurnText(_newAction);
        CancelVote(VotedTarget);
        UIGameVote.SetReady4ResetTime(true);
        Cmd_SetDone4Player(false);
        Cmd_SetAction4Player(_newAction);
    }
    #endregion

    #region ChangeScene
    [Command]
    void Cmd_ChangeScene(string _action, string _scene)
    {
        if (CheckAction4Players(_action))
        {
            var currentScene = SceneManager.GetActiveScene();
            if (currentScene.name != _scene)
            {
                NetworkManager.singleton.ServerChangeScene(_scene);
            }

        }
    }
    #endregion

    #region SetTime4GamePlay
    [Command]
    public void Cmd_VoteTime(int seconds) // Thiết lập time vote từ client và đồng bộ lên server
    {
        UIGameVote = FindObjectOfType<UIGameVote>();
        UIGameVote.setSecondsLeft(seconds);
        Rpc_VoteTime(seconds);
    }

    [ClientRpc]
    void Rpc_VoteTime(int seconds)
    {
        UIGameVote = FindObjectOfType<UIGameVote>();
        UIGameVote.setSecondsLeft(seconds);
    }
    [Command]
    public void Cmd_DefaultVoteTime() // Thiết lập time vote từ client và đồng bộ lên server
    {
        UIGameVote = FindObjectOfType<UIGameVote>();
        UIGameVote.setSecondsLeft(3);
        Rpc_DefaultVoteTime(3);
    }

    [ClientRpc]
    void Rpc_DefaultVoteTime(int seconds)
    {
        UIGameVote = FindObjectOfType<UIGameVote>();
        UIGameVote.setSecondsLeft(seconds);
    }
    IEnumerator Wait4NSeconds(int _seconds)
    {
        yield return new WaitForSeconds(_seconds);
    }

    [Command]
    public void Cmd_AllVoteTime() // Thời gian chờ khi tất cả player đều vote
    {
        UIGameVote = FindObjectOfType<UIGameVote>();
        UIGameVote.SetAllVote(true);
        UIGameVote.setSecondsLeft(5);
        Rpc_AllVoteTime(5);
    }

    [ClientRpc]
    void Rpc_AllVoteTime(int seconds)
    {
        UIGameVote = FindObjectOfType<UIGameVote>();
        UIGameVote.SetAllVote(true);
        UIGameVote.setSecondsLeft(seconds);
    }

    #endregion

    #region SetReady
    /* Hàm Command thiết lập trạng thái sẵn sàng của 1 player 
        _target: netId của player
        _isReady: Trạng thái sẵn sàng của player
        _isStart: Trạng thái bắt đầu của player
      Cách hoạt động:
        - Tìm player theo netId
        - Khởi tạo UIGameReady
        - Gán biến IsReady trong UIGameReady = _isReady
        - Gán biến IsReady = _isReady
    */
    [Command]
    public void Cmd_Ready(NetworkIdentity _target, bool _isReady)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        if (players.Length > 0)
        {
            var _player = players.Where(t => t.GetComponent<NetworkIdentity>().netId == _target.netId).FirstOrDefault();
            _player.GetComponent<PlayerNetworkBehavior>().UIGameReady = FindObjectOfType<UIGameReady>();
            _player.GetComponent<PlayerNetworkBehavior>().UIGameReady.SetIsReady(_isReady);
            _player.GetComponent<PlayerNetworkBehavior>().IsReady = _isReady;
        }
    }

    /* Hàm Command kiểm tra trạng thái sẵn sàng của tất cả player 
     * và thiết lập trạng thái bắt đầu của tất cả player

      Cách hoạt động:
        - Tìm tất cả player có trạng thái sẵn sàng (IsReady = true)
        - So sánh với số lượng player đã vào
        - Nếu bằng gán IsStart của tất cả player = true
    */

    [Command]
    public void Cmd_Start()
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        if (players.Length > 0)
        {
            var _players_isReady = players.Where(t => t.GetComponent<PlayerNetworkBehavior>().IsReady == true).ToArray();
            Debug.Log(_players_isReady.Length);
            if (_players_isReady.Length == players.Length)
            {
                for (var i = 0; i < _players_isReady.Length; i++)
                {
                    _players_isReady[i].GetComponent<PlayerNetworkBehavior>().IsStart = true;
                    //Rpc_Start(_players_isReady[i].GetComponent<NetworkIdentity>());
                }
            }
        }
    }

    [ClientRpc]
    void Rpc_Start(NetworkIdentity _target)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        if (players.Length > 0)
        {
            var _player_isReady = players.Where(t => t.GetComponent<NetworkIdentity>().netId == _target.netId).FirstOrDefault();
            _player_isReady.GetComponent<PlayerNetworkBehavior>().IsStart = true;
        }
    }


    #endregion

    #region SetAllVote
    [Command]
    public void Cmd_SetAllVote(bool _vote) // Thay đổi biến AllVote 
    {
        UIGameVote = FindObjectOfType<UIGameVote>();
        UIGameVote.SetAllVote(_vote);
        Rpc_SetAllVote(_vote);
    }
    [ClientRpc]
    void Rpc_SetAllVote(bool _vote)
    {
        UIGameVote = FindObjectOfType<UIGameVote>();
        UIGameVote.SetAllVote(_vote);
    }
    #endregion

    #region SetSkipVote
    [Command]
    public void Cmd_SkipVote(NetworkIdentity _target, bool _isSkipVote) // Thay đổi biến IsSkipVote của 1 player theo netId
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        if (players.Length > 0)
        {
            var _player = players.Where(t => t.GetComponent<NetworkIdentity>().netId == _target.netId).FirstOrDefault();
            _player.GetComponent<PlayerNetworkBehavior>().IsSkipVote = _isSkipVote;
        }
    }
    [Command]
    public void Cmd_SetSkipVote(bool _skipVote) // Thay đổi biến IsSkipVote của tất cả player
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        if (players.Length > 0)
        {
            foreach(var player in players)
            {
                player.GetComponent<PlayerNetworkBehavior>().IsSkipVote = _skipVote;
            }
        }
        Rpc_SetSkipVote(_skipVote);
    }
    [ClientRpc]
    void Rpc_SetSkipVote(bool _skipVote)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        if (players.Length > 0)
        {
            foreach (var player in players)
            {
                player.GetComponent<PlayerNetworkBehavior>().IsSkipVote = _skipVote;
            }
        }
    }
    #endregion

    #region Set Role
    [Command]
    void SetRole4Player(string _role)
    {
        this.GetComponent<PlayerNetworkBehavior>().Role = string.Empty;
    }
    #endregion

    #region Get/SetAction4Player
    [Command]
    void Cmd_SetAction4Player(string _action)
    {
        this.GetComponent<PlayerNetworkBehavior>().Action = _action;
    }

    [Command]
    void Cmd_SetGuilty4Player(bool _isGuilty)
    {
        this.GetComponent<PlayerNetworkBehavior>().IsGuilty = _isGuilty;
    }

    [Command]
    void Cmd_SetDone4Player(bool _isDone)
    {
        this.GetComponent<PlayerNetworkBehavior>().IsDone = _isDone;
    }


    [Command]
    void Cmd_SetDay4Player(int _day)
    {
        this.Day = _day;
    }
    #endregion

    #region Be a Great King
    [Command]
    void Cmd_Be_A_Great_King()
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        players = players.OrderByDescending(t => t.GetComponent<PlayerNetworkBehavior>().votes).ToArray();
        if (players.Length  > 0)
        {
            if (players[0].GetComponent<PlayerNetworkBehavior>().votes > 0)
            {
                players[0].GetComponent<PlayerNetworkBehavior>().IsKing = true;
                Rpc_Be_A_Great_King(players[0].GetComponent<NetworkIdentity>());
            }
            else
            {
                var _index = Random.Range(0, players.Length);
                players[_index].GetComponent<PlayerNetworkBehavior>().IsKing = true;
                Rpc_Be_A_Great_King(players[_index].GetComponent<NetworkIdentity>());
            }
        }
    }
    [ClientRpc]
    void Rpc_Be_A_Great_King(NetworkIdentity _target)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        if (players.Length > 0)
        {
            var _player = players.Where(t => t.GetComponent<NetworkIdentity>().netId == _target.netId).FirstOrDefault();
            if (_player != null)
            {
                _target.GetComponent<PlayerNetworkBehavior>().IsKing = true;
                _target.GetComponent<PlayerNetworkBehavior>().playerNameText.color = Color.yellow;
            }
        }
    }
    #endregion

    #region Kill Bad Guy

    IEnumerator DoKilledPlayer()
    {
        if (this.GetComponent<PlayerNetworkBehavior>().IsKilled)
        {
            this.AnimPlayer.SetBool(Param_4_Anim.IsDead, true);
        }
        var _anim = this.AnimPlayer.runtimeAnimatorController.animationClips.Where(t => t.name == Param_4_Anim.IsDead).FirstOrDefault();
        if (_anim != null)
        {
            Debug.Log(_anim.length);
            yield return new WaitForSeconds(2 + _anim.length);
            if (this.GetComponent<PlayerNetworkBehavior>().IsKilled)
            {
                var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player).ToArray();
                var player = players.Where(t => t.GetComponent<NetworkIdentity>().netId == this.GetComponent<NetworkIdentity>().netId).FirstOrDefault();
                if (player != null)
                {
                    SetupForNewAction(Action4Player.Default);
                    player.SetActive(false);
                    SetRole4Player(string.Empty);
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(2 + 4);
            if (this.GetComponent<PlayerNetworkBehavior>().IsKilled)
            {
                var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player).ToArray();
                var player = players.Where(t => t.GetComponent<NetworkIdentity>().netId == this.GetComponent<NetworkIdentity>().netId).FirstOrDefault();
                if (player != null)
                {
                    SetupForNewAction(Action4Player.Default);
                    player.SetActive(false);
                    SetRole4Player(string.Empty);
                }
            }
        }
    }

    [Command]
    void Cmd_KillPlayer()
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        players = players.OrderByDescending(t => t.GetComponent<PlayerNetworkBehavior>().votes).ToArray();
        if (players.Length > 1)
        {
            if (players[0].GetComponent<PlayerNetworkBehavior>().votes > 0)
            {
                players[0].GetComponent<PlayerNetworkBehavior>().IsKilled = true;
            }
        }
    }
    string ShowRole(GameObject _target)
    {
        return _target.GetComponent<PlayerNetworkBehavior>().Role;
    }
    [Command]
    void Cmd_SaveByGuard(NetworkIdentity _target)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        var player = players.Where(t => t.GetComponent<NetworkIdentity>().netId == _target.netId).FirstOrDefault();
        if (player != null)
        {
            player.GetComponent<PlayerNetworkBehavior>().IsSavedByGuard = true;
        }
    }
    [Command]
    void Cmd_SaveByWitch(NetworkIdentity _target)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        var player = players.Where(t => t.GetComponent<NetworkIdentity>().netId == _target.netId).FirstOrDefault();
        if (player != null)
        {
            player.GetComponent<PlayerNetworkBehavior>().IsSavedByWitch = true;
        }
    }
    [Command]
    void Cmd_GiveTheThrone(NetworkIdentity _target)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        var player = players.Where(t => t.GetComponent<NetworkIdentity>().netId == _target.netId).FirstOrDefault();
        if (player != null)
        {
            player.GetComponent<PlayerNetworkBehavior>().IsKing = true;
        }
    }
    #endregion

    #region Update Player Prefab
    void UpdateApperance(string _role)
    {
        Cmd_UpdateApperance(_role);
    }

    [Command]
    void Cmd_UpdateApperance(string _role)
    {
        var oldPrefab = GetComponent<NetworkIdentity>().connectionToClient;

        var _rolePrefab = FindObjectOfType<NetworkManager>().spawnPrefabs.Where(t => t.name == _role).FirstOrDefault();
        GameObject newPrefab = (GameObject)Instantiate(_rolePrefab, this.gameObject.transform.position, this.gameObject.transform.rotation);

        newPrefab.GetComponent<PlayerNetworkBehavior>().index = this.index;

        newPrefab.GetComponent<PlayerNetworkBehavior>().Role = this.Role;
        newPrefab.GetComponent<PlayerNetworkBehavior>().IsKing = this.IsKing;
        newPrefab.GetComponent<PlayerNetworkBehavior>().IsReady = this.IsReady;
        newPrefab.GetComponent<PlayerNetworkBehavior>().IsStart = this.IsStart;
        newPrefab.GetComponent<PlayerNetworkBehavior>().IsSkipVote = this.IsSkipVote;

        newPrefab.GetComponent<PlayerNetworkBehavior>().Day = this.Day;
        newPrefab.GetComponent<PlayerNetworkBehavior>().Action = this.Action;
        newPrefab.GetComponent<PlayerNetworkBehavior>().IsDone = this.IsDone;
        newPrefab.GetComponent<PlayerNetworkBehavior>().IsSavedByGuard = this.IsSavedByGuard;
        newPrefab.GetComponent<PlayerNetworkBehavior>().IsSavedByWitch = this.IsSavedByWitch;
        newPrefab.GetComponent<PlayerNetworkBehavior>().IsKilled = this.IsKilled;
        newPrefab.GetComponent<PlayerNetworkBehavior>().IsKilledByWitch = this.IsKilledByWitch;
        newPrefab.GetComponent<PlayerNetworkBehavior>().IsGuilty = this.IsGuilty;

        Destroy(oldPrefab.identity.gameObject);
        NetworkServer.ReplacePlayerForConnection(this.connectionToClient, newPrefab,true);
        Rpc_UpdateApperance();
    }

    [ClientRpc]
    void Rpc_UpdateApperance()
    {
        Debug.Log("Replace");
    }
    #endregion
}
