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
}
