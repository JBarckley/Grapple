using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine
{
    public PlayerState CurrentState { get; private set; }

    public void Init(PlayerState startingState)
    {
        CurrentState = startingState;
        startingState.Enter();
    }

    public void ToState(PlayerState nextState)
    {
        CurrentState.Exit();
        CurrentState = nextState;
        nextState.Enter();
    }
}
