using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public partial class PlayerNetworkBehavior : NetworkBehaviour
{

    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority) { return; }  // kiểm tra quyền client
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        foreach (var player in players)
        {
            player.GetComponent<PlayerNetworkBehavior>().NameTag.transform.LookAt(this.CameraPlayer.transform);
        }
        if (isLocalPlayer)
        {
            this.NameTag.SetActive(false);
            // Gán thuộc tính isReady trong UIGameReady vào biến IsReady, IsStart mặc định là false
            IsReady = UIGameReady.GetIsReady();

            // Thiết lập trạng thái sẵn sàng của player và đồng bộ lên server
            if (IsReady && !IsStart)
            {
                Cmd_Ready(gameObject.GetComponent<NetworkIdentity>(), IsReady);
            }           
            if(IsReady && IsStart)
            {
                UIGameReady.ShowReadyPanel(false); // Ẩn panel khi IsReady và IsStart bằng true
                float moveHor = Input.GetAxis("Horizontal");
                float moveVer = Input.GetAxis("Vertical");
                var movement = new Vector3(moveHor, 0, moveVer);
                transform.position += movement;
                if (!CheckKing())
                {
                    Vote4AKing();
                }
                else
                {
                    Vote4Guilty();
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
    private bool CheckKing()
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        players = players.Where(t => t.GetComponent<PlayerNetworkBehavior>().IsKing == true).ToArray();
        if (players.Length > 0)
        {
            return true;
        }
        return false;
    }

    #region GamePlay
    #region Vote 4 A King
    void Vote4AKing()
    {
        if (UIGameVote.GetReady4ResetTime() == true)
        {
            Cmd_VoteTime(20);
        }
        if (UIGameVote.getSecondsLeft() > 0)
        {
            UIGameVoted.SetVotedText(votes); // Gán số lần bị vote 
            if (Input.GetMouseButtonDown(0))
            {
                VotedTarget = Vote();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                CancelVote(VotedTarget);
            }
        }
        else
        {
            UIGameVote.SetReady4ResetTime(true);
            UIGameVoted.SetDefaultVotedText(); // Gán mặc định khi thời gian vote kết thúc
            Cmd_Be_A_Great_King();
        }
    }
    #endregion

    #region Vote 4 Guilty
    void Vote4Guilty()
    {
        if (UIGameVote.GetReady4ResetTime() == true)
        {
            Cmd_VoteTime(60);
        }
        if (UIGameVote.getSecondsLeft() > 0)
        {
            UIGameVoted.SetVotedText(votes); // Gán số lần bị vote 
            if (Input.GetMouseButtonDown(0))
            {
                VotedTarget = Vote();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                CancelVote(VotedTarget);
            }
        }
        else
        {
            UIGameVote.SetReady4ResetTime(true);
            UIGameVoted.SetDefaultVotedText(); // Gán mặc định khi thời gian vote kết thúc
            Cmd_Kill_BadGuy();
        }
    }
    #endregion
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

    #region Kill Bad Guy
    IEnumerator DoAnimDead(GameObject _target)
    {
        //Print the time of when the function is first called.
        _target.GetComponent<PlayerNetworkBehavior>().AnimPlayer.SetBool(Param_4_Anim.IsDead, true);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(2);

        //After we have waited 5 seconds print the time again.
        Destroy(_target);
    }
    [Command]
    public void Cmd_Kill_BadGuy()
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        players = players.OrderByDescending(t => t.GetComponent<PlayerNetworkBehavior>().votes).ToArray();
        if (players.Length > 1)
        {
            if (players[0].GetComponent<PlayerNetworkBehavior>().votes > 0)
            {
                StartCoroutine(DoAnimDead(players[0]));
                Rpc_Kill_Player(players[0].GetComponent<NetworkIdentity>());
            }
        }
    }
    [ClientRpc]
    void Rpc_Kill_Player(NetworkIdentity _target)
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        if (players.Length > 0)
        {
            var _player = players.Where(t => t.GetComponent<NetworkIdentity>().netId == _target.netId).FirstOrDefault();
            if (_player != null)
            {
                StartCoroutine(DoAnimDead(_player));
            }
        }
    }
    #endregion

    #region Be a Great King
    [Command]
    void Cmd_Be_A_Great_King()
    {
        var players = GameObject.FindGameObjectsWithTag(Tags_4_Object.Player);
        players = players.OrderByDescending(t => t.GetComponent<PlayerNetworkBehavior>().votes).ToArray();
        if (players.Length > 1)
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



}
