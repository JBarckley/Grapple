using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "newMovingPlatformData", menuName = "Data/Moving Platform Data")]
public class MovingPlatformData : ScriptableObject
{
    [Header("This List contains all the nodes on our Moving Platform's path")]
    public List<Vector2> Path = new List<Vector2>();

    public Vector2 this[int index]
    {
        get => Path[index];
        set => Path[index] = value;
    }

    public int Count()
    {
        return Path.Count;
    }

    public void Add(Vector2 value)
    {
        Path.Add(value);
    }

    public IEnumerator GetEnumerator()
    {
        return Path.GetEnumerator();
    }
}
