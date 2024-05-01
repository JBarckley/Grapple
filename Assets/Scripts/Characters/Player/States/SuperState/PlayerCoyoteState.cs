using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *      Simple Class: We create a intermediary state between falling off a platform and being in the airstate so that player's who slightly jump late can still jump without
 *                    being charged two jumps
 */


public class PlayerCoyoteState : PlayerState
{
    public PlayerCoyoteState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string stateName) : base(player, stateMachine, playerData, stateName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        playerData.coyoteFrames = 45f;

    }

    public override void Update()
    {
        base.Update();

        playerData.coyoteFrames--;

        if (playerData.coyoteFrames <= 0)
        {
            // if the player was in a moving platform state and is now falling:
            player.transform.SetParent(null);
            stateMachine.ToState(player.A_MoveState);
        }
    }
}
