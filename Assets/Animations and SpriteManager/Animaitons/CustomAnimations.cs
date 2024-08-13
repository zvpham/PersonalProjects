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
    public bool disableOnStart = true;
    //public int xindex;
    //public int yindex;
    public SpriteManager spriteManager;
    public UnityAction endAnimation;

    public virtual void Start()
    {
        if(disableOnStart)
        {
            enabled = false;
        }
    }

    public virtual void PlayAnimation()
    {
        spriteManager.ChangePlayNewAnimation(false);
        enabled = true;
        disableOnStart = false;
    }

    public virtual void EndAnimation()
    {
        spriteManager.animations[0].Remove(this);
        if (spriteManager.animations[0].Count == 0)
        {
            spriteManager.animations.RemoveAt(0);
            spriteManager.timeBetweenAnimations.RemoveAt(0);
            spriteManager.ChangePlayNewAnimation(true);
        }


        if(spriteManager.animations.Count == 0)
        {
            spriteManager.EndAnimations();
        }
        endAnimation?.Invoke();
        Destroy(gameObject);
    }
}
