using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SetWorldBounds : MonoBehaviour
{
    public CameraBounds cameraBounds;
    public void Awake()
    {
        var bounds = GetComponent<SpriteRenderer>().bounds;
        cameraBounds.WorldBounds = bounds;
        Vector3 adjustedBounds = new Vector3(cameraBounds.WorldBounds.center.x, cameraBounds.WorldBounds.center.y, -10);
        cameraBounds.WorldBounds.center = adjustedBounds;
    }
}
