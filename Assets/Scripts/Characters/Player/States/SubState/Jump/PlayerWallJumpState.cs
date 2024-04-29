using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallJumpState : PlayerJumpState
{

    public PlayerWallJumpState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string stateName) : base(player, stateMachine, playerData, stateName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter()
    {
        base.Enter();

        // when we wall jump, we force the player away from the wall they jumped from in a slightly more than regular jump manner and
        // adjust the jumping power accordingly
        //Debug.Log("wall jumped with transform.right = " + wallDirection);

        player.AnimationHandler.JumpUp();
        
        // if the player has bhopframes and their velocity before hitting the wall is greater than 4f, we let them bhop.
        if (playerData.bHopFrames > 0 && playerData.bHopVelocity.y > 4f)
        {
            // this works but it's just a niche use case right now.
            player.SetVelocity(-playerData.wallDirection.x * playerData.movementVelocity * playerData.xScalar * 0.5f, playerData.bHopVelocity.y * playerData.yScalar);
        }
        else
        {
            player.SetVelocity(-playerData.wallDirection.x * playerData.movementVelocity * playerData.xScalar, playerData.jumpingPower * playerData.yScalar);
        }

        //player.GizmoSpherePoints.Add(player.transform.position + new Vector3(0.3f, 0, 0));
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

        /*
         *      DATA:
         *      
         *      A Wall Jump with 0.03f dampening factor, 1.5f x velo scalar, 1.1f jumping scalar takes 0.42 seconds and uses 260 frames.
         *      Thus, 0.21 seconds is exactly half the jumping time (since there's a constant acceleration downwards we can do this)
         */

        // when we're walljumping towards the wall we dampen the player's movement input's heavily so they feel the weight of the wall
        if (playerData.wallDirection.x * moveInput.x >= 0)
        {
            // 0.004
            player.AddVelocityX(playerData.movementVelocity * (playerData.dampeningScalar * (5 - player.CurrentVelocity.y) / (5 - 0)) * moveInput.x);
        }

        // on a wall jump in particular, we want to keep the player in the wall jump state so we can control their ability to change their momentum
        // whereas a normal jump returns to the general movement states much sooner because the player just moves with normal conditions.
        if (player.CurrentVelocity.y <= 0f)
        {
            if (playerData.wallDirection.x * moveInput.x >= 0)
            {
                player.AddVelocityY(-1f);
            }
            stateMachine.ToState(player.A_MoveState);
        }
    }

    public override void OnWallAttach()
    {
        // if your speed is a little less than a jump and you're walled and holding the wall attach button, then attach back to the wall.
        // this prevents the walljump state instantly reverting to the wall attach state if the player is holding the wall attach button
        if (player.CurrentVelocity.y < 4f)
        {
            base.OnWallAttach();
        }
    }
}
