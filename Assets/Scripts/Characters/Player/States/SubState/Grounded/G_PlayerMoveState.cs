using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class G_PlayerMoveState : PlayerGroundedState
{
    public G_PlayerMoveState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string stateName) : base(player, stateMachine, playerData, stateName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter()
    {
        base.Enter();

        player.AnimationHandler.Move();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void Update()
    {
        base.Update();

        if (moveInput.x == 0f)
        {
            stateMachine.ToState(player.G_IdleState);
        }
    }
}
