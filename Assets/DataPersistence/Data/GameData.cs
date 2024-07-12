using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public long lastUpdated;

    public bool inCombat = false;
    public bool newGame = true;

    // Player Specific Data
    public int availableClassLevelUps;

    // Inventory Data
    public List<int> itemQuantities;
    public List<int> itemSOIndexes;

    // Unit Data
    public unitLoadoutData unitPrefabDatas;
}
