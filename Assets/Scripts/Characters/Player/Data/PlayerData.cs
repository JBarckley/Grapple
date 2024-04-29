using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "newPlayerData", menuName = "Data/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Movement Qualifiers")]
    public float movementVelocity = 2.5f;
    public float jumpingPower = 5f;
    public float wallSlideSpeed = 1.6f;
    public float grapplingHookLength = 2f;
    [Header("Don't change these, they will reset to these values")]
    public float coyoteFrames = 45f;
    public float bHopFrames = 0f;
    public Vector2 bHopVelocity;
    public int jumps = 2;
    public Vector2 wallDirection = Vector2.zero;

    [Space]
    [Header("Wall Jump Conditions")]
    [SerializeField]
    public float xScalar = 1.5f;
    [SerializeField]
    public float yScalar = 1.1f;
    [SerializeField]
    public float dampeningScalar = 0.045f;

}
