using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoveModifier : ScriptableObject
{
    public int priority;
    public abstract void SetUnwalkable(CombatGameManager gameManager, Unit MovingUnit);

    public abstract void SetWalkable(CombatGameManager gameManager, Unit MovingUnit);

    // only checks to see if the difference in elevation between Two Hexes is valid
     public abstract bool validElevationDifference(CombatGameManager gameManager, Vector2Int currentNode, Vector2Int nextNode, int range);
    public abstract bool validElevationDifference(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode nextNode, int range);
    public abstract bool ValidMovePosition(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode nextNode,
        int moveAmountChange);

    // This is to check to see if a dijkstrade can be moved to and if there is enough movement from origin node to move to next node
    public abstract bool CheckIfHexIsInMovementRange(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode nextNode,
    int moveAmountChange);

    public abstract bool ValidMove(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode neighborNode);

    //Just Checks elevation and hex Distance
    public abstract bool NewValidMeleeAttack(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode nextNode, int maxRange);
}
