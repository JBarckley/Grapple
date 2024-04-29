using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 *      This is a solution I don't like writing, but I think it's the best tool I can concieve for the current job.
 *      The problem with my implementation of coyote frames without this state is that a player who is in air and touches a
 *      wall will NOT recieve coyote frames when moving away from said wall. Having to create a new state to rectify this
 *      feels inappropriate and indicative of a shortcoming of my architecture, but nevertheless I can't figure out
 *      where that shortcoming is so this is a solution that will work.
 *      
 *      Here's what happens:
 *      
 *      The player is in an air state or a jump state and they touch a wall. This means they're not currently grappling or in any state that would
 *      create problems with a sudden state change. They come here as an intermediary state. When they are no longer walled,
 *      we transition to another intermediary state of wallcoyote which handles coyote frames.
 *      
 *      Two intermediate states: bad architecture or good problem solving? I'm not sure.
 * 
 */


public class PlayerOnWallState : PlayerState
{
    public PlayerOnWallState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string stateName) : base(player, stateMachine, playerData, stateName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        player.SetVelocityX(playerData.movementVelocity * moveInput.x);

        // if we fall to the ground at a corner while being on the wall, just put us in a idle ground state
        if (player.IsGrounded())
        {
            stateMachine.ToState(player.G_IdleState);
        }
        // otherwise when we leave the wall give us coyote frames
        else if (!player.IsWalled())
        {
            stateMachine.ToState(player.WallCoyoteState);
        }
    }
}
