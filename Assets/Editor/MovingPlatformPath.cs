using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using GrappleGame.Math;

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformPath : Editor
{
    private readonly string baseFilePath = "Assets/Scripts/Effects/Object Effects/Moving Platform/";
    private string filePath;
    private MovingPlatformData data;
    private MovingPlatform MovingPlatform;

    private Vector3 pos;
    private Transform tr;

    private Vector3 rawMousePosition;
    private Vector3 mousePosition;

    private bool pressed = false;

    public void Awake()
    {
        MovingPlatform = (MovingPlatform)target;
        filePath = baseFilePath + "MovingPlatformData_" + MovingPlatform.name + ".asset";

        // if an asset does not exist at the filePath
        if (!AssetDatabase.LoadAssetAtPath<MovingPlatformData>(filePath))
        {
            Debug.Log("couldn't find Moving Platform data, generating new file");
            AssetDatabase.CreateAsset(new MovingPlatformData(), filePath);
        }
        data = AssetDatabase.LoadAssetAtPath<MovingPlatformData>(filePath);

        if (data.Count() == 0)
        {
            data.Add(MovingPlatform.transform.position);
        }
        else if (!Application.isPlaying)
        {
            data[0] = MovingPlatform.transform.position;
        }
    }

    public void OnSceneGUI()
    {
        MovingPlatform = (MovingPlatform)target;
        tr = MovingPlatform.transform;
        pos = tr.position;

        rawMousePosition = Event.current.mousePosition;
        mousePosition = HandleUtility.GUIPointToWorldRay(rawMousePosition).origin;

        Handles.color = Color.red;
        Vector3[] lineSegments = new Vector3[(data.Count() - 1) * 2];
        Vector2 nodeSize = Vector2.one * 0.1f;
        Vector2 nodePoint;
        for(int i = 0; i < lineSegments.Length; i++)
        {
            nodePoint = data[(i + 1) / 2];
            lineSegments[i] = nodePoint;
            if (i % 2 == 1)
            {
                Handles.DrawSolidRectangleWithOutline(new Rect(nodePoint - (nodeSize / 2), nodeSize), Color.red, Color.black);
            }
        }
        Handles.DrawDottedLines(lineSegments, 1f);

        if (pressed)
        {
            SecondPress();
        }
        else
        {
            FirstPress();
        }
    }
    
    public void FirstPress()
    {
        if (Handles.Button(data[data.Count() - 1], Quaternion.identity, 0.1f, 0.1f, Handles.DotHandleCap))
        {
            pressed = true;
        }
    }

    public void SecondPress()
    {
        // if we're holding down left control, lock the x or y value to be exactly level with the last point
        if (Event.current.control)
        {
            // if we're on the x side of the line y = x
            if (Mathf.Abs(mousePosition.x - data[data.Count() - 1].x) > Mathf.Abs(mousePosition.y - data[data.Count() - 1].y))
            {
                // lock the x value
                mousePosition.Set(mousePosition.x, data[data.Count() - 1].y, 0);
            }
            else
            {
                // lock the y value
                mousePosition.Set(data[data.Count() - 1].x, mousePosition.y, 0);
            }
        }
        // see if the mouse is near a node, if so "snap" there
        else
        {
            // data will never grow greater than ~20ish elements, so iterating over it again is reasonable
            for (int i = 0; i < data.Count(); i++)
            {
                if (Math.Near(mousePosition, data[i], 0.1f))
                {
                    mousePosition = data[i];
                }
            }
        }

        // draw a button on the mouse cursor & a line between the current path and the new path
        if (Handles.Button(mousePosition, Quaternion.identity, 0.1f, 0.1f, Handles.DotHandleCap))
        {
            pressed = false;
            data.Add(mousePosition);
        }
        Handles.DrawDottedLine(data[data.Count() - 1], mousePosition, 1f);
    }
    
}
