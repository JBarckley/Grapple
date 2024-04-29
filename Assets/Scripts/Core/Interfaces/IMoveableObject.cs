using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveableObject
{
    public Vector2 GetVelocity();

    public Transform GetTransform();
}
