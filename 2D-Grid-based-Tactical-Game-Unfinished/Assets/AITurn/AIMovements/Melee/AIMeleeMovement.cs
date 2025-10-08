using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMeleeMovement
{
    // This is for when there are spotted Enemy Units
    public static void MeleeRangeOneGetCloserToEnemyUnits(AIActionData AiActionData)
    {
        Unit movingUnit = AiActionData.unit;
        CombatGameManager gameManager = movingUnit.gameManager;
        AiActionData.badWalkInPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        List<List<PassiveEffectArea>> classifiedPassiveEffectArea = movingUnit.CalculuatePassiveAreas();

        for (int i = 0; i < classifiedPassiveEffectArea[1].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[1][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[1][i].passiveLocations[j];
                AiActionData.badWalkInPassivesValues[passiveLocation.x, passiveLocation.y] = true;
            }
        }

        List<int> moveActionIndexes = new List<int>();
        for (int i = 0; i < movingUnit.actions.Count; i++)
        {
            if (movingUnit.actions[i].action.actionTypes.Contains(ActionType.Movement))
            {
                moveActionIndexes.Add(i);
            }
        }

        int lowestMoveValue = int.MaxValue;
        int lowestMoveActionIndex = -1;
        List<Action> moveActionsCalculated = new List<Action>();
        List<Vector2Int> unitStartPositions = new List<Vector2Int>();
        bool ignorePassives = false;

        for (int i = 0; i < moveActionIndexes.Count; i++)
        {
            Action currentMoveAction = movingUnit.actions[moveActionIndexes[i]].action;

            Tuple<int, Vector2Int, List<Action>, List<Vector2Int>> moveValue = currentMoveAction.MoveUnitToEnemyUnits(AiActionData);
            if (moveValue.Item1 < lowestMoveValue)
            {
                Debug.Log("Current Move Action: " + currentMoveAction + ", false" + ", " + moveValue.Item1 + ", " + moveValue.Item2);
                lowestMoveActionIndex = i;
                lowestMoveValue = moveValue.Item1;
                AiActionData.desiredEndPosition = moveValue.Item2;
                moveActionsCalculated = moveValue.Item3;
                unitStartPositions = moveValue.Item4;
            }
        }

        for (int i = 0; i < moveActionIndexes.Count; i++)
        {
            Action currentMoveAction = movingUnit.actions[moveActionIndexes[i]].action;

            Tuple<int, Vector2Int, List<Action>, List<Vector2Int>> moveValue = currentMoveAction.MoveUnitToEnemyUnits(AiActionData, true);
            if (moveValue.Item1 < lowestMoveValue)
            {
                Debug.Log("Current Move Action: " + currentMoveAction + ", true" + ", " + moveValue.Item1 + ", " + moveValue.Item2);
                ignorePassives = true;
                lowestMoveActionIndex = i;
                lowestMoveValue = moveValue.Item1;
                AiActionData.desiredEndPosition = moveValue.Item2;
                moveActionsCalculated = moveValue.Item3;
                unitStartPositions = moveValue.Item4;
            }
        }
        Vector2Int endPosition = AiActionData.desiredEndPosition;
        AiActionData.movementActions[endPosition.x, endPosition.y] = moveActionsCalculated;
        AiActionData.startPositions[endPosition.x, endPosition.y] = unitStartPositions;
        AiActionData.ignorePassiveArea[endPosition.x, endPosition.y] = ignorePassives;

        if (lowestMoveActionIndex == -1)
        {
            movingUnit.EndTurnAction();
        }
        else
        {
            for (int i = 0; i < moveActionsCalculated.Count; i++)
            {
                moveActionsCalculated[i].AIUseAction(AiActionData);
            }
            gameManager.PlayActions();
        }
    }

    // Get Closer to enemy Units while priortising
    public static void MeleeRangeOneSkirmish(AIActionData AiActionData)
    {
        Unit movingUnit = AiActionData.unit;
        CombatGameManager gameManager = movingUnit.gameManager;
        DijkstraMap map = gameManager.map;
        Vector2Int unitPosition = new Vector2Int(movingUnit.x, movingUnit.y);
        int currMaxSpeed = movingUnit.currentMoveSpeed;
        if(!movingUnit.unitMovedThisTurn)
        {
            currMaxSpeed += movingUnit.moveSpeedPerMoveAction;
        }

        AiActionData.badWalkInPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        List<List<PassiveEffectArea>> classifiedPassiveEffectArea = movingUnit.CalculuatePassiveAreas();

        for (int i = 0; i < classifiedPassiveEffectArea[1].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[1][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[1][i].passiveLocations[j];
                AiActionData.badWalkInPassivesValues[passiveLocation.x, passiveLocation.y] = true;
            }
        }

        List<Vector2Int> targetPositions;
        if (AiActionData.enemyUnits.Count == 0)
        {
            targetPositions = AiActionData.enemyTeamStartingPositions;
        }
        else
        {
            targetPositions = AiActionData.enemyUnits;
        }

        gameManager.map.ResetMap(true, false);
        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
        List<DijkstraMapNode> nodesInMoverange =  gameManager.map.GetNodesInMovementRange(movingUnit.x, movingUnit.y, currMaxSpeed, movingUnit.moveModifier, gameManager);
        Debug.Log("Nodes In Range: " +  nodesInMoverange.Count);
        map.ResetMap(false);
        map.SetGoalsNew(new List<Vector2Int>() { unitPosition }, gameManager, AiActionData.unit.moveModifier);
        int[,] gridOfMoveableHexes = map.GetGridValues();
        movingUnit.unitMovedThisTurn = true;
        gameManager.map.ResetMap(true);

        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
        gameManager.map.SetGoalsNew(targetPositions, gameManager, movingUnit.moveModifier);
        if (AiActionData.futureFriendlyUnits.Count == 0)
        {
            if (!SkirmishHelpFuturePositionsAbsent(movingUnit, currMaxSpeed, gameManager, nodesInMoverange, AiActionData, gridOfMoveableHexes))
            {
                movingUnit.EndTurnAction();
            }
        }
        else
        {
            if (!SkirmishHelpFuturePositionsPresent(movingUnit, currMaxSpeed, gameManager, AiActionData, gridOfMoveableHexes))
            {
                if (!SkirmishHelpFuturePositionsAbsent(movingUnit, currMaxSpeed, gameManager, nodesInMoverange, AiActionData, gridOfMoveableHexes))
                {
                    movingUnit.EndTurnAction();
                }
            }
        }
    }

    public static bool SkirmishHelpFuturePositionsAbsent(Unit movingUnit, int currMaxSpeed,  CombatGameManager gameManager, List<DijkstraMapNode> nodesInrange, AIActionData AiActionData, int[,] gridOfMoveableHexes)
    {
        bool foundPosition = false;
        int highestValue = int.MinValue;
        int originalUnitNodeValue = gameManager.map.getGrid().GetGridObject(new Vector2Int(movingUnit.x, movingUnit.y)).value;
        DijkstraMapNode targetDestination = null;
        GridHex<GridPosition> unitGrid = gameManager.grid;
        for (int i = 0; i < nodesInrange.Count; i++)
        {
            DijkstraMapNode currentNode = nodesInrange[i];
            int modifiedValue = originalUnitNodeValue - currentNode.value - currMaxSpeed;

            //Debug.Log("test 10: " + currentNode.x + ", " + currentNode.y + ", " + gridOfMoveableHexes[currentNode.x, currentNode.y]);
            if (modifiedValue > highestValue && (unitGrid.GetGridObject(currentNode.x, currentNode.y).unit == null ||
                unitGrid.GetGridObject(currentNode.x, currentNode.y).unit == movingUnit) && gridOfMoveableHexes[currentNode.x, currentNode.y] <= currMaxSpeed)
            {
                targetDestination = currentNode;
                highestValue = modifiedValue;
            }
        }

        if (targetDestination != null)
        {
            AiActionData.desiredEndPosition = new Vector2Int(targetDestination.x, targetDestination.y);
            List<Action> movementActions = AiActionData.movementActions[targetDestination.x, targetDestination.y];
            Debug.Log("targetDestination: " + targetDestination.x + ", " + targetDestination.y + ", " + movementActions.Count);
            for (int i = 0; i < movementActions.Count; i++)
            {
                movementActions[i].AIUseAction(AiActionData);
            }
            gameManager.PlayActions();  
            foundPosition = true;
        }
        return foundPosition;
    }

    public static bool SkirmishHelpFuturePositionsPresent(Unit movingUnit, int currMaxSpeed, CombatGameManager gameManager, AIActionData AiActionData, int[,] gridOfMoveableHexes)
    {
        bool foundPosition = false;
        List<Vector2Int> positionsNearUnitsThatHaveMoved = new List<Vector2Int>();
        for (int i = 0; i < AiActionData.futureFriendlyUnits.Count; i++)
        {
            Vector2Int futurePosition = AiActionData.futureFriendlyUnits[i];
            List<Vector2Int> positionsinRing = gameManager.map.getGrid().GetGridPositionsInRing(futurePosition.x, futurePosition.y, 1);
            for (int j = 0; j < positionsinRing.Count; j++)
            {
                if (!positionsNearUnitsThatHaveMoved.Contains(positionsinRing[j]))
                {
                    positionsNearUnitsThatHaveMoved.Add(positionsinRing[j]);
                }
            }
        }
        GridHex<GridPosition> unitGrid = gameManager.grid;
        int highestValue = int.MinValue;
        int originalUnitNodeValue = gameManager.map.getGrid().GetGridObject(new Vector2Int(movingUnit.x, movingUnit.y)).value;
        DijkstraMapNode targetDestination = null;
        GridHex<DijkstraMapNode> grid = gameManager.map.getGrid();
        for (int i = 0; i < positionsNearUnitsThatHaveMoved.Count; i++)
        {
            Vector2Int testPosition = positionsNearUnitsThatHaveMoved[i];
            DijkstraMapNode currentNode = grid.GetGridObject(testPosition);
            int modifiedValue = originalUnitNodeValue - currentNode.value - currMaxSpeed;
            if (modifiedValue > highestValue && (unitGrid.GetGridObject(currentNode.x, currentNode.y).unit == null ||
                unitGrid.GetGridObject(currentNode.x, currentNode.y).unit == movingUnit) && gridOfMoveableHexes[currentNode.x, currentNode.y] <= currMaxSpeed)
            {
                targetDestination = currentNode;
                highestValue = modifiedValue;
            }
        }

        if( targetDestination != null )
        {
            AiActionData.desiredEndPosition = new Vector2Int(targetDestination.x, targetDestination.y);
            List<Action> movementActions = AiActionData.movementActions[targetDestination.x, targetDestination.y];
            for (int i = 0; i < movementActions.Count; i++)
            {
                movementActions[i].AIUseAction(AiActionData);
            }
            gameManager.PlayActions();
            foundPosition = true;
        }
        return foundPosition;
    }
}
