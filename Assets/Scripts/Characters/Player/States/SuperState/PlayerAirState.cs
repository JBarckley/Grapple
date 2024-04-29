using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAirState : PlayerState
{

    public PlayerAirState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string stateName) : base(player, stateMachine, playerData, stateName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter()
    {
        base.Enter();

        // when the player enters the air state, start the animation. Jumping or not, the animation is the same here.
        player.AnimationHandler.StopAllCoroutines();
        player.AnimationHandler.StartCoroutine(player.AnimationHandler.AirAnimation());
    }

    public override void Exit()
    {
        base.Exit();

        // if our velocity is larger than the normal amount, we have the ability to bunny hop.
        // any bunny hopping opportunity comes from speed greater than the normal movement velocity, so we know the player is exiting the air state
        if (Mathf.Abs(player.CurrentVelocity.x) > 2.5f || player.CurrentVelocity.y > 4f)
        {
            playerData.bHopVelocity = player.CurrentVelocity;
            playerData.bHopFrames = 25f;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void Update()
    {
        base.Update();

        // If the player's current velocity is normal move speed OR if they're moving against the direction of their current velocity and their current velocity is larger than regular speed
        if (Mathf.Abs(player.CurrentVelocity.x) <= 2.6f || moveInput.x * player.transform.right.x <= 0)
        {
            player.SetVelocityX(playerData.movementVelocity * moveInput.x);
        }


        // refer to PlayerOnWallState for solution details, but here we have an intermediate state to help with coyote frames on wall jumps
        if (player.IsWalled())
        {
            stateMachine.ToState(player.OnWallState);
        }
    }
}
