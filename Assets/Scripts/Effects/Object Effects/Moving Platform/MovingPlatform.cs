using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using GrappleGame.Math;

public class MovingPlatform : MonoBehaviour, IMoveableObject
{
    //public MovingPlatformData data;

    public List<Vector2> platformData = new List<Vector2>();
    public Vector2 DeltaX { get; private set; }

    // this represents the current node the platform is moving towards
    private int currentNode = 1;
    private int previousNode = 0;
    private float time = 0;
    private float t = 0;
    private float lastT = 0;

    [SerializeField]
    public float duration;

    void Awake()
    {
        platformData.Add(new Vector2(-2.58f, -1.68f));
        platformData.Add(new Vector2(2.33f, -1.68f));
        platformData.Add(new Vector2(-2.58f, -1.68f));
    }   

    void Update()
    {
        if (Math.Near(transform.position, platformData[currentNode], 0.01f))
        {
            previousNode = currentNode;
            currentNode = (currentNode + 1) % platformData.Count;
            time = 0;
        }

        lastT = t;
        Vector2 previousPoint = platformData[previousNode];
        Vector2 currentPoint = platformData[currentNode];
        Vector2 currentPath = currentPoint - previousPoint;

        time += Time.deltaTime;
        t = time / duration;
        // "smoother step" lerp taken from https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
        t = t * t * t * (t * (6f * t - 15f) + 10f);
        float deltaT = t - lastT;
        transform.position = Vector2.Lerp(previousPoint, currentPoint, t);
        // what portion of the path from current node to previous node is the most recent "instant change in position" aka "slope of the position graph" aka "velocity"
        // Note on this:
        // what's not said here is that deltaX being created from the difference in 't'!! not 'time' means that it respects the changes in speed and we're not assuming linear x(t)
        Vector2 _deltaX = deltaT * currentPath;
        // at the transition between nodes velocity can increase to 100x the intended value, so we disregard these values in case they happen
        if (_deltaX.magnitude < 1f) { DeltaX = _deltaX; }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)GetVelocity());
            //Gizmos.color = Color.blue;
            //Gizmos.DrawLine(transform.position, transform.position + (Vector3)(data[currentNode] - data[previousNode]));
        }
    }

    public Vector2 GetDeltaX()
    {
        //Debug.Log(Velocity);
        return DeltaX;
    }

    public Vector2 GetVelocity()
    {
        float _t = time / duration;
        // this is the derivative of x(t) = t * t * t * (t * (6f * t - 15f) + 10f);
        float _velocity = 30 * _t * _t * ((_t * _t) - (2 * _t) + 1);
        Vector2 _velocityDirection = (platformData[currentNode] - platformData[previousNode]).normalized;
        return _velocityDirection * _velocity * 2;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
