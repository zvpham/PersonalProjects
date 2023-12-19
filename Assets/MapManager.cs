using Inventory.Model;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour, IDataPersistence
{
    // For Seeing in Editor (Don't Reference these Two)
    public int worldMapWidthInspector;
    public int worldMapHeightInspector;

    public float extraDangerModifier;

    public Vector2Int playerPosition;
    public Vector2Int currentMapPosition;
    public bool changedMapPosition = false;

    public List<Vector2Int> previousMapPositions;
    public List<Vector2Int> previousMapPositionsKeys;
    public List<int> previousMapPositionsValues;

    public bool[,] hasVisitedLocation;
    public List<Vector3Int> hasVisitedLocationsaveData;

    public TileData[] templateTiles;
    public TileData[] currentTiles;

    public GameManager centerGameManager;
    public GameManager BLGameManager;
    public GameManager BGameManager;
    public GameManager BRGameManager;
    public GameManager MLGameManager;
    public GameManager MRGameManager;
    public GameManager TLGameManager;
    public GameManager TGameManager;
    public GameManager TRGameManager;


    public DataPersistenceManager dataPersistenceManager;
    public ResourceManager resourceManager;
    public WorldMap worldMap;
    public MapGenerator mapGenerator;

    public static MapManager Instance;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Found more than ONe Data Persistence Manager in the Scence");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        hasVisitedLocation = new bool[worldMap.width, worldMap.height];
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
        hasVisitedLocationsaveData = mapData.hasVisitedLocations;

        //Sets Start position for Player when first starting SaveSlot
        if (currentMapPosition == null)
        {
            currentMapPosition = new Vector2Int(0, 0);

            previousMapPositions = new List<Vector2Int>() { new Vector2Int(0, 0) };
            previousMapPositionsKeys = new List<Vector2Int>() { new Vector2Int(0, 0) };
            previousMapPositionsValues = new List<int>() {1};

            hasVisitedLocationsaveData = new List<Vector3Int>();
            for(int i = 0; i < worldMap.height; i++) 
            {
                for(int j = 0; j < worldMap.width; j++)
                {
                    hasVisitedLocationsaveData.Add(new Vector3Int(j, i, 0));
                }
            }
        }

        if (currentMapPosition.x >= worldMap.width || currentMapPosition.y >= worldMap.height || currentMapPosition.x < 0 || currentMapPosition.y < 0)
        {
            Debug.LogError("out of bounds. attempted to go to map position: " + currentMapPosition);
        }

        //Attempting to get tileData for current Position
        TileData tileData = dataPersistenceManager.GetTileData(currentMapPosition);
        if (tileData == null)
        {
            int tileBaseIndex = worldMap.tilesInspectorUse[currentMapPosition.x + currentMapPosition.y * worldMap.width].z;
            //For Prefab Tile Bases
            if(resourceManager.tilesBases[tileBaseIndex].tileType == TileType.Premade)
            {
                Debug.Log("load prefab");
                TilePrefabBase tile =  (TilePrefabBase)resourceManager.tilesBases[tileBaseIndex];
                if (tile.mapTiles != null)
                {

                }

                if(tile.units != null)
                {

                }
            }
            // Generate a NonPrefab Tilebase Map
            else
            {
                Debug.Log("Generate TileBase");
                mapGenerator.GenerateTile(resourceManager.tilesBases[tileBaseIndex], centerGameManager, extraDangerModifier);
            }
        }
        else
        {
            centerGameManager.LoadData(tileData);
        }
        Vector2Int test = new Vector2Int(2, 2);

        for(int i = 0; i < previousMapPositionsKeys.Count; i++)
        {
            tileData = dataPersistenceManager.GetTileData(previousMapPositions[i]);
            Vector2Int relativePosition = previousMapPositionsKeys[i] - currentMapPosition;
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

        changedMapPosition = false;
    }

    public void SaveData(MapData mapData)
    {
        mapData.extraDangerModifier = extraDangerModifier;
        mapData.playerPosition = playerPosition;
        mapData.currentMapPosition = currentMapPosition;
        mapData.changedMapPosition = changedMapPosition;
        mapData.previousMapPositions = previousMapPositions;
        mapData.mapPositionsKey = previousMapPositionsKeys;
        mapData.mapPositionValue = previousMapPositionsValues;
        mapData.hasVisitedLocations = hasVisitedLocationsaveData;

        // Unit data
        mapData.priority = centerGameManager.priority[0];
        Unit unit = centerGameManager.units[0];
        List<int> actionCooldowns = new List<int>();
        List<ActionName> actionNames = new List<ActionName>();
        foreach (Action action in unit.actions)
        {
            actionCooldowns.Add(action.currentCooldown);
            actionNames.Add(action.actionName);
        }
        unitPrefabData tempUnitData = new unitPrefabData(unit.gameObject.transform.position, unit.unitResourceManagerIndex, unit.health,
            actionCooldowns, actionNames, unit.forcedMovementPathData);
        
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
                statusPriorities.Add(centerGameManager.statusPriority[i]);
                statusDurations.Add(centerGameManager.statusDuration[i]);
            }

        }
        mapData.statusPrefabIndex = statusIndexList;
        mapData.indexOfUnitThatHasStatus = indexOfUnitThatHasStatus;
        mapData.statusPriority = statusPriorities;
        mapData.statusDuration = statusDurations;
        mapData.statusIntData = statusIntData;
        mapData.statusStringData = statusStringData;
        mapData.statusBoolData = statusBoolData;
    }

    public void FreezeZone(int x, int y)
    {
            
    }

    public void MoveCurrentMapPosition(Vector2Int direction)
    {
        currentMapPosition += direction;
        if(currentMapPosition.x >= worldMap.width || currentMapPosition.x < 0
            || currentMapPosition.y >= worldMap.height || currentMapPosition.y < 0)
        {
            Debug.LogError("Tried to move out of WorldMapBounds to position: " + currentMapPosition.x + " , " + currentMapPosition.y);
            currentMapPosition -= direction;
            return;
        }
        changedMapPosition = true;
        dataPersistenceManager.SaveGame(dataPersistenceManager.autoSaveID, dataPersistenceManager.playerID);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    
}
