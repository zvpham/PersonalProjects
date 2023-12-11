using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData
{
    public long lastUpdated;

    public float extraDangerModifier;
    public Vector2Int playerPosition;
    public Vector2Int currentMapPosition;
    public bool changedMapPosition;
    public List<Vector2Int> previousMapPositions;
    public List<Vector2Int> mapPositionsKey;
    public List<int> mapPositionValue;
    public List<Vector3Int> hasVisitedLocations;
}
