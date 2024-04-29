using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using GrappleGame.GizmoHelper;

public class Player : MonoBehaviour
{
    #region State Setup
    public PlayerStateMachine StateMachine { get; private set; }

    public G_PlayerIdleState G_IdleState { get; private set; }
    public G_PlayerMoveState G_MoveState { get; private set; }
    public A_PlayerIdleState A_IdleState { get; private set; }
    public A_PlayerMoveState A_MoveState { get; private set; }
    public PlayerWallClimbState WallClimbState { get; private set; }
    public PlayerNormalJumpState NormalJumpState { get; private set; }
    public PlayerWallJumpState WallJumpState { get; private set; }
    public PlayerGroundCoyoteState GroundCoyoteState { get; private set; }
    public PlayerWallCoyoteState WallCoyoteState { get; private set; }
    public PlayerOnWallState OnWallState { get; private set; }
    public PlayerGrappleState GrappleState { get; private set; }

    [SerializeField]
    public PlayerData playerData;
    #endregion

    #region Components
    public PlayerInputHandler InputHandler { get; private set; }

    public Rigidbody2D rb { get; private set; }

    public CapsuleCollider2D capsuleCollider { get; private set; }

    public SpriteRenderer Sprite { get; private set; }

    public AnimationHandler AnimationHandler { get; private set; }

    public Animator Animator { get; private set; }

    public Grapple Grapple { get; private set; }

    public MainCamera MainCamera { get; private set; }

    [SerializeField]
    public GameObject HookPrefab;

    // We need this because we create the hook when OnGrapple is called in order to lerp it for the animation before it's determined
    // whether the grappling hook hit anything. The OnGrapple method is in PlayerState, so getting a reference to a hook created there
    // in GrappleState is not possible without this centralized storage
    [NonSerialized]
    public GameObject Hook;
    #endregion

    #region Variables
    public Vector2 CurrentVelocity { get; private set; }
    public float FacingDirection { get; private set; }
    public Vector3 StartingPoint { get; private set; }

    public List<Tuple<Vector2, Vector2>> GizmoLinePoints = new();

    public GameObject groundCheck;
    public GameObject wallCheck;

    [SerializeField] public LayerMask groundLayer;
    [SerializeField] public LayerMask wallLayer;
    [SerializeField] public LayerMask killFloor;

    // we use this to enable a buffered input if the player hits the ground within a certain very small timeframe after pressing jump
    public bool JumpBuffer;

    // dummy variable to avoid making new vectors
    private Vector2 _v;
    #endregion

    #region Unity Hooks
    private void Awake()
    {
        // we need this at the start of awake so the states can reference it as a part of them being observers in the observer pattern
        InputHandler = GetComponent<PlayerInputHandler>();
        // this is referenced by AirState, which the player may start in
        AnimationHandler = this.AddComponent<AnimationHandler>();
        // logic pertaining to the VFX of the grappling rope
        Grapple = GetComponent<Grapple>();
        // functions related to moving the camera on command
        MainCamera = FindObjectOfType<MainCamera>();

        StateMachine = new PlayerStateMachine();

        G_IdleState = new G_PlayerIdleState(this, StateMachine, playerData, "g_idle");
        G_MoveState = new G_PlayerMoveState(this, StateMachine, playerData, "g_move");

        A_IdleState = new A_PlayerIdleState(this, StateMachine, playerData, "a_idle");
        A_MoveState = new A_PlayerMoveState(this, StateMachine, playerData, "a_move");

        WallClimbState = new PlayerWallClimbState(this, StateMachine, playerData, "wall_climb");

        NormalJumpState = new PlayerNormalJumpState(this, StateMachine, playerData, "jump");
        WallJumpState = new PlayerWallJumpState(this, StateMachine, playerData, "wall_jump");

        WallCoyoteState = new PlayerWallCoyoteState(this, StateMachine, playerData, "wall_coyote");
        GroundCoyoteState = new PlayerGroundCoyoteState(this, StateMachine, playerData, "ground_coyote");

        OnWallState = new PlayerOnWallState(this, StateMachine, playerData, "on_wall");

        GrappleState = new PlayerGrappleState(this, StateMachine, playerData, "grapple");
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Sprite = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        StartingPoint = transform.position;

        FacingDirection = 1;

        rb.gravityScale = 1.3f;
        // initialize the game with the player in the idle state
        StateMachine.Init(G_IdleState);
    }

    void Update()
    {
        CurrentVelocity = rb.velocity;
        StateMachine.CurrentState.Update();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentState.PhysicsUpdate();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 topL = wallCheck.transform.position;
        Vector3 topR = wallCheck.transform.position + new Vector3(transform.right.x * 0.02f, 0, 0);
        Vector3 bottomL = wallCheck.transform.position + new Vector3(0, -0.22f, 0);
        Vector3 bottomR = wallCheck.transform.position + new Vector3(transform.right.x * 0.02f, -0.22f, 0);
        //Gizmos.DrawLineList(new ReadOnlySpan<Vector3>(new Vector3[] { topL, topR, topR, bottomR, bottomR, bottomL, bottomL, topL }));
        GizmoHelper.DrawBox(new Vector3[] { topL, topR, bottomL, bottomR });
        topL = groundCheck.transform.position - new Vector3(0.15f, 0);
        topR = groundCheck.transform.position + new Vector3(0.15f, 0);
        bottomL = topL - new Vector3(0, 0.03f);
        bottomR = topR - new Vector3(0, 0.03f);
        GizmoHelper.DrawBox(new Vector3[] { topL, topR, bottomL, bottomR });


        if (Application.isPlaying)
        {
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)rb.velocity);
        }

        //Gizmos.DrawSphere(groundCheck.transform.position, 0.02f);

        /*
        foreach (Tuple<Vector2, Vector2> line in GizmoLinePoints)
        {
            Gizmos.DrawLine(line.Item1, line.Item2);
        }*/
    }

    #endregion

    #region Setter Functions
    // change the player's velocity
    public void SetVelocity(Vector2 v)
    {
        rb.velocity = v;
    }
    
    public void SetVelocity(float x, float y)
    {
        _v.Set(x, y);
        rb.velocity = _v;
        CurrentVelocity = _v;
    }

    // change only one axis of velocity
    public void SetVelocityX(float val)
    {
        _v.Set(val, rb.velocity.y);
        rb.velocity = _v;
        CurrentVelocity = _v;
    }

    public void SetVelocityY(float val)
    {
        _v.Set(rb.velocity.x, val);
        rb.velocity = _v;
        CurrentVelocity = _v;
    }

    public void AddVelocity(Vector2 v)
    {
        rb.velocity += v;
        CurrentVelocity += v;
    }

    // add to only one axis of velocity
    public void AddVelocityX(float val)
    {
        _v.Set(val, 0);
        rb.velocity += _v;
        CurrentVelocity += _v;
    }

    public void AddVelocityY(float val)
    {
        _v.Set(0, val);
        rb.velocity += _v;
        CurrentVelocity += _v;
    }
    #endregion

    #region Getter Functions

    public bool IsGrounded()
    {

        // create a 0.01 radius circle around the position of the groundcheck object (the bottom pixel of the player) and see if anything on the groundLayer overlaps
        // if something does, it is the ground so we return the amount of overlaps (which is indirectly true or false of any exist)
        //return Physics2D.OverlapCircle(groundCheck.transform.position, 0.02f, groundLayer);
        Vector2 groundCheckPosition = groundCheck.transform.position;
        return Physics2D.OverlapArea(new Vector2(groundCheckPosition.x - 0.15f, groundCheckPosition.y), new Vector2(groundCheckPosition.x + 0.15f, groundCheckPosition.y - 0.03f), groundLayer);

        /*
        Collider2D[] results = new Collider2D[5];
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.layerMask = groundLayer;
        capsuleCollider.OverlapCollider(contactFilter, results);
        return results[0] != null;
        */

    }

    public void Ground(out Collider2D ground)
    {

        RaycastHit2D _ground = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);
        ground = _ground.collider;

        /*
        Collider2D[] results = new Collider2D[5];
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.layerMask = groundLayer;
        capsuleCollider.OverlapCollider(contactFilter, results);
        //Debug.Log(results[0]);
        ground = results[0];
        */
    }

    public bool IsWalled()
    {
        // same concept, but we make a small rectangular bounding box 0.22 long and 0.01 wide to check for the wall.
        // this line is admittedly disgusting, but I like the inline nature and it's pretty digestible:
        // overlap and area bounded by two points at the corners which are the position of the top corner near the direction the player is facing and
        // the point 0.01 (just slightly) away horizontally and 0.22 lower than that corner
        Collider2D walled =  Physics2D.OverlapArea(wallCheck.transform.position, new Vector2(wallCheck.transform.position.x + (transform.right.x * 0.02f), wallCheck.transform.position.y - 0.22f), wallLayer);

        if (walled)
        {
            // set the walldirection to the right direction of the player AKA if we're hitting a wall on the right this is (1, 0) else left is (-1, 0)
            playerData.wallDirection = transform.right;
        }

        return walled;
    }

    public bool IsDead()
    {
        return Physics2D.OverlapCircle(groundCheck.transform.position, 0.1f, killFloor) || Physics2D.OverlapArea(wallCheck.transform.position, new Vector2(wallCheck.transform.position.x + 0.01f, wallCheck.transform.position.y - 0.22f), killFloor);
    }

    #endregion

    #region Do Functions

    public void Flip()
    {
        FacingDirection *= -1;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    public void CheckForFlip(Vector2 inputVector)
    {
        // if we're not idle and we're moving in the opposite direction we're facing:
        if (inputVector.x != 0 && inputVector.x != FacingDirection)
        {
            Flip();
        }
    }

    public void CheckForFlip()
    {
        if (Mathf.Round(rb.velocity.x) * FacingDirection < 0)
        {
            Flip();
        }
    }

    public void StartJumpBuffer(float time)
    {
        StartCoroutine(Buffer(time));
    }

    #endregion

    #region Coroutines

    public IEnumerator LerpVelocityX(float start, float target, float duration)
    {
        float time = 0;
        float t = 0;

        while (time < duration)
        {
            yield return null;
            SetVelocityX(Mathf.Lerp(start, target, t));
            time += Time.deltaTime;
            t = time / duration;
            // "smoother step" lerp taken from https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
            t = t * t * t * (t * (6f * t - 15f) + 10f);
        }
    }

    /*
    public IEnumerator LerpHook(Vector2 targetPos, float duration, bool hit)
    {
        float time = 0;
        float t = 0;
        Vector3 startPos = transform.position;

        AnimationHandler.GrappleAir();
        GrappleRope.enabled = true;

        while (time < duration)
        {
            yield return null;
            Hook.transform.position = Vector3.Lerp(startPos, targetPos, t);
            time += Time.deltaTime;
            t = time / duration;
            t = t * t;
            //GrappleState.DrawChain();
        }
        Hook.transform.position = targetPos;
        // if the grapple hits apply an impulse force to the player
        if (hit)
        {
            // unit tangent of the vector perpindicular to the line between the hook and the player
            Vector2 nTan = Vector3.Normalize(Vector2.Perpendicular(Hook.transform.position - transform.position));
            if (rb.velocity.magnitude < 3.0f)
            {
                Vector2 minV = new Vector2(3 * Mathf.Sign(nTan.x), 0);
                // if the player is stationary and facing left, make the minV (-3, 0)
                if (transform.right.x > 0) { minV *= -1; }
                rb.velocity = minV;
            }
            /*
             *      Idea behind incoming math:
             *      
             *      I want to give the player a scaled inpulse force when they first grapple... through iteration I've found it makes the mechanic feel better.
             *      The scaling factor I settled on here is slightly altered whether the player is moving slow or fast. If you have a lot of speed, it will go into
             *      the grapple, so I don't need to give you as much of a boost. Thus, we scale a littl less.
             *      
             *      The last part of this (which is performed first) is that we dont want players who have positive y velocity to be rewarded with speed...
             *      Without this qualification, a player who jumps and grapples at the peak of their jump would be given less speed than a player who
             *      grapples the first moment they recieve y velocity, which doesn't feel right. So we make sure y velocity can at maximum be 0.
             *      
             *      The function 9 / rb.velocity.magnitude just came to be over iteration
             * 
             *

            if (transform.right.x < 0)
            {
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, Mathf.NegativeInfinity, 0), Mathf.Clamp(rb.velocity.y, Mathf.NegativeInfinity, 0));
            }
            else
            {
                rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, 0, Mathf.Infinity), Mathf.Clamp(rb.velocity.y, Mathf.NegativeInfinity, 0));
            }
            //rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, ), Mathf.Clamp(rb.velocity.y, Mathf.NegativeInfinity, 0));
            float scalingFactor = Mathf.Clamp(9 / rb.velocity.magnitude, 1.5f, 3.0f);
            rb.velocity = Vector3.Project(rb.velocity * scalingFactor, nTan);
            //GizmoLinePoints.Add(new Tuple<Vector2, Vector2>(transform.position, transform.position + (Vector3)rb.velocity));
            // finally set the player's state to the grappling state.
            StateMachine.ToState(GrappleState);
        }
        // if the hook doesn't hit start lerping back
        if (!hit)
        {
            time = 0;
            t = 0;
            startPos = Hook.transform.position;
            while (time < duration)
            {
                Hook.transform.position = Vector3.Lerp(startPos, transform.position, t);
                time += Time.deltaTime;
                t = time / duration;
                t = t * t * t;
                //GrappleState.DrawChain();
                yield return null;
            }
            GrappleState.DestroyGrapple();

            // if we dont hit the grapple we either return to a grounded movement animation or a air falling animation:
            if (IsGrounded())
            {
                AnimationHandler.Move();
            }
            else if (!AnimationHandler.airAnim)
            {
                AnimationHandler.StartCoroutine(AnimationHandler.AirAnimation());
                AnimationHandler.airAnim = true;
            }
        }
    }
    */

    public IEnumerator Buffer(float time)
    {
        JumpBuffer = true;
        yield return new WaitForSeconds(time);
        JumpBuffer = false;
    }

    #endregion
}
