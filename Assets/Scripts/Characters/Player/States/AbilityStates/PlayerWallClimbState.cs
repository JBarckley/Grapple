using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWallClimbState : PlayerState
{

    // in order to properly do the wall climbing animation with respect to the state architecture, I should have created another state called "wallclimb" where the movement
    // logic is and let the animations pass between the animation states within each of the controller states.
    // Anyways, at this point I'm not going to make another state, especially when a boolean can do the trick quick and dirty.
    public bool climbAnim = false;

    public PlayerWallClimbState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string stateName) : base(player, stateMachine, playerData, stateName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter()
    {
        base.Enter();

        // while we're climbing the wall, disable physics interactions
        player.rb.gravityScale = 0f;

        if (player.IsGrounded())
        {
            // push the player juuuuust slightly higher than the object that checks for the ground so we dont loop
            player.transform.position += Vector3.up * 0.055f;
        }

        player.AnimationHandler.WallGrab();

        /* sometimes when you enter the wallclimb state, you're a variable x distance smaller than the wallcheck's width away from the wall.
        / to fix this, send a ray out, check how far you are, and get rid of the difference
        RaycastHit2D wall = Physics2D.Raycast(player.transform.position, Vector2.right * playerData.wallDirection);
        player.transform.position = new Vector3(player.transform.position.x - (playerData.wallDirection.x * wall.distance), player.transform.position.y, 0);
        */

        // if the player falls off the ground from a high ledge onto a wall (and uses their coyote frames) then they can still jump again from the wall so we reset coyote frames
        playerData.coyoteFrames = 45f;

        player.InputHandler.PlayerWallDetach += OnWallDetach;
    }

    public override void Exit()
    {
        base.Exit();

        // reenable physics (gravity) once we've stopped climbing the wall
        player.rb.gravityScale = 1.3f;

        player.InputHandler.PlayerWallDetach -= OnWallDetach;
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void Update()
    {
        base.Update();

        // while walled we can climb up and shimmy down slowly. Still not sure about this as a design decision.

        /*                                                                                                                                                   ________
         *   TODO: Lerp the player's position from a wall to the ground of a higher platform when you approach the corner moving upwards.           Here -> |
         *                                                                                                                                                  |
         *                                                                                                                                                  |
         */

        player.SetVelocity(0, moveInput.y * 1.5f);
        if (moveInput.y != 0)
        {
            player.AnimationHandler.WallClimb();
            climbAnim = true;
        }
        else if (climbAnim)
        {
            player.AnimationHandler.WallGrab();
            climbAnim = false;
        }

        // otherwise if the player fell off the wall into midair, we give the player coyote frames at the wall coyote state
        if (!player.IsWalled())
        {
            stateMachine.ToState(player.WallCoyoteState);
        }
    }

    // override the default implementation of OnJump so that jumping from a wall results in a wall jump
    public override void OnJump()
    {
        stateMachine.ToState(player.WallJumpState);
    }

    public override void OnWallAttach()
    {
        // this needs to be empty to override the PlayerState behavior into an empty body (don't enter into the state repeatedly while holding the wall attach button)
    }

    public void OnWallDetach()
    {
        stateMachine.ToState(player.A_MoveState);
    }

    public override void OnGrapple()
    {
        // intentionally empty
    }
}
