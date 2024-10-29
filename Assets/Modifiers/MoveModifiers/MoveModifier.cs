using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoveModifier : ScriptableObject
{
    public int priority;
    public abstract void SetUnwalkable(CombatGameManager gameManager, Unit MovingUnit);
    public abstract bool ValidMovePosition(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode nextNode);
    public abstract bool ValidMove(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode neighborNode);

    public abstract bool ValidMeleeAttack(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode nextNode, int maxRange);
}
