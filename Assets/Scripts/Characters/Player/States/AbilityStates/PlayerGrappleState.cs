using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrappleState : PlayerState
{

    public PlayerGrappleState(Player player, PlayerStateMachine stateMachine, PlayerData playerData, string stateName) : base(player, stateMachine, playerData, stateName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.InputHandler.PlayerCancelGrapple += OnCancelGrapple;

        //player.animationHandler.GrappleAir();
    }

    public override void Exit()
    {
        base.Exit();

        player.InputHandler.PlayerCancelGrapple -= OnCancelGrapple;
        player.Grapple.StartCoroutine(player.Grapple.LerpHookBack(0.05f));

        player.transform.SetParent(null);
    }

    public override void OnCancelJump()
    {
        // intentionally empty to overwrite the PlayerState's OnCancelJump so that the player's y velocity isn't halved during a grapple
    }

    public override void OnGrapple()
    {
        // intentionally empty so you can't grapple from the grapple state (although you'd have to release the button to press it down again which would put you out of grapple state anyways)
    }

    public void OnCancelGrapple()
    {
        player.AnimationHandler.GrappleCancel();
        stateMachine.ToState(player.A_MoveState);
    }

    public override void Update()
    {
        base.Update();
        
        float relY = player.transform.position.y - player.Grapple.Hook.transform.position.y;
        float relX = player.transform.position.x - player.Grapple.Hook.transform.position.x;

        // normal vector tangent to the circle created with radius r = distance between the player and the hook.
        Vector2 nTan = Vector3.Normalize(Vector2.Perpendicular(player.Grapple.Hook.transform.position - player.transform.position));
        // put the player speed in the direction of the tangent normal and add the vertical velocity projected onto the past velocity vector
        player.rb.velocity = Vector3.Project(player.CurrentVelocity, nTan);

        // if the player swings 10 degrees higher than the grapple point, break the chain (uses arctan and trig properties, pi/18 is 10 deg in radians)
        // only check this condition if the player is higher than the grapple point on the y axis
        if (relY > 0 && Mathf.Abs(Mathf.Atan(relY / relX)) >= Mathf.PI / 18)
        {
            player.AnimationHandler.GrappleCancel();
            stateMachine.ToState(player.A_MoveState);
        }
        // else if moving towards the grapple point or haulted by a obstacle such as a wall, destroy grapple
        else if (Mathf.Abs(player.CurrentVelocity.x) <= 0.05f)
        {
            player.AnimationHandler.GrappleCancel();
            stateMachine.ToState(player.A_MoveState);
        }
        else if (player.IsGrounded())
        {
            stateMachine.ToState(player.G_MoveState);
        }
    }

    public void DestroyGrapple()
    {
        player.Grapple.enabled = false;
    }

    /*
     *      DEPRECATED: This was the old grappling chain VFX (if you can call it that)
     *      It drew 8 dots between the player and hook as a fill in chain during development.
     * 
    public void DrawChain()
    {
        // clear the current drawn chain
        foreach (GameObject link in chain)
        {
            Object.Destroy(link);
        }
        chain.Clear();

        // Idea: Draw a line between player and grapple hook point, linerally interpolate on the line and place a chain every x lengths.
        Vector3 start = player.Hook.transform.position;
        Vector3 traversalLine = player.transform.position - start;
        for (int i = 1; i < 8; i++)
        {
            start += traversalLine / 8; // here x is the magnitude of the distance between the player and the hook times 2. it means as the player gets futher away, the chain length will seem more tense.
            chain.Add(Object.Instantiate(player.chainLinkPrefab, start, Quaternion.identity));
        }
    }
    */
}
