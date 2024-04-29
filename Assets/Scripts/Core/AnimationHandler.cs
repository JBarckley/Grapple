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

    /*
    // if the player is spamming left and right changing directions quickly, they'll enter the idle state in between moving left and right which will introduce
    // a pause between the continuation of the walk animation. This code ensures that behavior will not transition to an idle animation immediately and continue the move in the other direction.
    public IEnumerator Idle()
    {
        yield return new WaitForSeconds(0.05f);
        if (player.InputHandler.MoveVector.x == 0)
        {
            animator.SetTrigger("Idle");
        }
    }*/

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
