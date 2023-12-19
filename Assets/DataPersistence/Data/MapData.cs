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

    // Player Data

    // Unit Data
    public unitPrefabData unitPrefabDatas;
    public int priority;

    // Player Specific Data
    public List<int> soulSlotIndexes;
    public List<int> soulIndexes;

    // Status Data
    public List<int> statusPriority;
    public List<int> statusDuration;
    public List<int> statusPrefabIndex;
    public List<int> indexOfUnitThatHasStatus;
    public List<int> statusIntData;
    public List<string> statusStringData;
    public List<bool> statusBoolData;
}
