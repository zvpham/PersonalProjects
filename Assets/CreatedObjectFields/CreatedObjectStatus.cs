using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CreatedObjectStatus : CreatedObject
{
    public Grid<CreatedObjectStatus> grid;
    public float quickness = 1;
    public float timeflow = 1;
    public int duration;
    public Status[] statuses;
    public GameObject spriteObject;
    public float blastRadius;
        
    public override string ToString()
    {
        return timeflow.ToString();
    }
}
