using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Animator anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("Explosion: Unable to find Animation script");
        }
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        float explosionClipLength = clips[0].length;
        Destroy(this.gameObject, explosionClipLength);
    }

}
