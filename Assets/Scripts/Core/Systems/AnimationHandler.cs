using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    private Animator animator;
    private Player player;

    private void Awake()
    {
        // gets the player's animator & player
        player = GetComponent<Player>();
        animator = GetComponent<Animator>();
    }

    public void Idle()
    {
        animator.Play("Idle");
    }

    public void Move()
    {
        animator.Play("Move");
    }

    public void JumpUp()
    {
        animator.Play("JumpUp");
    }

    public void JumpDown()
    {
        animator.Play("Fall");
    }

    public void JumpLand()
    {
        animator.Play("Land");
    }

    public IEnumerator AirAnimation()
    {
        yield return new WaitUntil(() => player.CurrentVelocity.y < 0f);
        JumpDown();
        /*
        yield return new WaitUntil(() => player.IsGrounded() || player.CurrentVelocity.y >= 0f);
        if (player.CurrentVelocity.y < 0f)
        {
            JumpLand();
        }*/
    }

    public void GrappleAir()
    {
        animator.Play("GrappleAir");
    }

    public void GrappleCancel()
    {
        animator.Play("GrappleCancel");
    }

    public void WallGrab()
    {
        animator.Play("WallGrab");
    }

    public void WallClimb()
    {
        animator.Play("WallClimb");
    }
}
