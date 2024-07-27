using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public abstract class CustomAnimations : MonoBehaviour
{
    public float currentTime;
    public float totalTime;
    public int xindex;
    public SpriteManager spriteManager;
    public UnityAction endAnimation;

    public virtual void Start()
    {
        enabled = false;
    }

    public virtual void PlayAnimation()
    {
        enabled = true;
    }

    public virtual void EndAnimation()
    {
        spriteManager.animations.RemoveAt(xindex);
        spriteManager.timeBetweenAnimations.RemoveAt(0);
        spriteManager.playNewAnimation = true;

        for( int i = xindex; i < spriteManager.animations.Count; i++ )
        {
            CustomAnimations customAnimation = spriteManager.animations[i];
            customAnimation.xindex = i;
        }

        if(spriteManager.animations.Count == 0)
        {
            spriteManager.EndAnimations();
        }
        endAnimation?.Invoke();
        Destroy(gameObject);
    }
}
