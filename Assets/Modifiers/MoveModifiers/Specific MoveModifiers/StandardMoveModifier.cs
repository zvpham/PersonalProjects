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

    // IS it possible to make move not counting movespeed
    public override bool ValidMove(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode neighborNode)
    {
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        int currentNodeElevation = elevationGrid[currentNode.x, currentNode.y];
        int neighborNodeElevation = elevationGrid[neighborNode.x, neighborNode.y];
        return neighborNode.walkable && Mathf.Abs(currentNodeElevation - neighborNodeElevation) <= 1;
    }


    // IS it possible to make move accounting for movespeed
    public override bool ValidMovePosition(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode neighborNode,
        int moveAmountChange)
    {
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        int currentNodeElevation = elevationGrid[currentNode.x, currentNode.y];
        int neighborNodeElevation = elevationGrid[neighborNode.x, neighborNode.y];
        return currentNode.value + moveAmountChange < neighborNode.value && neighborNode.walkable && 
            Mathf.Abs(currentNodeElevation - neighborNodeElevation) <= 1;
    }

    public override bool CheckIfHexIsInMovementRange(CombatGameManager gameManager, DijkstraMapNode currentNode, 
        DijkstraMapNode nextNode, int moveAmountChange)
    {
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        int currentNodeElevation = elevationGrid[currentNode.x, currentNode.y];
        int neighborNodeElevation = elevationGrid[nextNode.x, nextNode.y];
        return currentNode.value - moveAmountChange > nextNode.value && nextNode.walkable &&
            Mathf.Abs(currentNodeElevation - neighborNodeElevation) <= 1;
    }

    public override bool ValidMeleeAttack(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode nextNode, int maxRange)
    {
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        int currentNodeElevation = elevationGrid[currentNode.x, currentNode.y];
        int neighborNodeElevation = elevationGrid[nextNode.x, nextNode.y];
        return Mathf.Abs(currentNode.value - nextNode.value) <= maxRange  && Mathf.Abs(currentNodeElevation - neighborNodeElevation) <= 1;
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
