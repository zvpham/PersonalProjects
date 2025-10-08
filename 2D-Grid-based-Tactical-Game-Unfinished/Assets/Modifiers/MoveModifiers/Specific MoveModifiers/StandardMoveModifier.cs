using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Modifier/MoveModifer/StandardMoveModifer")]
public class StandardMoveModifier : MoveModifier
{
    public override void SetUnwalkable(CombatGameManager gameManager, Unit movingUnit)
    {
        GridHex<DijkstraMapNode> grid = gameManager.map.getGrid();
        DijkstraMap map = gameManager.map;
        for (int i = 0; i < gameManager.units.Count; i++)
        {
            if (gameManager.units[i].team != movingUnit.team)
            {
                grid.GetXY(gameManager.units[i].transform.position, out int unitX, out int unitY);
                map.SetUnwalkable(new Vector2Int(unitX, unitY));
            }
        }
    }

    public override void SetWalkable(CombatGameManager gameManager, Unit movingUnit)
    {
        GridHex<DijkstraMapNode> grid = gameManager.map.getGrid();
        DijkstraMap map = gameManager.map;
        for (int i = 0; i < gameManager.units.Count; i++)
        {
            if (gameManager.units[i].team != movingUnit.team)
            {
                grid.GetXY(gameManager.units[i].transform.position, out int unitX, out int unitY);
                map.SetWalkable(new Vector2Int(unitX, unitY));
            }
        }
    }

    public override bool validElevationDifference(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode nextNode, int range)
    {
        Vector2Int currentNodePosition = new Vector2Int(currentNode.x, currentNode.y);
        Vector2Int nextNodePosition = new Vector2Int(nextNode.x, nextNode.y);
        return validElevationDifference(gameManager, currentNodePosition, nextNodePosition, range);
    }

    public override bool validElevationDifference(CombatGameManager gameManager, Vector2Int currentNode, Vector2Int nextNode, int range)
    {
        int originElevation = gameManager.spriteManager.elevationOfHexes[currentNode.x, currentNode.y];
        int targetElevation = gameManager.spriteManager.elevationOfHexes[nextNode.x, nextNode.y];
        return (originElevation == targetElevation || Mathf.Abs(originElevation - targetElevation) <= range);
    }

    // IS it possible to make move not counting movespeed
    public override bool ValidMove(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode neighborNode)
    {
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        int currentNodeElevation = elevationGrid[currentNode.x, currentNode.y];
        int neighborNodeElevation = elevationGrid[neighborNode.x, neighborNode.y];
        return neighborNode.walkable && Mathf.Abs(currentNodeElevation - neighborNodeElevation) <= 1;
    }


    // IS it possible to make move accounting for movespeed
    // Start node is 0 and expand increase by move AMount Change
    public override bool ValidMovePosition(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode neighborNode,
        int moveAmountChange, bool debug = false)
    {
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        int currentNodeElevation = elevationGrid[currentNode.x, currentNode.y];
        int neighborNodeElevation = elevationGrid[neighborNode.x, neighborNode.y];
        if (debug)
        {
            Debug.Log("Check Valid Move: " + neighborNode.x + ", " + neighborNode.y + ": " + (currentNode.value + moveAmountChange) + ", " + neighborNode.value + ", "  +  neighborNode.walkable + ", " + Mathf.Abs(currentNodeElevation - neighborNodeElevation));
        }
        return currentNode.value + moveAmountChange < neighborNode.value && neighborNode.walkable && 
            Mathf.Abs(currentNodeElevation - neighborNodeElevation) <= 1;
    }

    // this is for when we start at 0 for goals (nodes need to be walkable, accounted for when called for by permissable moves)
    public override bool ValidMovePositionNoWalkable(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode nextNode,
        int moveAmountChange, bool debug = false)
    {
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        int currentNodeElevation = elevationGrid[currentNode.x, currentNode.y];
        int neighborNodeElevation = elevationGrid[nextNode.x, nextNode.y];
        if (debug)
        {
            Debug.Log("Check Valid Move: " + nextNode.x + ", " + nextNode.y + ": " + (currentNode.value + moveAmountChange) + ", " + nextNode.value + ", " + nextNode.walkable + ", " + Mathf.Abs(currentNodeElevation - neighborNodeElevation));
        }
        return currentNode.value + moveAmountChange < nextNode.value && Mathf.Abs(currentNodeElevation - neighborNodeElevation) <= 1;
    }

    // start node is equal to movesped and decreease evey step away
    public override bool CheckIfHexIsInMovementRange(CombatGameManager gameManager, DijkstraMapNode currentNode, 
        DijkstraMapNode nextNode, int moveAmountChange)
    {
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        int currentNodeElevation = elevationGrid[currentNode.x, currentNode.y];
        int neighborNodeElevation = elevationGrid[nextNode.x, nextNode.y];
        return currentNode.value - moveAmountChange > nextNode.value && nextNode.walkable &&
            Mathf.Abs(currentNodeElevation - neighborNodeElevation) <= 1;
    }

    public override bool NewValidMeleeAttack(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode nextNode, int maxRange)
    {
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        int currentNodeElevation = elevationGrid[currentNode.x, currentNode.y];
        int neighborNodeElevation = elevationGrid[nextNode.x, nextNode.y];
        Vector2Int currentNodePosition =  new Vector2Int(currentNode.x, currentNode.y);
        Vector2Int nextNodePosition = new Vector2Int(nextNode.x, nextNode.y);
        return  gameManager.grid.OffsetDistance(currentNodePosition, nextNodePosition) <= maxRange && Mathf.Abs(currentNodeElevation - neighborNodeElevation) <= 1;
    }
}   
