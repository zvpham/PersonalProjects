using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CreatedObjectDamage : CreatedObject
{
    public FullDamage damageCalculation;
    public bool isActive = false;

    public override bool CheckStatus(Status status)
    {
        return false;
    }
}
