using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public AnimationHandler animationHandler { get; private set; }

    public ParticleHandler particleHandler { get; private set; }

    public Player player {  get; private set; } 

    void Awake()
    {
        player = FindObjectOfType<Player>();
        animationHandler = GetComponent<AnimationHandler>();
        particleHandler = GetComponent<ParticleHandler>();
    }
}
