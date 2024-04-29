using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using GrappleGame.Math;

public class Jump : MonoBehaviour
{
    Player player;

    void Start()
    {
        // I'm stealing a page from Celeste's playbook here, which is one of my favorite games.

        // What we do is simple, we squish the player horizontally almost instantly, then back to normal over a short timeframe

        player = GetComponent<Player>();
        StartCoroutine(JumpVFX(0.25f));

    }

    private IEnumerator JumpVFX(float duration)
    {
        float time = 0;
        float t = 0;
        Vector3 workspace = Vector3.zero; // dummy so we don't call new on repeat
        Vector3 parentScale = Vector3.one;

        // immediately squish inwards quickly
        while (time < duration / 10)
        {
            parentScale = transform.parent != null ? transform.parent.localScale : Vector3.one;

            workspace.Set(Mathf.Lerp(0.3f, 0.2f, t), 0.3f, 1);
            transform.localScale = workspace.Divide(parentScale);

            time += Time.deltaTime;
            t = time / (duration / 2);
            // "smoother step" lerp taken from https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
            t = t * t * t * (t * (6f * t - 15f) + 10f);

            yield return null;   
        }

        // slowly squish back to normal (time is relative, this all happens over ~0.25 s)
        while (time < duration)
        {
            parentScale = transform.parent != null ? transform.parent.localScale : Vector3.one;

            //Debug.Log(parentScale);

            workspace.Set(Mathf.Lerp(0.2f, 0.3f, t), 0.3f, 1);
            transform.localScale = workspace.Divide(parentScale);

            time += Time.deltaTime;
            t = time / duration;
            // "smoother step" lerp taken from https://chicounity3d.wordpress.com/2014/05/23/how-to-lerp-like-a-pro/
            t = t * t * t * (t * (6f * t - 15f) + 10f);

            yield return null;
        }

        // make sure we're back to normal
        player.transform.localScale.Set(0.3f / parentScale.x, 0.3f / parentScale.y, 1.0f);

        // once the VFX is finished, we can destroy the component
        Destroy(this);
    }

}
