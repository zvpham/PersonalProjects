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
    // [1 Action Moves, 2 Action Moves]
    public List<Vector2Int>[] hexesUnitCanMoveTo;
    public List<Action>[,] movementActions;
    public List<Vector2Int>[,] startPositions;
    public bool[,] ignorePassiveArea;

    public int expectedCurrentActionPoints = 0; //  update after every Action
    public int expectedInitialMoveSpeed = 0; // Update After every action that Changes moveSpeed

    // Calculated Action Info
    public Action action;
    public int highestActionWeight;
    public Vector2Int desiredEndPosition;
    public Vector2Int desiredTargetPositionStart;
    public Vector2Int desiredTargetPositionEnd;
    public int itemIndex;

    public ActionPredictionData prediction;
    public List<List<PassiveEffectArea>> classifiedPassiveEffectArea;
    public bool[,] unwalkablePassivesValues;
    public bool[,] badWalkInPassivesValues;
    public bool[,] goodWalkinPassivesValues;
    public List<PassiveEffectArea>[,] passives;

    public AIActionData()
    {

    }

    public AIActionData(int mapSize)
    {
        movementData = new int[mapSize, mapSize];
        movementActions = new List<Action>[mapSize, mapSize];
        startPositions = new List<Vector2Int>[mapSize, mapSize];
        ignorePassiveArea = new bool[mapSize, mapSize];
        hexesUnitCanMoveTo = new List<Vector2Int>[2];
        hexesUnitCanMoveTo[0] = new List<Vector2Int>();
        hexesUnitCanMoveTo[1] = new List<Vector2Int>();
    }
    public int ModifyActionValue(AIActionData AiActionData, Vector2Int expectedEndPosition, Action action, int actionValue)
    {
        //Debug.Log("modify Action Value Posiotn: " + expectedEndPosition);
        //Debug.Log("test: " + AiActionData.movementData.GetLength(0) + ", " + AiActionData.movementData.GetLength(1));
        int amountOfActionsUsed = AiActionData.movementData[expectedEndPosition.x, expectedEndPosition.y];      
        int actionIndex = action.GetActionIndex(AiActionData.unit);
        amountOfActionsUsed += action.actionPointUsage;
        return actionValue = actionValue / (amountOfActionsUsed + 1); // add + 1 so that position and other modifiers are more valuable than actionpoints
    }

}
