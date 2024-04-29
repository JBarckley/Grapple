using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Rendering;

public class MainCamera : MonoBehaviour
{
    [SerializeField]
    private GameObject go;
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private GameObject plr;
    [SerializeField]
    AudioClip music;
    [SerializeField]
    private float xMin;
    [SerializeField]
    private float xMax;
    [SerializeField]
    private float yMin;
    [SerializeField]
    private float yMax;
    private Vector3 cameraV = Vector3.zero;
    private Vector3 playerPos;
    private AudioSource source;

    private CameraShake CameraShake;

    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(2560, 1440, false);

        source = plr.AddComponent<AudioSource>();
        source.clip = music;
        source.volume = 0.2f;
        source.loop = true;
        source.Play();

        CameraShake = go.AddComponent<CameraShake>();
        CameraShake.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //_camera.transform.position = new Vector3(Mathf.Clamp(player.transform.position.x, -2, 2), Mathf.Clamp(player.transform.position.y, -2, 2), _camera.transform.position.z);

        // make playerPos variable for ease of use
        // We clamp x & y so the camera doesn't show what is outside the game area, set y to zero so the camera stays level and doesn't follow jump, and set z to -10 so the camera says set on the z axis.

        /*
        playerPos = new Vector3(Mathf.Clamp(plr.transform.position.x, xMin, xMax), Mathf.Clamp(plr.transform.position.y, yMin, yMax), -10);
        if (Mathf.Abs(transform.position.x - plr.transform.position.x) > 0.1f || Mathf.Abs(transform.position.y - plr.transform.position.y) > 0.1f)
        {
            transform.position = Vector3.SmoothDamp(transform.position, playerPos, ref cameraV, 0.1f);
        }*/

    }

    // For reference:
    // Every 0.04 in camera transform is a single pixel, so 0.2 would be 5 pixels & 0.04 is a single pixel
    /*
    public IEnumerator Shake(float durationSeconds, float shakes, float shakingFactor)
    {
        float time = 0;
        float currentShake = 1;
        Vector3 startPos = transform.position;

        while (time < durationSeconds)
        {
            time += Time.deltaTime;
            transform.position += (Vector3)Random.insideUnitCircle * shakingFactor;
            while (time < currentShake * (durationSeconds / shakes))
            {
                time += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, startPos, time / currentShake * (durationSeconds / shakes));
                yield return null;
            }
            transform.position = startPos;
            currentShake++;
        }
    }*/

    public void Shake(float durationSeconds, float shakeAmount = 5f, float shakeMagnitude = 0.04f, float dampingSpeed = 1f)
    {
        CameraShake.enabled = true;
        CameraShake.shakeMagnitude = shakeMagnitude;
        CameraShake.dampingSpeed = dampingSpeed;
        CameraShake.shakeDuration = durationSeconds;
        CameraShake.shakeAmount = shakeAmount;
    }

    private void OnDrawGizmos()
    {
        // We know the Camera's vertical bound is + or - _camera.orthographicSize on either side
        // We can use math to figure out the horizonal bound is
        // _camera.orthographicSize * _camera.aspect

        float verticalBound = _camera.orthographicSize;
        float horizontalBound = _camera.orthographicSize * _camera.aspect;
        Vector3 bottomLeft = new Vector3(-horizontalBound , -verticalBound, 0);
        bottomLeft += transform.position;
        Vector3 bottomRight = new Vector3(horizontalBound, -verticalBound, 0);
        bottomRight += transform.position;
        Vector3 topRight = new Vector3(horizontalBound, verticalBound, 0);
        topRight += transform.position;
        Vector3 topLeft = new Vector3(-horizontalBound, verticalBound, 0);
        topLeft += transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawLineList(new System.ReadOnlySpan<Vector3>(new Vector3[] { bottomLeft, bottomRight, bottomRight, topRight, topRight, topLeft, topLeft, bottomLeft }));
        bottomLeft += new Vector3(xMin, yMin);
        bottomRight += new Vector3(xMax, yMin);
        topLeft += new Vector3(xMin, yMax);
        topRight += new Vector3(xMax, yMax);
        Gizmos.color = Color.black;
        Gizmos.DrawLineList(new System.ReadOnlySpan<Vector3>(new Vector3[] { bottomLeft, bottomRight, bottomRight, topRight, topRight, topLeft, topLeft, bottomLeft }));
    }

}
