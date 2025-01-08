using System.Collections.Generic;
using UnityEngine;

public class AIActionData
{
    public Unit unit;
    public Vector2Int originalPosition;
    public List<Vector2Int> enemyUnits;
    public AITurnStates AIState;
    public bool inMelee;

    // Movementdata
    public bool wantsToMove;
    public int[,] movementData;
    public List<Action>[,] movementActions;
    public List<Vector2Int>[,] startPositions;

    public int expectedCurrentActionPoints = 0; //  update after every Action
    public int expectedInitialMoveSpeed = 0; // Update After every action that Changes moveSpeed

    // Calculated Action Info
    public Action action;
    public Vector2Int desiredEndPosition;
    public Vector2Int desiredTargetPositionStart;
    public Vector2Int desiredTargetPositionEnd;
}
