using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerState
{

    private Collider2D ground;

    public PlayerGroundedState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string stateName) : base(player, stateMachine, playerData, stateName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter()
    {
        base.Enter();

        // restore double jumping capabilities when touching the ground
        playerData.jumps = 1;
        // reset coyote frames when player is grounded
        playerData.coyoteFrames = 45f;


        player.Ground(out Collider2D ground);
        Debug.Log(ground);
        if (ground != null && ground.TryGetComponent<IMoveableObject>(out IMoveableObject moveableObject))
        {
            player.transform.SetParent(ground.transform, true);
        }

        // if the jump buffer has an input, we call the onjump function as if the player had just pressed the jump button
        if (player.JumpBuffer)
        {
            OnJump();
        }
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

        player.SetVelocityX(playerData.movementVelocity * moveInput.x);

        if (!player.IsGrounded())
        {
            // here, if we fall off a platform without jumping, we still decrement the double jump counter
            stateMachine.ToState(player.GroundCoyoteState);
        } 

        /*
        else if (ground == null)
        {
            player.Ground(out ground);
            if (ground != null && ground.TryGetComponent<IMoveableObject>(out IMoveableObject moveableObject))
            {
                //player.transform.position -= new Vector3(0.014f, 0);
                player.transform.SetParent(ground.transform, true);
            }
        }*/

    }
}
