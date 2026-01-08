using PurrNet;
using PurrNet.StateMachine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RoundRunningStateDM : StateNode<List<PlayerHealth>>
{
    [SerializeField] private float matchDuration;
    [SerializeField] private float timer;
    private List<PlayerID> _players = new();
    bool gameEnded = false;

    public override void Enter(List<PlayerHealth> data, bool asServer)
    {
        base.Enter(data, asServer);
        if (!asServer) return;
        _players.Clear();
        timer = matchDuration;

        foreach (var player in data)
        { 
            if(player.owner.HasValue) _players.Add(player.owner.Value);

            player.OnDeath_Server += OnPlayerDeath;
        }
        
    }

    public override void StateUpdate(bool asServer)
    {
        base.StateUpdate(asServer);
        if(!asServer) return;
        
        if (timer <= 0) 
        {
            if(!gameEnded)
            {
                gameEnded = true;
                Debug.Log("asdasdasdasd FINISHED aihsdbiahbdihad");
            }
        }

        else if (timer > 0)
        {
            Timer();
        }
    }

    private void Timer()
    {
        timer -= Time.deltaTime;
    }

    private void OnPlayerDeath(PlayerID _deadPlayer)
    { 

    }
}
