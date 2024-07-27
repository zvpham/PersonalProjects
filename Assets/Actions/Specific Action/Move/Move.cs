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
                MoveAnimation moveAnimation = (MoveAnimation)Instantiate(animation);
                moveAnimation.SetParameters(movingUnit.gameManager, movingUnit.transform.position, newPosition);
                movingUnit.MovePositions(movingUnit.transform.position, newPosition);

            }
            movingUnit.gameManager.spriteManager.playNewAnimation = true;
            int amountMoveActionUsed = ((path.Count - 1) / movingUnit.moveSpeed) + 1;
            int actionPointsUsed = 0;
            for (int i = 0; i < amountMoveActionUsed; i++)
            {
                actionPointsUsed += this.intialActionPointUsage + this.actionPointGrowth * movingUnit.amountMoveUsedDuringRound;
                movingUnit.amountMoveUsedDuringRound += 1;
            }
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

        if(path.Count == 0 || (path.Count == 1 && 
            movingUnit.gameManager.map.getGrid().GetWorldPosition(path[0].x, path[0].y) == movingUnit.transform.position))
        {
            return;
        }
        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
        DijkstraMap map = movingUnit.gameManager.map;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 newPosition = map.getGrid().GetWorldPosition(path[i].x, path[i].y);
            MoveAnimation moveAnimation = (MoveAnimation)Instantiate(animation);
            moveAnimation.SetParameters(movingUnit.gameManager, movingUnit.transform.position, newPosition);
            movingUnit.MovePositions(movingUnit.transform.position, newPosition);
        }
        movingUnit.gameManager.spriteManager.playNewAnimation = true;
        int amountMoveActionUsed = ((path.Count - 1) / movingUnit.moveSpeed) + 1;
        int actionPointsUsed = 0;
        for (int i = 0; i < amountMoveActionUsed; i++)
        {
            actionPointsUsed += this.intialActionPointUsage + this.actionPointGrowth * movingUnit.amountMoveUsedDuringRound;
            movingUnit.amountMoveUsedDuringRound += 1;
        }

        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
        movingUnit.UseActionPoints(actionPointsUsed);
    }
}
