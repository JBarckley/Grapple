using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UIElements;
using GrappleGame.Math;

public class Grapple : MonoBehaviour
{
    // amount of points on the line renderer to adjust, increasing the effective curvature
    [SerializeField]
    private int precision = 40;

    public LineRenderer lr;
    public Player player;
    public GameObject Hook;
    public Vector2 hitPosition;

    private IMoveableObject moveableObject;
    private Vector2 attachedPlatformVelocity = Vector2.zero;

    [SerializeField]
    public GameObject BaseHook;

    public AnimationCurve ropeCurve;

    [Range(0.1f, 4)]
    [SerializeField]
    private float waveSizeScalar = 0.1f;

    private float waveSize = 0.1f;

    public AnimationCurve progressionCurve;

    public AnimationCurve waveSizeCurve;

    [SerializeField]
    [Range(1, 50)]
    private float progressionSpeed = 30f;

    float moveTime = 0;

    Vector3 ropeLength = Vector3.zero;

    private void OnEnable()
    {
        player = GetComponent<Player>();
        lr = GetComponent<LineRenderer>();
        Hook = Instantiate(BaseHook, player.transform.position, Quaternion.identity);


        /*          FIRST:
         *          We figure out where the grappling hook will hit the wall
         */

        Vector2 aimVec;

        // if the player is not currently moving in any horizontal direction
        if (player.InputHandler.MoveVector.x == 0f)
        {
            aimVec = new Vector2(player.FacingDirection, 1);
        }
        // otherwise take the player's movement direction
        else
        {
            aimVec = new Vector2(player.InputHandler.RawMoveVector.x, 1);
        }
        aimVec.Normalize();
        aimVec *= player.playerData.grapplingHookLength;

        // see if there is any grappleable spots

        /*      IDEA:
         *      Occasionally, the player may try to grapple with a trajectory just slightly off the intended hook position.
         *      Here, we want to help the player by still grappling them to their intended spot.
         *      
         *      To do this, if the first ray does not hit, we cast two extra rays, each a little above and below the landing position of the original.
         *      If either of these rays hit, we'll grapple the player to that position (if both hit, we'll take the max spot arbitrarily)
         */

        RaycastHit2D findGrappleable = Physics2D.Raycast(player.transform.position, aimVec, player.playerData.grapplingHookLength, player.wallLayer);
        Vector2 hitPoint;
        float rot = Mathf.PI / 17;

        if (findGrappleable.point == Vector2.zero)
        {
            Vector2 top = aimVec.Rotate(rot);
            Vector2 bottom = aimVec.Rotate(-rot);
            RaycastHit2D topRay = Physics2D.Raycast(player.transform.position, top, player.playerData.grapplingHookLength, player.wallLayer);
            RaycastHit2D botRay = Physics2D.Raycast(player.transform.position, bottom, player.playerData.grapplingHookLength, player.wallLayer);

            //Debug.Log(topRay.point + " " + botRay.point);

            if (topRay.point != Vector2.zero)
            {
                findGrappleable = topRay;
            }
            else if (botRay.point != Vector2.zero)
            {
                findGrappleable = botRay;
            }
        }

        hitPoint = findGrappleable.point;

        if (hitPoint != Vector2.zero)
        {
            GameObject grappledObject = findGrappleable.transform.gameObject;
            if (grappledObject.TryGetComponent<IMoveableObject>(out moveableObject))
            {
                Debug.Log("grapple hit moveable object " + moveableObject);
                attachedPlatformVelocity = moveableObject.GetDeltaX();
                Hook.transform.SetParent(grappledObject.transform);
            }
        }
        

        Vector2 maxThrow = Hook.transform.position + (Vector3)aimVec;
        Debug.Log(maxThrow);

        /*          SECOND:
         *          We call LerpHook which lerps the hook position to the position of the hit
         */

        if (hitPoint != Vector2.zero)
        {
            StartCoroutine(LerpHook(hitPoint, 0.05f, true));
        }
        else
        {
            StartCoroutine(LerpHook(maxThrow, 0.1f, false));
        }

        /*      THIRD:
         *      We set up for drawing the line renderer which acts as the grappling rope
         */

        ropeLength = Hook.transform.position - player.transform.position;
        moveTime = 0;
        waveSize = 0.5f;
        lr.enabled = true;
        lr.positionCount = precision;
        for (int i = 0; i < precision; i++)
        {
            lr.SetPosition(i, Hook.transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        ropeLength = Hook.transform.position - player.transform.position;

        RotateHook();

        moveTime += Time.deltaTime;
        waveSize = waveSizeCurve.Evaluate(moveTime * 2) * waveSizeScalar;
        DrawRope();
    }

    private void OnDisable()
    {
        lr.enabled = false;
        Destroy(Hook);
    }

    void DrawRope()
    {
        for (int i = 0; i < precision; i++)
        {
            float delta = i / (precision - 1f);
            Vector2 offset = Vector2.Perpendicular(ropeLength).normalized * ropeCurve.Evaluate(delta) * waveSize;
            Vector2 targetPosition = Vector2.Lerp(player.transform.position, Hook.transform.position, delta) + offset;
            Vector2 currentPosition = Vector2.Lerp(player.transform.position, targetPosition, progressionCurve.Evaluate(moveTime) * progressionSpeed);

            lr.SetPosition(i, currentPosition);
        }
    }

    void RotateHook()
    {
        Vector2 HookToPlayer = -1 * ropeLength;
        float HookToPlayerAngle = Mathf.Atan2(HookToPlayer.y, HookToPlayer.x) * Mathf.Rad2Deg;
        HookToPlayerAngle = Mathf.Clamp(HookToPlayerAngle, -135f, -45f);
        Quaternion rotation = Quaternion.AngleAxis(HookToPlayerAngle - 90, Vector3.forward);
        Hook.transform.rotation = rotation;
    }

    public IEnumerator LerpHook(Vector2 targetPos, float duration, bool hit)
    {
        float time = 0;
        float t = 0;
        Vector3 startPos = player.transform.position;

        player.AnimationHandler.GrappleAir();

        while (time < duration)
        {
            if (moveableObject != null)
            {
                targetPos += moveableObject.GetDeltaX();
                //Debug.Log(moveableObject.GetVelocity() + " " + targetPos);
            }

            yield return null;
            Hook.transform.position = Vector3.Lerp(startPos, targetPos, t);
            time += Time.deltaTime;
            t = time / duration;
            t = t * t;
        }
        Hook.transform.position = targetPos;
        // if the grapple hits apply an impulse force to the player
        if (hit)
        {
            // unit tangent of the vector perpindicular to the line between the hook and the player
            Vector2 nTan = Vector3.Normalize(Vector2.Perpendicular(Hook.transform.position - player.transform.position));
            if (player.rb.velocity.magnitude < 3.0f)
            {
                Vector2 minV = new Vector2(3 * Mathf.Sign(nTan.x), 0);
                // if the player is stationary and facing left, make the minV (-3, 0)
                if (player.transform.right.x > 0) { minV *= -1; }
                player.rb.velocity = minV;
            }

            //player.MainCamera.StartCoroutine(player.MainCamera.Shake(0.2f, 1f, 0.04f));

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
             *      EDIT: with the advent of moving platforms, we increase the pulse velocity by the velocity of the movement.
             *      
             *      The function 9 / rb.velocity.magnitude just came to be over iteration
             * 
             */

            if (player.transform.right.x < 0)
            {
                player.rb.velocity = new Vector2(Mathf.Clamp(player.rb.velocity.x, Mathf.NegativeInfinity, 0), Mathf.Clamp(player.rb.velocity.y, Mathf.NegativeInfinity, 0));
            }
            else
            {
                player.rb.velocity = new Vector2(Mathf.Clamp(player.rb.velocity.x, 0, Mathf.Infinity), Mathf.Clamp(player.rb.velocity.y, Mathf.NegativeInfinity, 0));
            }
            //rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, ), Mathf.Clamp(rb.velocity.y, Mathf.NegativeInfinity, 0));
            float scalingFactor = Mathf.Clamp(9 / player.rb.velocity.magnitude, 1.5f, 3.0f);
            player.rb.velocity = Vector3.Project(player.rb.velocity * scalingFactor + attachedPlatformVelocity, nTan);
            //GizmoLinePoints.Add(new Tuple<Vector2, Vector2>(transform.position, transform.position + (Vector3)rb.velocity));
            // finally set the player's state to the grappling state.
            player.StateMachine.ToState(player.GrappleState);

            if (moveableObject != null) 
            { 
                player.transform.SetParent(moveableObject.GetTransform()); 
            }
        }
        // if the hook doesn't hit start lerping back
        if (!hit)
        {
            StartCoroutine(LerpHookBack(duration));
        }
    }

    // TODO: write function to lerp hook back to player and play screenshake when hook returns
    public IEnumerator LerpHookBack(float duration)
    {
        float time = 0;
        float t = 0;
        Vector3 startPos = Hook.transform.position;
        while (time < duration)
        {
            Hook.transform.position = Vector3.Lerp(startPos, player.transform.position, t);
            time += Time.deltaTime;
            t = time / duration;
            t = t * t * t;
            yield return null;
        }

        // if we dont hit the grapple we either return to a grounded movement animation or a air falling animation:
        if (player.IsGrounded())
        {
            player.AnimationHandler.Move();
        }
        else
        {
            player.MainCamera.Shake(0.2f, 3f, 0.12f);
            player.AnimationHandler.StopAllCoroutines();
            player.AnimationHandler.StartCoroutine(player.AnimationHandler.AirAnimation());
        }

        Destroy(Hook);
        enabled = false;
    }
}
