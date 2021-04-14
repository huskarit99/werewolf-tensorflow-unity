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
        Cmd_VoteTime(60);
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
