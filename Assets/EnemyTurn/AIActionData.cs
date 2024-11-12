using System.Collections.Generic;
using UnityEngine;

public class AIActionData
{
    public Unit unit;
    public Vector2Int originalPosition;
    public bool wantsToMove;
    public List<Vector2Int> enemyUnits;
}
