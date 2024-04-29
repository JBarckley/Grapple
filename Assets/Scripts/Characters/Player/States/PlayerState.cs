using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerState
{
    protected Player player;
    protected PlayerStateMachine stateMachine;
    protected PlayerData playerData;

    protected float startTime;
    protected Vector2 moveInput;
    protected Vector2 rawMoveInput;

    protected GameObject playerHook;

    public string stateName;

    // Constructor
    public PlayerState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string stateName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.playerData = playerData;
        this.stateName = stateName;
    }

    public virtual void Enter()
    {
        DoChecks();
        startTime = Time.time;

        // assign the delegate listener playerJump to this state's jump functions
        player.InputHandler.PlayerJump += OnJump;
        player.InputHandler.PlayerCancelJump += OnCancelJump;
        player.InputHandler.PlayerWallAttach += OnWallAttach;
        player.InputHandler.PlayerGrapple += OnGrapple;
        //Debug.Log(stateName);
    }

    public virtual void Exit()
    {
        // remove this state's jump functions from the delegate listener
        player.InputHandler.PlayerJump -= OnJump;
        player.InputHandler.PlayerCancelJump -= OnCancelJump;
        player.InputHandler.PlayerWallAttach -= OnWallAttach;
        player.InputHandler.PlayerGrapple -= OnGrapple;
    }

    public virtual void Update()
    {
        moveInput = player.InputHandler.MoveVector;
        rawMoveInput = player.InputHandler.RawMoveVector;

        // if the player has bHopFrames, reduce them each fixedupdate
        if (playerData.bHopFrames > 0f) 
        { 
            playerData.bHopFrames--; 
        }

        if (player.IsDead())
        {
            player.transform.position = player.StartingPoint;
        }

        player.CheckForFlip();
    }

    public virtual void PhysicsUpdate()
    {
        //DoChecks();
    }

    public virtual void DoChecks()
    {

    }

    #region Listener functions

    public virtual void OnJump()
    {    
        // if we're on a wall midair, but not climbing, still allow the player to perform a wall jump (prevent a corner jump from being a walljump by ensuring we're not grounded)
        if (!player.IsGrounded() && player.IsWalled())
        {
            stateMachine.ToState(player.WallJumpState);
            player.AnimationHandler.JumpUp();
        }
        else if (playerData.jumps > 0)
        {
            stateMachine.ToState(player.NormalJumpState);
            player.AnimationHandler.JumpUp();
        }

        // if we're not grounded or on wall, we start a jump buffer
        // Our other states will be able to see if a coroutine exists in order to see if the buffer is full or empty
        else
        {
            // start the jump buffer
            player.StartJumpBuffer(0.05f);
        }
    }

    public virtual void OnCancelJump()
    {
        // if you cancel the jump midway through, half your vertical speed so how long you hold the button factors into movement
        // if you cancel the jump while descending or after landing, do nothing
        // if the player is grappling or bhopping and achieves a large max speed then jumps midair by accident, we don't want to half
        // their speed, so we set a cap to the speed that will be reduced to the regular jumping power of a player (5f)
        if (player.CurrentVelocity.y >= 0 && player.CurrentVelocity.y <= playerData.jumpingPower)
        {
            player.SetVelocityY(player.rb.velocity.y * 0.5f);
        }
    }

    public virtual void OnWallAttach()
    {
        if (player.IsWalled())
        {
            stateMachine.ToState(player.WallClimbState);
        }
    }

    public virtual void OnGrapple()
    {

        /*
        Vector2 aimVec = Vector2.zero;

        // if the player is not currently moving in any horizontal direction
        if (moveInput.x == 0f)
        {
            aimVec = new Vector2(player.FacingDirection, 1);
        }
        // otherwise take the player's movement direction
        else
        {
            aimVec = new Vector2(player.InputHandler.RawMoveVector.x, 1);
        }
        aimVec.Normalize();
        aimVec *= 4;

        // see if there is any grappleable spots
        RaycastHit2D findGrappleable = Physics2D.Raycast(player.transform.position, aimVec, playerData.grapplingHookLength, player.wallLayer);

        Vector2 hitPoint = findGrappleable.point;
        Vector2 maxThrow = player.Hook.transform.position + (Vector3)aimVec;
        if (hitPoint != Vector2.zero)
        {
            player.StartCoroutine(player.LerpHook(hitPoint, 0.05f, true));
        }
        else
        {
            player.StartCoroutine(player.LerpHook(maxThrow, 0.1f, false));
        }
        */

        player.Grapple.enabled = true;
    }

    #endregion
}
