using System;
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

    public override bool CheckIfActionIsInRange(AIActionData actionData)
    {
        return false;
    }

    public override int[,] GetMovementMap(AIActionData actionData)
    {
        CombatGameManager gameManager = actionData.unit.gameManager;
        Unit movingUnit = actionData.unit;

        List<PassiveEffectArea>[,] passives = new List<PassiveEffectArea>[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < gameManager.mapSize; i++)
        {
            for (int j = 0; j < gameManager.mapSize; j++)
            {
                passives[i, j] = new List<PassiveEffectArea>();
            }
        }

        List<List<PassiveEffectArea>> classifiedPassiveEffectArea = movingUnit.CalculuatePassiveAreas();
        bool[,] unwalkablePassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];

        for (int i = 0; i < classifiedPassiveEffectArea[0].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[0][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[0][i].passiveLocations[j];
                unwalkablePassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[0][i]);
            }
        }

        bool[,] badWalkInPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < classifiedPassiveEffectArea[1].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[1][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[1][i].passiveLocations[j];
                badWalkInPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[1][i]);
            }
        }

        bool[,] goodWalkinPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < classifiedPassiveEffectArea[2].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[2][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[2][i].passiveLocations[j];
                goodWalkinPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[2][i]);
            }
        }
        actionData.unit.gameManager.map.ResetMap(true);
        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
        actionData.unit.gameManager.map.SetGoals(new List<Vector2Int>() { actionData.originalPosition }, gameManager, movingUnit.moveModifier, badWalkInPassivesValues);
        int[,] movementGridValues = actionData.unit.gameManager.map.GetGridValues();

        int amountMoved = 0;
        for (int i = 0; i < movingUnit.actions.Count; i++)
        {
            if (movingUnit.actions[i].action.GetType() == typeof(Move))
            {
                amountMoved = movingUnit.actions[i].amountUsedDuringRound;
            }
        }

        for (int i = 0; i < movementGridValues.GetLength(0); i++)
        {
            for(int j = 0; j < movementGridValues.GetLength(1); j++)
            {
                int actionAmount = (movementGridValues[i, j] + movingUnit.moveSpeed - 1) / movingUnit.moveSpeed;
                int totalActionAmount = 0;
                for(int k = 0;  k < actionAmount; k++)
                {
                    totalActionAmount += this.intialActionPointUsage + (k  + amountMoved) * this.actionPointGrowth;  
                }
                movementGridValues[i, j] = totalActionAmount;
            }
        }
        return movementGridValues;
    }

    public override void SelectAction(Unit self)
    {
        /*
        if (SpecificCheckActionUsable(self) && (this.intialActionPointUsage + actionPointGrowth * self.actions[actionIndex].amountUsedDuringRound
    <= self.currentActionsPoints || self.CanMove()))
        {
            return;
        }
        */
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
            for(int i = 0; i < path.Count; i++)
            {
                ActionData actionData= new ActionData();
                actionData.action = this;
                actionData.actingUnit = movingUnit;
                actionData.originLocation = new Vector2Int(movingUnit.x, movingUnit.y);

                List<Vector2Int> tempPath =  new List<Vector2Int>() { path[i] };
                actionData.path = tempPath;
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
        Vector2Int nextHexPosition = actionData.path[0];
        Vector3 newPosition = movingUnit.gameManager.spriteManager.GetWorldPosition(nextHexPosition.x, nextHexPosition.y);
        if (movingUnit.currentMoveSpeed < movingUnit.gameManager.moveCostMap[nextHexPosition.x, nextHexPosition.y])
        {
            int actionPointsUsed = this.intialActionPointUsage + this.actionPointGrowth * movingUnit.actions[0].amountUsedDuringRound;
            movingUnit.actions[0].amountUsedDuringRound += 1;
            movingUnit.UseActionPoints(actionPointsUsed);
            movingUnit.currentMoveSpeed += movingUnit.moveSpeedPerMoveAction;
        }
        movingUnit.currentMoveSpeed -= movingUnit.gameManager.moveCostMap[nextHexPosition.x, nextHexPosition.y];
        MoveAnimation moveAnimation = (MoveAnimation)Instantiate(animation);
        moveAnimation.SetParameters(movingUnit.gameManager, movingUnit.transform.position, newPosition, 
            new Vector2Int(nextHexPosition.x, nextHexPosition.y));
        movingUnit.MovePositions(movingUnit.transform.position, newPosition, true);

        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
    }
}
