using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoveModifier : ScriptableObject
{
    public int priority;
    public abstract void SetUnwalkable(CombatGameManager gameManager, Unit MovingUnit);
}
