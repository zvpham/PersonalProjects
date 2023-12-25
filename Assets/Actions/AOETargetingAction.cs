using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AOETargetingAction : TargetAction
{
    public CreatedField createdField;
    public CreatedObject createObject;
    public GameObject createObjectHolder;

    public int blastRadius;
    public float blastAngle;
}
