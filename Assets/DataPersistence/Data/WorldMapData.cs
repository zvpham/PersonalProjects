using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

[System.Serializable]
public class WorldMapData: IComparable<WorldMapData>   
{
    public long lastUpdated;
    public string selectedProfileId;

    public int saveNumber;
    public bool isAutoSave;
    public bool inCombat;
    public bool newGame = true;

    //Inventory Data
    public List<int> playerItemIndexes;
    public List<int> playerItemQuantities;
    public List<unitLoadoutData> playerHeroes;
    public List<mercenaryData> playerMercenaries;

    // player BattleLine Data
    public List<battleLineData> frontLineData;
    public List<battleLineData> backLineData;


    //mission Data
    // Data Elements
    public MissionType missionType;
    public MissionUnitPlacementName missionUnitPlacementName;
    public FactionName missionProviderFaction;
    public FactionName missionTargetFaction;

    //Additional Faction is for third enemy Faction in Intervene missions or a possible additional ally faction in other missions (not sure about last one)
    public FactionName missionAdditionalFaction;
    public List<battleLineData> enemyUnits1;
    public List<battleLineData> enemyUnits2;
    public List<battleLineData> allyUnits;
    public List<Vector2Int> tileDataPosition;
    public List<int> tileSeedData;

    public int CompareTo(WorldMapData other)
    {
        if (other == null) return 1;

        if(other != null)
        {
            return DateTime.FromBinary(lastUpdated).CompareTo(DateTime.FromBinary(other.lastUpdated));
        }
        else
        {
            throw new ArgumentException("Object is not WorldMapData");
        }
    }
}

[System.Serializable]
public struct unitLoadoutData
{
    //Prefab Data
    public int heroIndex;
    //Job Data
    public int jobIndex;
    public bool isEmpty => jobIndex == -1;
    public List<bool> skillTree1Branch1Unlocks;
    public List<bool> skillTree1Branch2Unlocks;
    public List<bool> skillTree2Branch1Unlocks;
    public List<bool> skillTree2Branch2Unlocks;

    //Item Data
    public int helmetIndex;
    public int armorIndex;
    public int bootsIndex;
    public int mainHandIndex;
    public int offHandIndex;
    public int item1Index;
    public int item2Index;
    public int item3Index;
    public int item4Index;
    public int backUpMainHandIndex;
    public int backUpOffHandIndex;

    public unitLoadoutData(List<bool> skillTree1Branch1Unlocks, List<bool> skillTree1Branch2Unlocks,
     List<bool> skillTree2Branch1Unlocks, List<bool> skillTree2Branch2Unlocks, int helmetIndex, int armorIndex, int bootIndex, int mainHandIndex,
     int offHandIndex, int item1Index, int item2Index, int item3Index, int item4Index, int backUpMainHandIndex, 
     int backUpOffHandIndex, int jobIndex, int heroIndex)
    {
        this.heroIndex = heroIndex;
        this.jobIndex = jobIndex;
        this.skillTree1Branch1Unlocks = skillTree1Branch1Unlocks;
        this.skillTree1Branch2Unlocks = skillTree1Branch2Unlocks;
        this.skillTree2Branch1Unlocks = skillTree2Branch1Unlocks;
        this.skillTree2Branch2Unlocks = skillTree2Branch2Unlocks;

        this.helmetIndex = helmetIndex;
        this.armorIndex = armorIndex;
        this.bootsIndex = bootIndex;
        this.mainHandIndex = mainHandIndex;
        this.offHandIndex = offHandIndex;
        this.item1Index = item1Index;
        this.item2Index = item2Index;
        this.item3Index = item3Index;
        this.item4Index = item4Index;
        this.backUpMainHandIndex = backUpMainHandIndex;
        this.backUpOffHandIndex = backUpOffHandIndex;
    }
}

[System.Serializable]
public struct mercenaryData
{
    public int mercenaryIndex;
    public bool isEmpty => mercenaryIndex == -1;
    public mercenaryData(int mercenaryIndex = -1)
    {
        this.mercenaryIndex = mercenaryIndex;
    }
}

[System.Serializable]
public struct battleLineData
{
    public unitLoadoutData unitData;
    public mercenaryData unitGroupData;

    public battleLineData(unitLoadoutData unitData, mercenaryData unitGroupData)
    {
        this.unitData = unitData;
        this.unitGroupData = unitGroupData;
    }
}
