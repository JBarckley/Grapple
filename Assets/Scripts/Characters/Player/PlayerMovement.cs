using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    private Vector2 startingPoint; // set to the position of the character at the start of the script
    private GameObject groundCheckObj;
    private GameObject wallCheckObj;
    private Transform groundCheck;
    private Transform wallCheck;

    private float horizontal;
    private float exactHorizontal;
    private float vertical;
    private float speed = 2.5f;
    private float jumpingPower = 5f;
    private bool isFacingRight = true;

    private bool isWallSliding;
    private float wallSlidingSpeed = 1.6f;
    private float wallDirection = 0f;
    private float movingTowardsWall = 0f; // negative value -> moving away from wall : positive value -> moving toward wall

    private bool wallJumping = false;
    private float wallJumpSpeed = 1f;

    [SerializeField] private GameObject baseHook;
    [SerializeField] private GameObject chainLink;
    private bool canGrapple = true;
    private RaycastHit2D findGrappleable;
    private Vector2 aimVec;
    private GameObject Hook;
    private List<GameObject> chain = new List<GameObject>();
    private bool hit = true;
    private bool isGrappling = false;

    // ooo spicy
    private float bHopFrames = 0f;
    private Vector2 bHopVelo;
    private bool isBhopping;
    // set this variable to change the minimum speed needed to perform a bhop (setting this slightly larger than I actually want seems like a best practice for edge cases)
    private float bHopSpeed = 2.6f;

    // used for the input buffer system
    private bool noRepeat = false;
    private bool finish = false;

    private float coyote = 35f; // coyote time (20 frames)
    private float recentSurface = 1f; // the most recent platform (1 = ground, 2 = wall)

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask killFloor;

    // Start is called before the first frame update
    void Start()
    {
        startingPoint = transform.position;
        groundCheckObj = new GameObject();
        wallCheckObj = new GameObject();
        groundCheckObj.transform.SetParent(transform, false);
        groundCheckObj.transform.localPosition = new Vector2(0, -0.5f);
        wallCheckObj.transform.SetParent(transform, false);
        wallCheckObj.transform.localPosition = new Vector2(0.5f, 0.35f);
        groundCheck = groundCheckObj.transform;
        wallCheck = wallCheckObj.transform;

        rb.gravityScale = 1.3f;
    }

    // Update is called once per frame
    void Update()
    {

        movingTowardsWall = horizontal * wallDirection;

        if (Death())
        {
            rb.velocity = Vector2.zero;
            DestroyGrapple();
            transform.position = startingPoint;
        }

        // gives the player x frames after landing to perform a jump and bhop
        if (bHopFrames > 0f) 
        { 
            bHopFrames--;
        }
        // if bHop frames run out and the player's velocity is not greater than normal (they're not bhopping) we know they're not bhopping
        if (bHopFrames <= 0 && Mathf.Abs(rb.velocity.x) < bHopSpeed)
        {
            isBhopping = false;
        }

        /*
         *          VELOCITY CONDITIONS
         */

        if (isGrappling)
        {
            float relY = transform.position.y - Hook.transform.position.y;
            float relX = transform.position.x - Hook.transform.position.x;
            // if the player swings 10 degrees higher than the grapple point, break the chain (uses arctan and trig properties, pi/18 is 10 deg in radians)
            // only check this condition if the player is higher than the grapple point on the y axis
            if (relY > 0 && Mathf.Abs(Mathf.Atan(relY / relX)) >= Mathf.PI / 18)
            {
                DestroyGrapple();
            }
            // else if moving towards the grapple point, destroy grapple
            else if (Mathf.Abs(rb.velocity.x) <= 0.05f)
            {
                DestroyGrapple();
            }
            else if (IsGrounded())
            {
                DestroyGrapple();
            }
            else
            {
                // normal vector tangent to the circle created with radius r = distance between the player and the hook.
                Vector2 nTan = Vector3.Normalize(Vector2.Perpendicular(Hook.transform.position - transform.position));
                // put the player speed in the direction of the tangent normal and add the vertical velocity projected onto the past velocity vector
                rb.velocity = Vector3.Project(rb.velocity, nTan);
            }
        }
        else if (!wallJumping)
        {
            Vector2 _v = new Vector2(horizontal * speed, rb.velocity.y);
            if (!IsGrounded())
            {
                // if the player's velocity is moving in the same direction as the current movement
                if (horizontal * rb.velocity.x > 0)
                {
                    _v = _v.magnitude >= rb.velocity.magnitude ? _v : rb.velocity;
                }
                // if the player is moving with speed and moves AGAINST THEIR SPEED, we dampen their motion so it feels a little better
                else if (Mathf.Abs(rb.velocity.x) > bHopSpeed)
                {
                    rb.velocity = new Vector2(rb.velocity.x + (horizontal * 0.05f), rb.velocity.y);
                }
            }
            // if you're not walljumping, are grounded, and your velo is greater than regular speed (so it's the first frame you've been grounded after grappling and not hit a wall)
            else if(Mathf.Abs(rb.velocity.x) > bHopSpeed)
            {
                bHopFrames = 50f;
                bHopVelo = rb.velocity;
            }
            // if the player is not bhopping use normal movement
            if (!isBhopping)
            {
                rb.velocity = _v;
            }
        }
        else if (wallJumping)
        {
            // if walljumping, player input moves against the natural wall jump motion to dampen it.
            // at the start of the walljump, rb.velocity.y will be large, so the player's input matters more than at
            // the end of the walljump when rb.velocity.y is small. (when rb.velocity.y is < 0, the walljump ends)

            // if horizontal * wallDirection > 0, we know both have the same sign so the player is moving towards the
            // wall they jumped off hence the dampening motion. Otherwise, we would amplify a jump away from the
            // wall, which I do not want.
            if (movingTowardsWall >= 0)
            {
                rb.velocity = new Vector2(rb.velocity.x + (horizontal * 0.015f), rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
            }
        }

        if (!isFacingRight && horizontal > 0f)
        {
            Flip();
        }
        else if (isFacingRight && horizontal < 0f)
        {
            Flip();
        }

        WallSlide();

        if (IsWalled() || IsGrounded())
        {
            coyote = 35f;
        }
        else
        {
            coyote -= 1f;
        }

        // after the peak of the jump, or if the player is moving away from the wall most of the way through the jump, give back full control
        // check for IsGrounded as an extra layer of assurance wallJumping will be false.
        // The intention of this is for some minute details to the movement that feel better for the player.
        if (rb.velocity.y < 0f || IsGrounded() || (movingTowardsWall < 0 && rb.velocity.y < 0.4f))
        {
            wallJumping = false;
        }
        
        // draw the chain attached to the grappling hook
        DrawChain();

    }

    public void Jump(InputAction.CallbackContext context)
    {

        // send the input to a buffer that waits x seconds and calls the function again with the same input
        // thus, if the player is slightly early, their jump input will still get recognized
        if (context.performed && !noRepeat && !IsGrounded())
        {
            StartCoroutine(InputBuffer(context));
        }
        if (context.performed && coyote >= 0f)
        {
            // Jumping from a wall
            if (recentSurface == 2f)
            {
                // only change the jumping calculation if played moves toward the wall they're jumping from
                /*if (horizontal * wallDirection > 0f)
                {
                    wallJumping = true;
                }*/
                wallJumping = true;
                wallJumpSpeed = 3.6f;
                jumpingPower -= 1.6f; // wall jump is more vertical and less horizontal
                rb.velocity = new Vector2(-wallDirection * wallJumpSpeed, jumpingPower);
                jumpingPower = 5f;
            }
            // Jumping from the ground
            else if (recentSurface == 1f)
            {
                if (bHopFrames > 0f)
                {
                    // bHopping feels good when you get slighly more speed and the jumping height is a little bit related to the speed (the higher the speed in [8.0f, 11.0f] the less y velo on the jump 
                    rb.velocity = new Vector2(bHopVelo.x * 1.15f, jumpingPower * Mathf.Clamp(10 / bHopVelo.x, 0.9f, 1.4f));
                    isBhopping = true;
                }
                else
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
                }
            }
        }

        if (context.canceled && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }
    public IEnumerator InputBuffer(InputAction.CallbackContext context)
    {
        Coroutine c_g = StartCoroutine(c_Grounded());
        Coroutine c_t = StartCoroutine(c_Time());
        yield return new WaitUntil(() => finish == true);
        StopCoroutine(c_g);
        StopCoroutine(c_t);
        noRepeat = true;
        Jump(context);
        noRepeat = false;
        finish = false;
    }

    public IEnumerator c_Grounded()
    {
        yield return new WaitUntil(() => IsGrounded());
        finish = true;
    }

    public IEnumerator c_Time()
    {
        yield return new WaitForSeconds(0.1f);
        finish = true;
    }

    public void Grapple(InputAction.CallbackContext context)
    {
        if (canGrapple)
        {
            if (context.performed)
            {
                // if the player is only holding a verticl direction, or is holding no direction, do not let them grapple.
                if (horizontal != 0)
                {
                    // see if there is any grappleable spots
                    aimVec = new Vector2(exactHorizontal, vertical);
                    aimVec.Normalize();
                    aimVec *= 2;
                    findGrappleable = Physics2D.Raycast(transform.position, aimVec, 4f, wallLayer);

                    // send out grapple projectile
                    Hook = Instantiate(baseHook, transform.position, Quaternion.identity);
                    Vector2 hitPoint = findGrappleable.point;
                    Vector2 maxThrow = Hook.transform.position + (Vector3)aimVec;
                    if (hitPoint != Vector2.zero)
                    {
                        hit = true;
                        StartCoroutine(LerpHook(hitPoint, 0.1f, Hook));
                    }
                    else
                    {
                        hit = false;
                        StartCoroutine(LerpHook(maxThrow, 0.1f, Hook));
                    }

                    // disallow sending another grapple projectile while one is out
                    canGrapple = false;
                }
            }
        }

        if (context.canceled && hit)
        {
            // destroy grapping hook & all chain links           
            DestroyGrapple();
        }
    }

    IEnumerator LerpHook(Vector2 targetPos, float duration, GameObject Hook)
    {
        float time = 0;
        float t = 0;
        Vector3 startPos = Hook.transform.position;

        while (time < duration)
        {
            yield return null;
            Hook.transform.position = Vector3.Lerp(startPos, targetPos, t);
            time += Time.deltaTime;
            t = time / duration;
            t = t * t;
        }
        Hook.transform.position = targetPos;
        if (hit)
        {

            /* SCRAPPED /
            // Check if the player's grappling trajectory will slightly hit the ground or another block
            // Mathf.Abs(((Vector3)targetPos - transform.position).magnitude)
            List<RaycastHit2D> gBlockers = new List<RaycastHit2D>();
            ContactFilter2D cF = new ContactFilter2D();
            cF.layerMask = groundLayer;
            cF.useLayerMask = true;
            // the distance is between player and hook times a scalar that accounts for the dimensions of the player and gravity making the "circle" of motion slightly eliptical
            float gDistance = Mathf.Abs(((Vector3)targetPos - transform.position).magnitude) * 1.1f;
            int _ = Physics2D.Raycast(targetPos, Vector2.down, cF, gBlockers, gDistance);
            foreach (RaycastHit2D ray in gBlockers)
            {
                // do something if the distance is just slightly less than the circle needs to have a good grapple
                Debug.Log(gDistance - ray.distance);
                if (gDistance - ray.distance <= 0.3)
                {
                    // find where we want to slightly move the player so they're in a good range
                    // here's the math:
                    // We project a vector with the magnitude of gDistance - ray.distance onto the vector traveling from the player to the hook
                    Vector2 gPos = (Vector3.Project(new Vector3(gDistance - ray.distance, 0, 0), (Vector3)targetPos - transform.position));
                    Debug.Log(gPos);
                    /*
                    time = 0;
                    t = 0;
                    float gDuration = 0.03f;
                    Vector2 playerStart = transform.position;
                    while (time < gDuration)
                    {
                        //Debug.Log(rb.velocity);
                        transform.position = Vector3.Lerp(playerStart, playerStart + (gPos * 10), t);
                        time += Time.deltaTime;
                        t = time / gDuration;
                        yield return null;
                    }
                    //rb.velocity += gPos * 20;
                    //transform.position = transform.position + ((Vector3)gPos * 10);

                }
            }

            */

            isGrappling = true;
            Vector2 nTan = Vector3.Normalize(Vector2.Perpendicular(Hook.transform.position - transform.position));
            if (rb.velocity.magnitude < 3.0f) {
                Vector2 minV = new Vector2(3 * Mathf.Sign(nTan.x), 0);
                // if the player is stationary and facing left, make the minV (-3, 0)
                if (isFacingRight) { minV *= -1; }
                rb.velocity = minV;
            }
            rb.velocity = Vector3.Project(rb.velocity * Mathf.Clamp(9 / rb.velocity.magnitude, 1.5f, 3.0f), nTan);
        }

        // if the hook doesn't hit or button is unpressed start lerping back, but quicker
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
                yield return null;
            }
            Hook.transform.position = transform.position;
            DestroyGrapple();
        }
    }

    private void DrawChain()
    {
        // only draw the chain if a hook exists
        if (Hook)
        {
            // Idea: Draw a line between player and grapple hook point, linerally interpolate on the line and place a chain every x lengths.
            foreach (GameObject link in chain)
            {
                Destroy(link);
            }
            chain.Clear();
            Vector3 start = Hook.transform.position;
            Vector3 traversalLine = transform.position - start;
            for (int i = 1; i < 8; i++)
            {
                start += traversalLine / 8; // here x is the magnitude of the distance between the player and the hook times 2. it means as the player gets futher away, the chain length will seem more tense.
                chain.Add(Instantiate(chainLink, start, Quaternion.identity));
            }
        }
        // there are race conditions related to Destroy(Hook) seeming to perform after Update() of a frame, so Hook will be true on the same frame it is destroyed.
        // here, we clear the capacity of the chain list no matter what if there is no hook in the world.
        else if (chain.Capacity > 0)
        {
            foreach (GameObject link in chain)
            {
                Destroy(link);
            }
            chain.Clear();
            isGrappling = false;
        }
    }

    private void DestroyGrapple()
    {
        if (Hook)
        {
            // Destroy Hook object & the chainlink objects
            Destroy(Hook);
            foreach (GameObject link in chain)
            {
                Destroy(link);
            }
            chain.Clear();

            // Set important variables so the game knows we are not longer grappling & ready to bhop (ooo spicy)
            canGrapple = true;
            isGrappling = false;
        }
    }

    public bool IsGrounded()
    {
        if (Physics2D.OverlapCircle(groundCheck.position, 0.05f, groundLayer))
        {
            if (bHopFrames <= 0f)
            {
                isBhopping = false;
            }
            recentSurface = 1f;
            return true;
        }
        return false;
    }

    private bool IsWalled()
    {
        if (Physics2D.OverlapArea(wallCheck.position, new Vector2(wallCheck.position.x + 0.01f, wallCheck.position.y - 0.22f), wallLayer))
        {
            recentSurface = 2f;
            wallDirection = isFacingRight ? 1f : -1f;
            return true;
        }
        return false;
    }

    private bool Death()
    {
        // use the ground and wall overlap strategy to check for a block with the tag "killfloor" colliding with the player
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, killFloor) || Physics2D.OverlapArea(wallCheck.position, new Vector2(wallCheck.position.x + 0.01f, wallCheck.position.y - 0.22f), killFloor);
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    public void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
        exactHorizontal = context.ReadValue<Vector2>().x;
        // allow the player to hold down and maintain maximum velocity.
        if (horizontal > 0.5f) { horizontal = 1f; }
        if (horizontal < -0.5f) { horizontal = -1f; }
        vertical = context.ReadValue<Vector2>().y;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheck.position, 0.05f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + 0.01f, wallCheck.position.y, 0));
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x, wallCheck.position.y - 0.22f, 0));
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + rb.velocity.x, transform.position.y + rb.velocity.y));
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x + (horizontal * 2f), transform.position.y + (vertical * 2f), 0));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(aimVec.x, aimVec.y, 0));
    }

}
