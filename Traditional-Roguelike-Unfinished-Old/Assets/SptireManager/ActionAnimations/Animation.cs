using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Animation : MonoBehaviour
{
    public float currentTime;
    public float totalTime;
    public UnityAction endAnimation;

    public virtual void EndAnimation()
    {
        endAnimation?.Invoke();
        Destroy(gameObject);
    }
}
