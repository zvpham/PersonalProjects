using System.Collections.Generic;
using UnityEngine;

public class AIActionData
{
    public Unit unit;
    public Vector2Int originalPosition;
    public Vector2Int desiredEndPosition;
    public List<Vector2Int> enemyUnits;
    public AITurnStates AIState;
    public bool inMelee;
    public bool wantsToMove;
    public List<int[,]> movementData;
}
