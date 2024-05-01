using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallCoyoteState : PlayerCoyoteState
{
    public PlayerWallCoyoteState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string stateName) : base(player, stateMachine, playerData, stateName)
    {
    }

    public override void OnJump()
    {
        stateMachine.ToState(player.WallJumpState);
    }

    public override void Update()
    {
        base.Update();

        player.SetVelocityX(playerData.movementVelocity * moveInput.x);
    }
}
