using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using System;

public class PlayerInputHandler : MonoBehaviour
{

    public Vector2 MoveVector { get; private set; }
    public Vector2 RawMoveVector { get; private set; }

    public float NormX { get; private set; }
    public float NormY { get; private set; }

    private bool HoldWallAttach;

    public event Action PlayerJump;
    public event Action PlayerCancelJump;

    public event Action PlayerWallAttach;
    public event Action PlayerWallDetach;

    public event Action PlayerGrapple;
    public event Action PlayerCancelGrapple;

    private void Update()
    {

        // the wall attach is a hold action, so as long as it's performed we want to keep invoking it every update
        // this is because if the player is holding the button and moves away from a wall and back they should "grab" the wall immediately
        if (HoldWallAttach)
        {
            PlayerWallAttach?.Invoke();
        }
    }

    public void MoveInput(InputAction.CallbackContext context)
    {
        RawMoveVector = context.ReadValue<Vector2>();

        // multiply the raw value by (1,0) or (0,1) to isolate the x or y component then normalize so it's x or y value will be 1 if it's any larger than 0
        NormX = (RawMoveVector * Vector2.right).normalized.x;
        NormY = (RawMoveVector * Vector2.up).normalized.y;

        MoveVector = new Vector2(NormX, NormY);
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PlayerJump?.Invoke();
        }
        else if (context.canceled)
        {
            PlayerCancelJump?.Invoke();
        }
    }

    public void WallAttachInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PlayerWallAttach?.Invoke();
            HoldWallAttach = true;
        }
        else if (context.canceled)
        {
            HoldWallAttach = false;
            PlayerWallDetach?.Invoke();
        }
    }

    public void GrappleInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PlayerGrapple?.Invoke();
        }
        else if (context.canceled)
        {
            PlayerCancelGrapple?.Invoke();
        }
    }

}
