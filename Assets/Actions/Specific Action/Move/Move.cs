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

    public void FoundTarget(List<Vector2Int> path, Unit movingUnit, int amountMoved, bool foundTarget)
    {
        if (foundTarget)
        {
            if (movingUnit.gameManager.spriteManager.GetWorldPosition(path[0].x, path[0].y) == movingUnit.transform.position)
            {
                path.RemoveAt(0);
            }

            if (path.Count == 0)
            {
                movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
                movingUnit.gameManager.spriteManager.ActivateMovementTargeting(movingUnit, false, movingUnit.currentActionsPoints);
                movingUnit.gameManager.spriteManager.movementTargeting.OnFoundTarget += FoundTarget;
                return;
            }

            string pathDebug = "";
            for (int i = 0; i < path.Count; i++)
            {
                pathDebug += path[i].ToString() + ", ";
            }

            DijkstraMap map = movingUnit.gameManager.map;
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 newPosition = movingUnit.gameManager.spriteManager.GetWorldPosition(path[i].x, path[i].y);
                MoveAnimation moveAnimation = (MoveAnimation)Instantiate(animation);
                moveAnimation.SetParameters(movingUnit.gameManager, movingUnit.transform.position, newPosition, new Vector2Int(path[i].x, path[i].y));
                movingUnit.MovePositions(movingUnit.transform.position, newPosition);

            }
            int actionPointsUsed = 0;
            for (int i = 0; i < amountMoved; i++)
            {
                actionPointsUsed += this.intialActionPointUsage + this.actionPointGrowth * movingUnit.amountMoveUsedDuringRound;
                movingUnit.amountMoveUsedDuringRound += 1;
            }
            movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
            movingUnit.UseActionPoints(actionPointsUsed);
        }
        else
        {
            movingUnit.UseActionPoints(0);
            movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
        }
    }

    public void AnotherActionMove(List<Vector2Int> path, int amountMoved, Unit movingUnit, bool onlyMoved)
    {
        Debug.Log("Another Action MOve");
        if(path.Count == 0 || (path.Count == 1 && 
            movingUnit.gameManager.spriteManager.GetWorldPosition(path[0].x, path[0].y) == movingUnit.transform.position))
        {
            return;
        }
        DijkstraMap map = movingUnit.gameManager.map;
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 newPosition = movingUnit.gameManager.spriteManager.GetWorldPosition(path[i].x, path[i].y);
            MoveAnimation moveAnimation = (MoveAnimation)Instantiate(animation);
            moveAnimation.SetParameters(movingUnit.gameManager, movingUnit.transform.position, newPosition, new Vector2Int(path[i].x, path[i].y));
            movingUnit.MovePositions(movingUnit.transform.position, newPosition);
        }
        int actionPointsUsed = 0;
        for (int i = 0; i < amountMoved; i++)
        {
            actionPointsUsed += this.intialActionPointUsage + this.actionPointGrowth * movingUnit.amountMoveUsedDuringRound;
            movingUnit.amountMoveUsedDuringRound += 1;
        }
        if(onlyMoved)
        {
            movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
        }
        movingUnit.UseActionPoints(actionPointsUsed, onlyMoved);
    }
}
