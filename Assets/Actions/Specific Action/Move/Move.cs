using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Move")]
public class Move : Action
{
    public override void SelectAction(Unit self)
    {
        base.SelectAction(self);
        self.gameManager.spriteManager.ActivateMovementTargeting(self, false, self.currentActionsPoints);
        self.gameManager.spriteManager.movementTargeting.OnFoundTarget += FoundTarget;
    }

    public void FoundTarget(List<Vector2Int> path, Unit movingUnit, bool foundTarget)
    {
        if (foundTarget)
        {
            DijkstraMap map = movingUnit.gameManager.map;
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 newPosition = map.getGrid().GetWorldPosition(path[i].x, path[i].y);
                movingUnit.MovePositions(movingUnit.transform.position, newPosition);
            }
            int actionPointsUsed = ((path.Count - 1) / movingUnit.moveSpeed) + 1;
            movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
            movingUnit.UseActionPoints(actionPointsUsed);
        }
        else
        {
            movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
        }
    }

    public void AnotherActionMove(List<Vector2Int> path, Unit movingUnit)
    {
        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
        DijkstraMap map = movingUnit.gameManager.map;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 newPosition = map.getGrid().GetWorldPosition(path[i].x, path[i].y);
            movingUnit.MovePositions(movingUnit.transform.position, newPosition);
        }
        int actionPointsUsed = ((path.Count - 1) / movingUnit.moveSpeed) + 1;
        movingUnit.UseActionPoints(actionPointsUsed);
    }
}
