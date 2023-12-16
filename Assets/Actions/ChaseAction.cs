using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChaseAction : Action
{
    abstract public void Activate(Unit self, Vector3 targetLocation);
    abstract public int CalculateWeight(Unit self, Vector3 targetLocation);
}
