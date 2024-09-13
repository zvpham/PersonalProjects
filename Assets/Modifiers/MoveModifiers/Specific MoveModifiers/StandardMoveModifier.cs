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

    public override bool ValidMovePosition(CombatGameManager gameManager, DijkstraMapNode currentNode, DijkstraMapNode neighborNode)
    {
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        int currentNodeElevation = elevationGrid[currentNode.x, currentNode.y];
        int neighborNodeElevation = elevationGrid[neighborNode.x, neighborNode.y];
        return currentNode.value + 1 < neighborNode.value && neighborNode.walkable && Mathf.Abs(currentNodeElevation - neighborNodeElevation) <= 1;
    }
}
