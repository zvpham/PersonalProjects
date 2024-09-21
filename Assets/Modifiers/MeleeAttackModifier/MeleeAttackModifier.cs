using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeAttackModifier : MonoBehaviour
{
    public int priority;
    public int meleeRange
    public abstract void SetUnwalkable(CombatGameManager gameManager, Unit MovingUnit);
    public abstract bool ValidMovePosition(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode nextNode);
}

