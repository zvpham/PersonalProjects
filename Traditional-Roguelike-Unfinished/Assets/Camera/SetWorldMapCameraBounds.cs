using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetWorldMapCameraBounds : MonoBehaviour
{
    public CameraBounds cameraBounds;
    public void Awake()
    {
        var bounds = GetComponent<SpriteRenderer>().bounds;
        cameraBounds.WorldBounds = bounds;
    }
}
