using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeDuration = 0;
    public float shakeMagnitude = 0;
    public float dampingSpeed = 0;
    public float shakeAmount = 0;

    private float time = 0;
    private float totalTime = 0;

    Vector3 initalPosition;
    Vector3 shakePosition;

    private void OnEnable()
    {
        initalPosition = transform.localPosition;
        shakePosition = initalPosition + (Vector3)UnityEngine.Random.insideUnitCircle * shakeMagnitude;
        time = 0;
        totalTime = 0;
    }

    void Update()
    {
        /*
         *      EARTHQUAKE SHAKE
         *  
        float time = 0;
        time += Time.deltaTime;
        Vector3 shakePosition = initalPosition + (Vector3)UnityEngine.Random.insideUnitCircle * shakeMagnitude;
        transform.localPosition = Vector3.Lerp(transform.localPosition, shakePosition, time);
        */

        if (totalTime < shakeDuration)
        {
            if (time < shakeDuration / (2 * shakeAmount))
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, shakePosition, time / shakeDuration * (2 * shakeAmount));
            }
            else if (time < shakeDuration / shakeAmount)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, initalPosition, (2 * time * shakeAmount / shakeDuration) - 1);
            }
            else
            {
                transform.localPosition = initalPosition;
                shakeMagnitude *= 1 - (0.04f / shakeMagnitude) <= 0 ? 1 : 1 - (0.04f / shakeMagnitude);
                shakePosition = initalPosition + (Vector3)UnityEngine.Random.insideUnitCircle * shakeMagnitude;
                time = 0;
            }
            time += Time.deltaTime;
            totalTime += Time.deltaTime;
        }
        else
        {
            transform.localPosition = initalPosition;
            enabled = false;
        }
    }
}
