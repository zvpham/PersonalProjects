using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionData
{
    public Action action;
    public Unit actingUnit;
    public List<Unit> affectedUnits = new List<Unit>();
    public Vector2Int originLocation;
    public Vector2Int targetLocation;
    public List<Vector2Int> path = new List<Vector2Int>();
    public int intReturnData;
}
