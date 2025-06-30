using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIRangedMovement
{
    // This is for when there are spotted Enemy Units
    public static void RangedMoveToOptimalPositionCombat(AIActionData AiActionData)
    {
        Unit currentUnit = AiActionData.unit;
        CombatGameManager gameManager = currentUnit.gameManager;
        int highestActionWeight = -1;
        int actionIndex = -1;
        for (int i = 0; i < currentUnit.actions.Count; i++)
        {
            int testActionWeight = currentUnit.actions[i].action.CalculateEnvironmentWeight(AiActionData);
            if (currentUnit.actions[i].action.actionTypes.Contains(ActionType.Movement))
            {
                continue;
            }

            if (testActionWeight > highestActionWeight)
            {
                actionIndex = i;
                highestActionWeight = testActionWeight;
            }
        }

        Debug.LogWarning("Remove This for final version, remove when you have enemyTeamStartingPositinons Set");
        Vector2Int endHex;
        if (AiActionData.enemyTeamStartingPositions == null || AiActionData.enemyTeamStartingPositions.Count == 0)
        {
            if (AiActionData.enemyUnits.Count == 0)
            {
                Debug.LogError("no EnemynUnits Left");
                return;
            }
            endHex = AiActionData.enemyUnits[0];
        }
        else
        {
            endHex = AiActionData.enemyTeamStartingPositions[0];
        }   

        AiActionData.desiredEndPosition = new Vector2Int(-1, -1);
        Action currentAction = currentUnit.actions[actionIndex].action;
        int lineDisatance = gameManager.grid.OffsetDistance(AiActionData.originalPosition, endHex);
        Vector3Int startCube = gameManager.grid.OffsetToCube(AiActionData.originalPosition);
        Vector3Int endCube = gameManager.grid.OffsetToCube(endHex);

        List<Vector3Int> cubeLine = gameManager.grid.CubeLineDraw(startCube, endCube);
        Vector3Int midPointCube = cubeLine[(lineDisatance) / 2];
        Vector2Int midPointPosition = gameManager.grid.CubeToOffset(midPointCube);
        AiActionData.desiredTargetPositionEnd = midPointPosition;
        Debug.Log("Current Action: " + currentAction);
        currentAction.FindOptimalPosition(AiActionData);
        if (AiActionData.desiredEndPosition == new Vector2Int(-1, -1))
        {
            AiActionData.unit.EndTurnAction();
        }
        else
        {
            int moveActionIndex = AiActionData.unit.GetMoveActionIndex();
            Move moveAction = (Move)AiActionData.unit.actions[moveActionIndex].action;
            Debug.Log("final Position: " + AiActionData.desiredEndPosition);
            moveAction.MoveIsOnlyActionAIAction(AiActionData);
        }
    }

    public static void RangedMoveToOptimalPositionSkirmish(AIActionData AiActionData)
    {
        Unit currentUnit = AiActionData.unit;
        CombatGameManager gameManager = currentUnit.gameManager;
        int highestActionWeight = -1;
        int actionIndex = -1;
        for (int i = 0; i < currentUnit.actions.Count; i++)
        {
            int testActionWeight = currentUnit.actions[i].action.CalculateEnvironmentWeight(AiActionData);
            if (currentUnit.actions[i].action.actionTypes.Contains(ActionType.Movement))
            {
                continue;
            }

            if (testActionWeight > highestActionWeight)
            {
                actionIndex = i;
                highestActionWeight = testActionWeight;
            }
        }

        Debug.LogWarning("Remove This for final version, remove when you have enemyTeamStartingPositinons Set");
        Vector2Int endHex;
        if (AiActionData.enemyTeamStartingPositions == null || AiActionData.enemyTeamStartingPositions.Count == 0)
        {
            if (AiActionData.enemyUnits.Count == 0)
            {
                Debug.LogError("no EnemynUnits Left");
                return;
            }
            endHex = AiActionData.enemyUnits[0];
        }
        else
        {
            endHex = AiActionData.enemyTeamStartingPositions[0];
        }

        AiActionData.desiredEndPosition = new Vector2Int(-1, -1);
        Action currentAction = currentUnit.actions[actionIndex].action;
        int lineDisatance = gameManager.grid.OffsetDistance(AiActionData.originalPosition, endHex);
        Vector3Int startCube = gameManager.grid.OffsetToCube(AiActionData.originalPosition);
        Vector3Int endCube = gameManager.grid.OffsetToCube(endHex);

        List<Vector3Int> cubeLine = gameManager.grid.CubeLineDraw(startCube, endCube);
        Vector3Int midPointCube = cubeLine[(lineDisatance) / 2];
        Vector2Int midPointPosition = gameManager.grid.CubeToOffset(midPointCube);
        AiActionData.desiredTargetPositionEnd = midPointPosition;
        Debug.Log("Current Action: " + currentAction);
        currentAction.FindOptimalPosition(AiActionData);
        if (AiActionData.desiredEndPosition == new Vector2Int(-1, -1))
        {
            AiActionData.unit.EndTurnAction();
        }
        else
        {
            int moveActionIndex = AiActionData.unit.GetMoveActionIndex();
            Move moveAction = (Move)AiActionData.unit.actions[moveActionIndex].action;
            Debug.Log("final Position: " + AiActionData.desiredEndPosition);
            moveAction.MoveIsOnlyActionAIAction(AiActionData);
        }
    }
}
