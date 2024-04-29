using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using GrappleGame.Math;

public class MovingPlatform : MonoBehaviour, IMoveableObject
{
    public MovingPlatformData data;
    public Vector2 Velocity { get; private set; }

    private readonly string baseFilePath = "Assets/Scripts/Effects/Object Effects/Moving Platform/";
    public string filePath = "";

    // this represents the current node the platform is moving towards
    private int currentNode = 1;
    private int previousNode = 0;
    private float time = 0;
    private float t = 0;
    private float lastT = 0;

    [SerializeField]
    public float duration;

    private void Start()
    {
        filePath = baseFilePath + "MovingPlatformData_" + name + ".asset";
        data = AssetDatabase.LoadAssetAtPath<MovingPlatformData>(filePath);
    }

    void Update()
    {
        if (Math.Near(transform.position, data[currentNode], 0.01f))
        {
            previousNode = currentNode;
            currentNode = (currentNode + 1) % data.Count();
            time = 0;
        }

        lastT = t;
        Vector2 previousPoint = data[previousNode];
        Vector2 currentPoint = data[currentNode];
        Vector2 currentPath = currentPoint - previousPoint;

        time += Time.deltaTime;
        t = time / duration;
        // "smoother step" lerp taken from https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
        t = t * t * t * (t * (6f * t - 15f) + 10f);
        float deltaT = t - lastT;
        transform.position = Vector2.Lerp(previousPoint, currentPoint, t);
        // what portion of the path from current node to previous node is the most recent "instant change in position" aka "slope of the position graph" aka "velocity"
        // Note on this:
        // what's not said here is that deltaT being created from the difference in 't'!! not 'time' means that it respects the changes in speed and we're not assuming linear x(t)
        Vector2 _velocity = deltaT * currentPath;
        // at the transition between nodes velocity can increase to 100x the intended value, so we disregard these values in case they happen
        if (_velocity.magnitude < 1f) { Velocity = _velocity; }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)Velocity);
        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(transform.position, transform.position + (Vector3)(data[currentNode] - data[previousNode]));
    }

    private void OnValidate()
    {
        // if an asset does not exists at the filePath
        if (!AssetDatabase.LoadAssetAtPath<MovingPlatformData>(filePath))
        {
            Debug.Log("couldn't find Moving Platform data, generating new file");
            filePath = baseFilePath + "MovingPlatformData_" + name + ".asset";
            AssetDatabase.CreateAsset(new MovingPlatformData(), filePath);
        }
        data = AssetDatabase.LoadAssetAtPath<MovingPlatformData>(filePath);
    }

    public Vector2 GetVelocity()
    {
        //Debug.Log(Velocity);
        return Velocity;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
