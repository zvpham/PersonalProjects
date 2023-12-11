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

    public GameManager gameManager;
    public DataPersistenceManager dataPersistenceManager;
    public ResourceManager resourceManager;
    public WorldMap worldMap;

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

        TileData tileData = dataPersistenceManager.GetTileData(currentMapPosition);
        if (tileData == null)
        {
            int tileBaseIndex = worldMap.tilesInspectorUse[currentMapPosition.x + currentMapPosition.y * worldMap.width].z;
            if(resourceManager.tilesBases[tileBaseIndex].tileType == TileType.Premade)
            {
                TilePrefabBase tile =  (TilePrefabBase)resourceManager.tilesBases[tileBaseIndex];
                if (tile.mapTiles != null)
                {

                }

                if(tile.units != null)
                {

                }
            }
            else
            {

            }
        }
        else
        {
            gameManager.LoadData(tileData);
        }

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }
                else
                {

                }
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
