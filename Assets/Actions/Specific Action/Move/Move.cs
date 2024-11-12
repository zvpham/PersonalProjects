using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Move")]
public class Move : Action
{
    public override int CalculateWeight(AIActionData actionData)
    {
        return 0;
    }

    public override void FindOptimalPosition(AIActionData actionData)
    {
        return;
    }

    public override void SelectAction(Unit self)
    {
        base.SelectAction(self);
        self.gameManager.spriteManager.ActivateMovementTargeting(self, false, self.currentActionsPoints);
        self.gameManager.spriteManager.movementTargeting.OnFoundTarget += FoundTarget;
    }

    public void FoundTarget(List<Vector2Int> path, List<int> indexOfStartingMoves,  Unit movingUnit, bool foundTarget)
    {
        if (foundTarget)
        {
            if (movingUnit.gameManager.spriteManager.GetWorldPosition(path[0].x, path[0].y) == movingUnit.transform.position)
            {
                path.RemoveAt(0);
                for(int i = 1; i < indexOfStartingMoves.Count; i++)
                {
                    indexOfStartingMoves[i] -= 1;
                }
            }
            if (path.Count == 0)
            {
                movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
                movingUnit.gameManager.spriteManager.ActivateMovementTargeting(movingUnit, false, movingUnit.currentActionsPoints);
                movingUnit.gameManager.spriteManager.movementTargeting.OnFoundTarget += FoundTarget;
                return;
            }
            for(int i = 0; i < indexOfStartingMoves.Count; i++)
            {
                ActionData actionData= new ActionData();
                actionData.action = this;
                actionData.actingUnit = movingUnit;
                actionData.originLocation = new Vector2Int(movingUnit.x, movingUnit.y);
                int startPathIndex = -1;
                int endPathIndex = -1;
                if(i ==  indexOfStartingMoves.Count - 1)
                {
                    startPathIndex = indexOfStartingMoves[i];
                    endPathIndex = path.Count;
                    actionData.targetLocation = path[path.Count - 1];
                }
                else
                {
                    startPathIndex = indexOfStartingMoves[i];
                    endPathIndex = indexOfStartingMoves[i + 1];
                    actionData.targetLocation = path[indexOfStartingMoves[i + 1]];
                }

                List<Vector2Int> tempPath =  new List<Vector2Int>();
                for(int j = startPathIndex; j < endPathIndex; j++)
                {
                    tempPath.Add(path[j]);
                }
                actionData.path = tempPath;
                actionData.intReturnData = endPathIndex - startPathIndex;
                movingUnit.gameManager.AddActionToQueue(actionData, false, false);
            }
            movingUnit.gameManager.PlayActions();   
        }
        else
        {
            movingUnit.UseActionPoints(0);
            movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
        }
    }

    public void AnotherActionMove(List<Vector2Int> path, List<int> indexOfStartingMoves, Unit movingUnit, bool onlyMoved)
    {
        Debug.Log("Another Action MOve");
        if (movingUnit.gameManager.spriteManager.GetWorldPosition(path[0].x, path[0].y) == movingUnit.transform.position)
        {
            path.RemoveAt(0);
            for (int i = 1; i < indexOfStartingMoves.Count; i++)
            {
                indexOfStartingMoves[i] -= 1;
            }
        }
        if (path.Count == 0)
        {
            return;
        }
        for (int i = 0; i < indexOfStartingMoves.Count; i++)
        {
            ActionData actionData = new ActionData();
            actionData.action = this;
            actionData.actingUnit = movingUnit;
            actionData.originLocation = new Vector2Int(movingUnit.x, movingUnit.y);
            int startPathIndex = -1;
            int endPathIndex = -1;
            if (i == indexOfStartingMoves.Count - 1)
            {
                startPathIndex = indexOfStartingMoves[i];
                endPathIndex = path.Count;
                actionData.targetLocation = path[path.Count - 1];
            }
            else
            {
                startPathIndex = indexOfStartingMoves[i];
                endPathIndex = indexOfStartingMoves[i + 1];
                actionData.targetLocation = path[indexOfStartingMoves[i + 1]];
            }

            List<Vector2Int> tempPath = new List<Vector2Int>();
            for (int j = startPathIndex; j < endPathIndex; j++)
            {
                tempPath.Add(path[j]);
            }
            actionData.path = tempPath;
            actionData.intReturnData = endPathIndex - startPathIndex;
            movingUnit.gameManager.AddActionToQueue(actionData, false, false);
        }
        movingUnit.gameManager.PlayActions();
    }

    public override void ConfirmAction(ActionData actionData)
    {
        Unit movingUnit = actionData.actingUnit;
        List<Vector2Int> path = new List<Vector2Int>();
        for (int i = 0; i < actionData.intReturnData; i++)
        {
            path.Add(actionData.path[i]);
        }

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 newPosition = movingUnit.gameManager.spriteManager.GetWorldPosition(path[i].x, path[i].y);
            MoveAnimation moveAnimation = (MoveAnimation)Instantiate(animation);
            moveAnimation.SetParameters(movingUnit.gameManager, movingUnit.transform.position, newPosition, new Vector2Int(path[i].x, path[i].y));
            movingUnit.MovePositions(movingUnit.transform.position, newPosition, i == path.Count - 1);

        }
        int actionPointsUsed = this.intialActionPointUsage + this.actionPointGrowth * movingUnit.actions[0].amountUsedDuringRound;
        movingUnit.actions[0].amountUsedDuringRound += 1;
        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
        movingUnit.UseActionPoints(actionPointsUsed);
    }
}
