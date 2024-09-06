using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatMapGenerator : MonoBehaviour
{
    public CombatGameManager gameManager;
    public MapTerrain mapTerrain;
    public MissionType missionType;
    public MissionUnitPlacementName missionUnitPlacementName;
    public FactionName missionProviderFaction;
    public FactionName missionTargetFaction;

    //Additional Faction is for third enemy Faction in Intervene missions or a possible additional ally faction in other missions (not sure about last one)
    public FactionName missionAdditionalFaction;
    public List<UnitSuperClass> enemyUnits1FrontLine;
    public List<UnitSuperClass> enemyUnits2FrontLine;
    public List<UnitSuperClass> allyUnitsFrontLine;
    public List<UnitSuperClass> enemyUnits1BackLine;
    public List<UnitSuperClass> enemyUnits2BackLine;
    public List<UnitSuperClass> allyUnitsBackLine;

    public List<UnitSuperClass> playerFrontLine;
    public List<UnitSuperClass> playerBackLine;

    public int mapSize;
    public GridHex<GridPosition> map;
    public void InitializeCombatMapGenerator(GridHex<GridPosition> map, int mapSize, MissionType missionType,
        MissionUnitPlacementName missionPlacementName, MapTerrain mapTerrain,
        FactionName missionProviderFaction, FactionName missionTargetFaction, FactionName missionAdditionalFaction,
        List<UnitSuperClass> playerFrontLine, List<UnitSuperClass> playerBackLine,
        List<UnitSuperClass> enemyUnits1FrontLine, List<UnitSuperClass> enemyUnits1BackLine, 
        List<UnitSuperClass> enemyUnits2FrontLine = null, List<UnitSuperClass> enemyUnits2BackLine = null,
        List<UnitSuperClass> allyUnitsFrontLine = null, List<UnitSuperClass> allyUnitsBackLine = null)
    {
        this.missionType = missionType;
        this.mapTerrain = mapTerrain;
        this.missionUnitPlacementName = missionPlacementName;
        this.missionProviderFaction = missionProviderFaction;
        this.missionTargetFaction = missionTargetFaction;
        this.missionAdditionalFaction = missionAdditionalFaction;
        this.enemyUnits1FrontLine = enemyUnits1FrontLine;
        this.enemyUnits1BackLine = enemyUnits1BackLine;
        this.enemyUnits2FrontLine = enemyUnits2FrontLine;
        this.enemyUnits2BackLine = enemyUnits2BackLine;
        this.allyUnitsFrontLine = allyUnitsFrontLine;
        this.allyUnitsBackLine = allyUnitsBackLine;
        this.playerFrontLine = playerFrontLine;
        this.playerBackLine = playerBackLine;
        this.mapSize = mapSize;
        this.map = map;
    }

    // Make Based On Map For Future Use
    public void GenerateTerrain()
    {
        if(mapTerrain.prefabTerrain != null)
        {
            GeneratePremadeTerrain();
        }
        else
        {
            TerrainType missionTerrain = TerrainType.Grassland;
            switch (missionTerrain)
            {
                case TerrainType.Grassland:
                    break;
                case TerrainType.Forest:
                    break;
                case TerrainType.Hilly:
                    break;
            }
        }
    }

    public void GeneratePremadeTerrain()
    {
        Debug.Log(gameManager.spriteManager.terrainTilePositions.Count + ", " + mapTerrain.prefabTerrain.terrainElevation.Count);
        for (int i = 0; i < mapTerrain.prefabTerrain.terrainElevation.Count; i++)
        {
            Vector3Int terrainHexData = mapTerrain.prefabTerrain.terrainElevation[i];
            gameManager.spriteManager.elevationOfHexes[terrainHexData.x, terrainHexData.y] = terrainHexData.z;
        }

        for (int i = 0; i < mapTerrain.prefabTerrain.terrainElevation.Count; i++)
        {
            Vector3Int terrainHexData = mapTerrain.prefabTerrain.terrainElevation[i];
            gameManager.spriteManager.terrainTilePositions[terrainHexData.z].Add(new Vector2Int(terrainHexData.x, terrainHexData.y));
            gameManager.spriteManager.ChangeElevation(terrainHexData.x, terrainHexData.y, 0);
        }
    }


    public void PlaceUnits()
    {
        switch (missionUnitPlacementName)
        {
            case (MissionUnitPlacementName.LineBattleWestStart):
                //Get half of mapsize minus 1 for base Player Frontline
                int startX = mapSize / 2 - 1;
                // reduce that amount by standard fast speed so player needs more than a normal double move to reach other side
                startX -= 4;
                int startY = mapSize / 2;
                //Place Player Frontline and BackLine
                PlaceUnitsLine(startX, startY, 2, playerFrontLine);
                PlaceUnitsLine(startX - 1, startY, 2, playerBackLine);

                //Place Enemy Units
                startX = mapSize / 2;
                startX += 4;
                PlaceUnitsLine(startX, startY, 2, enemyUnits1FrontLine);
                PlaceUnitsLine(startX + 1, startY, 2, enemyUnits1BackLine);
                break;
        }
    }

    //Places Units in a line and cubeDirectionIndex means a directions down position ex: (0, -1) for a north to south line
    public void PlaceUnitsLine(int startX, int startY, int cubeDirectionIndex, List<UnitSuperClass> unitSuperClasses)
    {
        int lineSize = 0;
        int currentX = startX;
        int currentY = startY;
        Vector3Int currentMapPositionCube = map.OffsetToCube(currentX, currentY);
        for (int i = 0; i < unitSuperClasses.Count; i++)
        {
            if (unitSuperClasses[i].transform.childCount == 0)
            {
                lineSize += 1;
            }
            else
            {
                lineSize += unitSuperClasses[i].transform.childCount;
            }
        }
        // where top of line is based on number of units in line
        for(int i = 0; i < (lineSize - 1) / 2; i++)
        {
            currentMapPositionCube = map.CubeSubtract(currentMapPositionCube, map.CubeDirection(cubeDirectionIndex));
        }

        for (int i = 0; i < unitSuperClasses.Count; i++)
        {
            if (unitSuperClasses[i].transform.childCount == 0)
            {
                Vector2Int currentMapPositionOffset = map.CubeToOffset(currentMapPositionCube);
                Vector3 newUnitPosition = gameManager.spriteManager.GetWorldPosition(new Vector2Int(currentMapPositionOffset.x, currentMapPositionOffset.y));
                unitSuperClasses[i].transform.position = newUnitPosition;
                currentMapPositionCube = map.CubeAdd(currentMapPositionCube, map.cubeDirectionVectors[cubeDirectionIndex]);
            }
            else
            {
                for (int j = 0; j < unitSuperClasses[i].transform.childCount; j++)
                {
                    Vector2Int currentMapPositionOffset = map.CubeToOffset(currentMapPositionCube);
                    Vector3 newUnitPosition = gameManager.spriteManager.GetWorldPosition(new Vector2Int(currentMapPositionOffset.x, currentMapPositionOffset.y));
                    unitSuperClasses[i].transform.GetChild(j).transform.position = newUnitPosition;
                    currentMapPositionCube = map.CubeAdd(currentMapPositionCube, map.cubeDirectionVectors[cubeDirectionIndex]);
                }
            }
        }
    }
}
