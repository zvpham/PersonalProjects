using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData
{
    public long lastUpdated;

    public Vector2Int currentMapPosition;
    public List<Vector2> previousMapPositions;
    public List<Vector2> mapPositionsKey;
    public List<int> mapPositionValue;
    public bool[,] hasVisitedLocations;
}
