using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundCoyoteState : PlayerCoyoteState
{
    public PlayerGroundCoyoteState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string stateName) : base(player, stateMachine, playerData, stateName)
    {
    }

    public override void Exit()
    {
        base.Exit();

        playerData.jumps--;
    }

    public override void OnJump()
    {
        stateMachine.ToState(player.NormalJumpState);
    }

    public override void Update()
    {
        base.Update();

        player.SetVelocityX(playerData.movementVelocity * moveInput.x);
    }
}
