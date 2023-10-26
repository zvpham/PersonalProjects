using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SetWorldBounds : MonoBehaviour
{
    public void Awake()
    {
        var bounds = GetComponent<SpriteRenderer>().bounds;
        CameraGlobal.WorldBounds = bounds;
    }
}
