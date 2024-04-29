using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class PlayerNormalJumpState : PlayerJumpState
{

    public PlayerNormalJumpState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string stateName) : base(player, stateMachine, playerData, stateName)
    {
    }

    public override void DoChecks()
    {
        base.DoChecks();
    }

    public override void Enter()
    {
        base.Enter();

        // decrement the jump counter
        playerData.jumps--;

        // find the ground we're jumping from
        player.Ground(out Collider2D ground);

        // we enter jump state after the jump input has been found, so we just perform the jump here:

        // first, we start the FX
        player.AddComponent<Jump>();

        Vector2 BunnyHop = Vector2.zero;
        Vector2 MovingPlatformMomentum = Vector2.zero;
        Vector2 NormalJump = new Vector2(player.rb.velocity.x, playerData.jumpingPower);

        // if bHopFrames > 0f and the player jumps, they perform a Bunny Hop (maintain momentum on the jump)
        if (playerData.bHopFrames > 0f)
        {
            // Bunny Hopping feels good when you get slighly more speed and the jumping height is a little bit related to the speed (the higher the speed in [4f, 5f] the less y velo on the jump)
            BunnyHop = new Vector2(playerData.bHopVelocity.x * 1.15f, playerData.jumpingPower * Mathf.Clamp(4 / Mathf.Abs(playerData.bHopVelocity.x), 0.8f, 1.0f)) - NormalJump;
        }
        if (ground != null && ground.TryGetComponent<IMoveableObject>(out IMoveableObject moveableObject))
        {
            MovingPlatformMomentum.Set(moveableObject.GetVelocity().x, 0);
        }

        player.rb.velocity = NormalJump + BunnyHop + MovingPlatformMomentum;
        Debug.Log(player.rb.velocity);

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

        // if the player's speed is lower than bhopping speed or they move opposite the direction of the bhop
        if (Mathf.Abs(player.CurrentVelocity.x) < 2.6f || moveInput.x * player.transform.right.x <= 0)
        {
            player.SetVelocityX(playerData.movementVelocity * moveInput.x);
        }

        // if player y velocity is less than 3 change to Air Move State
        // This is done so that we don't immediately give control to the air move state which would, if we were in a corner, put us right into the walled state and neuter the jump
        // I could have some sort of check if x velo is greater than 0 or not to move to idle state but I'd prefer
        // cleaner looking code and has to do an extra state transition
        if (player.rb.velocity.y <= 3f)
        {
            stateMachine.ToState(player.A_MoveState);
        }

    }

    public override void OnJump()
    {
        if (playerData.jumps > 0)
        {
            // if we jump in the jump state and we're walled, we're doing a wall jump so go to the walljump state
            if (player.IsWalled())
            {
                stateMachine.ToState(player.WallJumpState);
            }
            // if we're not walled, we're doing a double jump without any other inputs... so we just call the enter again
            // we could call Enter() but it feels more respectful of the architecture to "change the state"
            // For reference: the design currently has the player only able to do 1 jump, so this is unreachable.
            else
            {
                stateMachine.ToState(player.NormalJumpState);
            }
        }
    }
}
