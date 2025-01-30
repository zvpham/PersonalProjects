using System.Collections.Generic;
using UnityEngine;

public class AIActionData
{
    public Unit unit;
    public Vector2Int originalPosition;
    public List<Vector2Int> enemyUnits;
    public List<Vector2Int> enemyTeamStartingPositions;
    public AITurnStates AIState;
    public bool inMelee;

    // Movementdata
    public bool canMove;
    public int[,] movementData;
    public List<Action>[,] movementActions;
    public List<Vector2Int>[,] startPositions;
    public bool[,] ignorePassiveArea;

    public int expectedCurrentActionPoints = 0; //  update after every Action
    public int expectedInitialMoveSpeed = 0; // Update After every action that Changes moveSpeed

    // Calculated Action Info
    public Action action;
    public Vector2Int desiredEndPosition;
    public Vector2Int desiredTargetPositionStart;
    public Vector2Int desiredTargetPositionEnd;
    
    public int ModifyActionValue(AIActionData AiActionData, Vector2Int expectedEndPosition, Action action, int actionValue)
    {
        Debug.Log("modify Action Value Posiotn: " + expectedEndPosition);
        Debug.Log("test: " + AiActionData.movementData.GetLength(0) + ", " + AiActionData.movementData.GetLength(1));
        int amountOfActionsUsed = AiActionData.movementData[expectedEndPosition.x, expectedEndPosition.y];      
        int actionIndex = action.GetActionIndex(AiActionData.unit);
        amountOfActionsUsed += action.intialActionPointUsage + action.actionPointGrowth * AiActionData.unit.actions[actionIndex].amountUsedDuringRound;
        return actionValue = actionValue / amountOfActionsUsed;
    }

}
