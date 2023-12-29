using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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

    // MainGameManager Data
    public int least;
    public int index;
    public bool aUnitActed = false;
    public int duringTurn;

    // Player Specific Data
    public List<int> soulSlotIndexes;
    public List<int> soulIndexes;

    // Inventory Data
    public List<int> itemQuantities;
    public List<int> itemSOIndexes;

    // Unit Data
    public unitPrefabData unitPrefabDatas;
    public int priority;

    // Status Data
    public List<int> statusPriority;
    public List<int> statusDuration;
    public List<int> statusPrefabIndex;
    public List<int> indexOfUnitThatHasStatus;
    public List<int> statusIntData;
    public List<string> statusStringData;
    public List<bool> statusBoolData;
}
