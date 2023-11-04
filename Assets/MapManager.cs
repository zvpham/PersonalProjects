using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour, IDataPersistence
{
    // For Seeing in Editor (Don't Reference these Two)
    public int worldMapWidthInspector;
    public int worldMapHeightInspector;

    public Vector2Int currentMapPosition;
    public List<Vector2> previousMapPositions;
    public List<Vector2> previousMapPositionsKeys;
    public List<int> previousMapPositionsValues;
    public bool[,] hasVisitedLocation;
    public TileData[] templateTiles;
    public TileData[] currentTiles;

    public GameManager gameManager;
    public DataPersistenceManager dataPersistenceManager;

    public static int worldMapHeight;
    public static int worldMapWidth;
    public static TileBase[,] worldMap;
    public static MapManager Instance;
    

    public static void ChangeWorldSize(int width, int height)
    {
        worldMapWidth = width;
        worldMapHeight = height;
    }

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Found more than ONe Action Bar in the Scence");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        worldMapWidthInspector = worldMapWidth;
        worldMapHeightInspector = worldMapHeight;
        hasVisitedLocation = new bool[worldMapWidth, worldMapHeight];
    }


    public void LoadData(MapData mapData)
    {
        dataPersistenceManager = DataPersistenceManager.Instance;
        currentMapPosition = mapData.currentMapPosition;
        previousMapPositions = mapData.previousMapPositions;
        previousMapPositionsKeys = mapData.mapPositionsKey;
        previousMapPositionsValues = mapData.mapPositionValue;
        hasVisitedLocation = mapData.hasVisitedLocations;

        if (currentMapPosition.x >= worldMapWidth || currentMapPosition.y >= worldMapHeight || currentMapPosition.x < 0 || currentMapPosition.y < 0)
        {
            Debug.LogError("out of bounds attempted to go to map position: " + currentMapPosition);
        }

        TileData tileData = dataPersistenceManager.GetTileData(dataPersistenceManager.autoSaveID, currentMapPosition);
        if(tileData == null) 
        { 

        }
        else
        {
            gameManager.LoadData(tileData);
        }
    }

    public void SaveData(MapData mapData)
    {
        mapData.currentMapPosition = currentMapPosition;
        mapData.previousMapPositions = previousMapPositions;
        mapData.mapPositionsKey = previousMapPositionsKeys;
        mapData.mapPositionValue = previousMapPositionsValues;
        mapData.hasVisitedLocations = hasVisitedLocation;
    }
    
}
