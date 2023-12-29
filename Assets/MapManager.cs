using Inventory.Model;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEditor.Progress;

public class MapManager : MonoBehaviour, IDataPersistence
{
    // For Seeing in Editor (Don't Reference these Two)
    public int worldMapWidthInspector;
    public int worldMapHeightInspector;

    public float extraDangerModifier;

    public Vector2Int playerPosition;
    public Vector2Int currentMapPosition;
    public Vector2Int mapMovementDirection = new Vector2Int(0, 0);
    public bool changedMapPosition = false;

    public int maxPreviousMapPositions;
    public List<Vector2Int> previousMapPositions;
    public List<Vector2Int> previousMapPositionsKeys;
    public List<int> previousMapPositionsValues;

    public bool[,] hasVisitedLocation;
    public List<Vector3Int> hasVisitedLocationsaveData;

    public TileData[] templateTiles;
    public TileData[] currentTiles;

    public MainGameManger mainGameManger;

    public GameManager centerGameManager;
    public GameManager BLGameManager;
    public GameManager BGameManager;
    public GameManager BRGameManager;
    public GameManager MLGameManager;
    public GameManager MRGameManager;
    public GameManager TLGameManager;
    public GameManager TGameManager;
    public GameManager TRGameManager;

    //public int initialPlayerPriority;

    public DataPersistenceManager dataPersistenceManager;
    public ResourceManager resourceManager;
    public WorldMap worldMap;
    public MapGenerator mapGenerator;

    WorldMapData worldMapData;

    public static MapManager Instance;

    public void Awake()
    {
        Instance = this;
    }


    public void LoadData(MapData mapData)
    {
        dataPersistenceManager = DataPersistenceManager.Instance;
        extraDangerModifier = mapData.extraDangerModifier;
        playerPosition = mapData.playerPosition;
        currentMapPosition = mapData.currentMapPosition;
        changedMapPosition = mapData.changedMapPosition;
        previousMapPositions = mapData.previousMapPositions;
        previousMapPositionsKeys = mapData.mapPositionsKey;
        previousMapPositionsValues = mapData.mapPositionValue;
        Unit unit;

        //Sets Start position for Player when first starting SaveSlot
        if (previousMapPositionsKeys.Count == 0)
        {
            currentMapPosition = new Vector2Int(0, 0);

            previousMapPositions = new List<Vector2Int>() { new Vector2Int(0, 0) };
            previousMapPositionsKeys = new List<Vector2Int>() { new Vector2Int(0, 0) };
            previousMapPositionsValues = new List<int>() {1};

            GameObject temp = Instantiate(resourceManager.unitPrefabs[0], new Vector3(100, 45, 0), 
                new Quaternion(0, 0, 0, 1f));
            unit = temp.GetComponent<Unit>();
            unit.gameManager = centerGameManager;
            unit.gameManager.units.Insert(0, unit);
            unit.gameManager.mainGameManger.units.Insert(0, unit);

            worldMapData = dataPersistenceManager.worldMapData;
            worldMapData.tileDataPosition = new List<Vector2Int>();
            worldMapData.tileSeedData = new List<int> {};
        }
        else
        {
            // Load Unit Data
            unitPrefabData tempData = mapData.unitPrefabDatas;
            GameObject temp = Instantiate(resourceManager.unitPrefabs[0], new Vector3(playerPosition.x, 
                playerPosition.y, 0), new Quaternion(0, 0, 0, 1f));
            unit = temp.GetComponent<Unit>();
            unit.gameManager = centerGameManager;
            unit.gameManager.units.Insert(0, unit);
            unit.gameManager.mainGameManger.units.Insert(0, unit);
            unit.health = tempData.health;
            unit.actionNamesForCoolDownOnLoad = tempData.actionNames;
            unit.currentCooldownOnLoad = tempData.actionCooldowns;
            unit.forcedMovementPathData = tempData.forcedMovementPathData;
            unit.priority = (int)(mainGameManger.baseTurnTime * 1f);

            // Load Player Specific Data
            Player playerTemp = (Player)unit;
            playerTemp.soulSlotIndexes = mapData.soulSlotIndexes;

            List<SoulItemSO> souls = new List<SoulItemSO>();
            foreach (int soulIndex in mapData.soulIndexes)
            {
                souls.Add(resourceManager.souls[soulIndex]);
            }
            playerTemp.onLoadSouls = souls;

            // Load Inventory Data
            InventorySystem inventory = playerTemp.inventorySystem;
            List<InventoryItem> initialItems = new List<InventoryItem>();
            for (int i = 0; i < mapData.itemQuantities.Count; i++)
            {
                InventoryItem newItem = new InventoryItem();
                newItem.quantity = mapData.itemQuantities[i];
                newItem.item = resourceManager.itemRefrences[mapData.itemSOIndexes[i]];
                initialItems.Add(newItem);
            }
            inventory.initialItems = initialItems;

            // Load Status Data
            for (int i = 0; i < mapData.statusPrefabIndex.Count; i++)
            {
                Status tempStatus = Instantiate(resourceManager.statuses[mapData.statusPrefabIndex[i]]);
                tempStatus.statusPriority = mapData.statusPriority[i];
                tempStatus.currentStatusDuration = mapData.statusDuration[i];
                tempStatus.statusIntData = mapData.statusIntData[i];
                tempStatus.statusStringData = mapData.statusStringData[i];
                tempStatus.statusBoolData = mapData.statusBoolData[i];
                tempStatus.onLoadApply(unit);
            }

            // Loading WorldMapTemplateData
            worldMapData = dataPersistenceManager.GetWorldMapData();
        }

        // Loading MainGameManagerData
        if (!changedMapPosition)
        {
            mainGameManger.least = mapData.least;
            mainGameManger.index = mapData.index;
            mainGameManger.aUnitActed = mapData.aUnitActed;
            mainGameManger.duringTurn = mapData.duringTurn;
            mainGameManger.units[mapData.index].enabled = true;
        }

        if (currentMapPosition.x >= worldMap.width || currentMapPosition.y >= worldMap.height || 
            currentMapPosition.x < 0 || currentMapPosition.y < 0)
        {
            Debug.LogError("out of bounds. attempted to go to map position: " + currentMapPosition);
        }

        //Attempting to get Recent tileData for current Position
        TileData tileData = dataPersistenceManager.GetTileData(dataPersistenceManager.autoSaveID, dataPersistenceManager.playerID, currentMapPosition);

        // Case - Failure
        // Attempts to get frozen Tiledata
        if(tileData == null)
        {
            tileData = dataPersistenceManager.GetTileData(dataPersistenceManager.autoSaveID, currentMapPosition);
        }

        // Case - Failed to get Frozen TileData
        // Use MapGenerator to generate new Tile whether it be by a seed or not
        if (tileData == null)
        {
            //Attempting to get Seed For MapGeneration
            int tileSeedDataIndex =  worldMapData.tileDataPosition.IndexOf(currentMapPosition);

            // Case - New Tile (Tile position not in worldMapData) 
            if(tileSeedDataIndex == -1)
            {
                int tileBaseIndex = worldMap.tilesInspectorUse[currentMapPosition.x + currentMapPosition.y * worldMap.width].z;
                mapGenerator.UsePresetSeed = false;
                //For Prefab Tile Bases
                if (resourceManager.tilesBases[tileBaseIndex].tileType == TileType.Premade)
                {
                    Debug.Log("Generate Prefab");
                    TilePrefabBase tile = (TilePrefabBase)resourceManager.tilesBases[tileBaseIndex];
                    mapGenerator.GeneratePrefabTile(tile, centerGameManager);
                    worldMapData.tileDataPosition.Add(currentMapPosition);
                    worldMapData.tileSeedData.Add(mapGenerator.initialSeed);
                }
                // Generate a NonPrefab Tilebase Map
                else
                {
                    Debug.Log("Generate TileBase");
                    mapGenerator.GenerateTile(resourceManager.tilesBases[tileBaseIndex], centerGameManager, extraDangerModifier);
                    worldMapData.tileDataPosition.Add(currentMapPosition);
                    worldMapData.tileSeedData.Add(mapGenerator.initialSeed);
                }
            }
            // Case - Tile Data Not Generated but seed found in worldMapData
            else
            {
                int tileBaseIndex = worldMap.tilesInspectorUse[currentMapPosition.x + currentMapPosition.y * worldMap.width].z;
                mapGenerator.UsePresetSeed = true;
                mapGenerator.initialSeed = worldMapData.tileSeedData[tileSeedDataIndex];
                //For Prefab Tile Bases
                if (resourceManager.tilesBases[tileBaseIndex].tileType == TileType.Premade)
                {
                    Debug.Log("Generate Seeded Prefab");
                    TilePrefabBase tile = (TilePrefabBase)resourceManager.tilesBases[tileBaseIndex];
                    mapGenerator.GeneratePrefabTile(tile, centerGameManager);
                }
                // Generate a NonPrefab Tilebase Map
                else
                {
                    Debug.Log("Generate Seeded TileBase");
                    mapGenerator.GenerateTile(resourceManager.tilesBases[tileBaseIndex], centerGameManager, extraDangerModifier);
                }
            }
        }
        else
        {
            Debug.Log("Loading TileData for : " + currentMapPosition);
            centerGameManager.LoadData(tileData);
        }

        // Attempting to load Recent Tile Data into Peripheral GameManagers
        for(int i = 0; i < previousMapPositionsKeys.Count; i++)
        {
            tileData = dataPersistenceManager.GetTileData(dataPersistenceManager.autoSaveID, dataPersistenceManager.playerID, previousMapPositionsKeys[i]);
            Vector2Int relativePosition = previousMapPositionsKeys[i] - currentMapPosition;
            if(tileData == null)
            {
                continue;
            }

            if(relativePosition == new Vector2Int(-1, -1))
            {
                BLGameManager.LoadData(tileData);
            }
            else if(relativePosition == new Vector2Int(0, -1))
            {
                BGameManager.LoadData(tileData);
            }
            else if (relativePosition == new Vector2Int(1, -1))
            {
                BRGameManager.LoadData(tileData);
            }
            else if (relativePosition == new Vector2Int(-1, 0))
            {
                MLGameManager.LoadData(tileData);
            }
            else if (relativePosition == new Vector2Int(1, 0))
            {
                MRGameManager.LoadData(tileData);
            }
            else if (relativePosition == new Vector2Int(-1, 1))
            {
                TLGameManager.LoadData(tileData);
            }
            else if (relativePosition == new Vector2Int(0, 1))
            {
                TGameManager.LoadData(tileData);
            }
            else if (relativePosition == new Vector2Int(1, 1))
            {
                TRGameManager.LoadData(tileData);
            }

        }
        unit.gameManager = centerGameManager;
        changedMapPosition = false;
    }

    public void SaveData(MapData mapData)
    {
        dataPersistenceManager.SaveWorldMapData();

        mapData.extraDangerModifier = extraDangerModifier;
        if(changedMapPosition)
        {
            mapData.playerPosition = playerPosition;
        }
        else
        {
            Vector3 newPlayerPosition = mainGameManger.units[0].transform.position;
            mapData.playerPosition = new Vector2Int((int)newPlayerPosition.x, (int)newPlayerPosition.y);
            mapData.least = mainGameManger.least;
            mapData.index = mainGameManger.index;
            mapData.aUnitActed = mainGameManger.aUnitActed;
            mapData.duringTurn = mainGameManger.duringTurn;
        }
        mapData.currentMapPosition = currentMapPosition;
        mapData.changedMapPosition = changedMapPosition;
        mapData.previousMapPositions = previousMapPositions;
        mapData.mapPositionsKey = previousMapPositionsKeys;
        mapData.mapPositionValue = previousMapPositionsValues;

        // Unit data
        Unit unit = centerGameManager.units[0];
        List<int> actionCooldowns = new List<int>();
        List<ActionName> actionNames = new List<ActionName>();
        foreach (Action action in unit.actions)
        {
            actionCooldowns.Add(action.currentCooldown);
            actionNames.Add(action.actionName);
        }
        unitPrefabData tempUnitData = new unitPrefabData(unit.gameObject.transform.position, unit.priority,
            unit.unitResourceManagerIndex, unit.health, actionCooldowns, actionNames, unit.forcedMovementPathData);
        
        mapData.unitPrefabDatas = tempUnitData;

        // Player Specific Data
        Player player = (Player)centerGameManager.units[0];
        List<int> tempOnLoadSouls = new List<int>();
        foreach (SoulItemSO soul in player.onLoadSouls)
        {
            tempOnLoadSouls.Add(resourceManager.souls.IndexOf(soul));
        }
        mapData.soulSlotIndexes = player.soulSlotIndexes;
        mapData.soulIndexes = tempOnLoadSouls;

        // Inventory Data
        List<int> itemQuantities = new List<int>();
        List<int> itemSOIndexes = new List<int>();
        InventorySO inventory = player.inventorySystem.inventoryData;
        for (int i = 0; i < inventory.inventoryItems.Count; i++)
        {
            itemQuantities.Add(inventory.inventoryItems[i].quantity);
            itemSOIndexes.Add(resourceManager.itemRefrences.IndexOf(inventory.inventoryItems[i].item));
        }

        mapData.itemQuantities = itemQuantities;
        mapData.itemSOIndexes = itemSOIndexes;

        // Status Data
        List<int> statusIndexList = new List<int>();
        List<int> indexOfUnitThatHasStatus = new List<int>();
        List<int> statusIntData = new List<int>();
        List<string> statusStringData = new List<string>();
        List<bool> statusBoolData = new List<bool>();
        List<int> statusPriorities = new List<int>();
        List<int> statusDurations = new List<int>();
        for(int i = 0; i < centerGameManager.allStatuses.Count; i++)
        {
            Status status = centerGameManager.allStatuses[i];
            if (status.targetUnit == unit)
            {
                statusIndexList.Add(status.statusPrefabIndex);
                indexOfUnitThatHasStatus.Add(centerGameManager.units.IndexOf(status.targetUnit));
                statusIntData.Add(status.statusIntData);
                statusStringData.Add(status.statusStringData);
                statusBoolData.Add(status.statusBoolData);
                statusPriorities.Add(status.statusPriority);
                statusDurations.Add(status.currentStatusDuration);
            }

        }
        mapData.statusPrefabIndex = statusIndexList;
        mapData.indexOfUnitThatHasStatus = indexOfUnitThatHasStatus;
        mapData.statusPriority = statusPriorities;
        mapData.statusDuration = statusDurations;
        mapData.statusIntData = statusIntData;
        mapData.statusStringData = statusStringData;
        mapData.statusBoolData = statusBoolData;

        // Reversing currentMapPosition Transformation applied during AttemptToMoveMapPosition if it succeeded
        Debug.Log(currentMapPosition + ", " + mapMovementDirection);
        currentMapPosition -= mapMovementDirection;


        for(int i = 0; i < mainGameManger.activeGameManagers.Count; i++)
        {
            TileData tileData = new TileData();
            mainGameManger.activeGameManagers[i].SaveData(tileData);
            dataPersistenceManager.SaveTileData(tileData, dataPersistenceManager.autoSaveID, dataPersistenceManager.playerID, 
                currentMapPosition + mainGameManger.activeGameManagers[i].gameManagerDirection);
            Debug.Log(mainGameManger.activeGameManagers[i].name + ". "  + currentMapPosition + mainGameManger.activeGameManagers[i].gameManagerDirection);
        }
    }

    public void FreezeZone(Vector2Int frozenPosition, Vector2Int relativeGameManagerDirection, bool isMovingPosition = true)
    {
        Debug.Log("Freezing Zone: " + frozenPosition);
        TileData tileData = new TileData();
        GameManager frozenGameManager;
        if (relativeGameManagerDirection == new Vector2Int(-1, -1))
        {
            frozenGameManager = BLGameManager;
        }
        else if (relativeGameManagerDirection == new Vector2Int(0, -1))
        {
            frozenGameManager = BGameManager;
        }
        else if (relativeGameManagerDirection == new Vector2Int(1, -1))
        {
            frozenGameManager = BRGameManager;
        }
        else if (relativeGameManagerDirection == new Vector2Int(-1, 0))
        {
            frozenGameManager = MLGameManager;
        }
        else if (relativeGameManagerDirection == new Vector2Int(1, 0))
        {
            frozenGameManager = MRGameManager;
        }
        else if (relativeGameManagerDirection == new Vector2Int(-1, 1))
        {
            frozenGameManager = TLGameManager;
        }
        else if (relativeGameManagerDirection == new Vector2Int(0, 1))
        {
            frozenGameManager = TGameManager;
        }
        else if (relativeGameManagerDirection == new Vector2Int(1, 1))
        {
            frozenGameManager = TRGameManager;
        }
        else
        {
            frozenGameManager = null;
        }

        if(frozenGameManager == null)
        {
            Debug.LogError("Can't Find GameManager to freeze");
        }
        else
        {
            frozenGameManager.FreezeTile();
            frozenGameManager.SaveData(tileData, true);
            dataPersistenceManager.SaveTileData(tileData, dataPersistenceManager.autoSaveID, frozenPosition);
            dataPersistenceManager.DeleteTileData(frozenPosition);
            if(isMovingPosition)
            {
                return;
            }
            mainGameManger.FreezeGameManager(relativeGameManagerDirection);
        }
    }

    public void AttemptToMoveMapPosition(Vector2Int direction, Vector2Int newPlayerPosition)
    {
        mapMovementDirection = new Vector2Int(0, 0);
        currentMapPosition += direction;
        if(currentMapPosition.x >= worldMap.width || currentMapPosition.x < 0
            || currentMapPosition.y >= worldMap.height || currentMapPosition.y < 0)
        {
            Debug.Log("Tried to move out of WorldMapBounds to position: " + 
                currentMapPosition.x + " , " + currentMapPosition.y);
            currentMapPosition -= direction;
            return;
        }

        currentMapPosition -= direction;
        UpdatePreviousPositions();
        currentMapPosition += direction;

        Vector2 newDirection = direction;
        for(int i = 0; i < mainGameManger.activeGameManagers.Count; i++)
        {
            Vector2 gameManagerDirection = mainGameManger.activeGameManagers[i].gameManagerDirection;
            if(Vector2.Dot(gameManagerDirection, newDirection) == -1f)
            {
                FreezeZone(currentMapPosition - direction, mainGameManger.activeGameManagers[i].gameManagerDirection);
            }
        }

        playerPosition = newPlayerPosition - new Vector2Int(mapGenerator.mapWidth * direction.x, 
            mapGenerator.mapHeight * direction.y);
        changedMapPosition = true;
        mapMovementDirection = direction;
        dataPersistenceManager.SaveGame(dataPersistenceManager.autoSaveID, dataPersistenceManager.playerID);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UpdatePreviousPositions()
    {
        previousMapPositions.Add(currentMapPosition);

        int newPositionIndex = previousMapPositionsKeys.IndexOf(currentMapPosition);
        if (newPositionIndex == -1)
        {
            previousMapPositionsKeys.Add(currentMapPosition);
            previousMapPositionsValues.Add(1);
        }
        else
        {
            previousMapPositionsValues[newPositionIndex] += 1;
        }

        if (previousMapPositions.Count > maxPreviousMapPositions)
        {
            Vector2Int oldPosition = previousMapPositions[0];
            previousMapPositions.RemoveAt(0);
            int oldPositionIndex = previousMapPositionsKeys.IndexOf(oldPosition);
            previousMapPositionsValues[oldPositionIndex] -= 1;
            if (previousMapPositionsValues[oldPositionIndex] <= 0)
            {
                previousMapPositionsKeys.RemoveAt(oldPositionIndex);
                previousMapPositionsValues.RemoveAt(oldPositionIndex);
                FreezeZone(oldPosition, oldPosition - currentMapPosition, false);
            }
        }
    }
}
