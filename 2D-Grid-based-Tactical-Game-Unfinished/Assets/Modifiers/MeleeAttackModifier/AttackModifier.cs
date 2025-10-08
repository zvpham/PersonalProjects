using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackModifier : ScriptableObject
{
    public int priority;
    public abstract void SetUnwalkable(CombatGameManager gameManager, Unit MovingUnit);
    public abstract bool ValidMovePosition(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode nextNode, int meleeRange);
}

