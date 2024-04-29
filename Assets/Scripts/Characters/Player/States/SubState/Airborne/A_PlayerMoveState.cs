using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_PlayerMoveState : PlayerAirState
{
    public A_PlayerMoveState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string stateName) : base(player, stateMachine, playerData, stateName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter()
    {
        base.Enter();
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

        if (player.IsGrounded())
        {
            stateMachine.ToState(player.G_MoveState);
        }
        else if (moveInput.x == 0f)
        {
            stateMachine.ToState(player.A_IdleState);
        }
    }
}
