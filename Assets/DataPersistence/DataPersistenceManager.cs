using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;

// save game before moving to any new scene

public class DataPersistenceManager : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool disableDataPersistence = false;

    [SerializeField] private bool initializeDataIfNull = false;

    [SerializeField] private bool overrideSelectedProfileId = false;

    [SerializeField] private string testSelectedProfileId = "test";

    [Header("File Storage Config")]

    [SerializeField] private string fileName;
    [SerializeField] private string mapFileName; 
    [SerializeField] private string worldMapFileName;
    [SerializeField] private string GameFileName;
    [SerializeField] private bool useEncryption;

    public TileData tileData;
    private MapData mapData;
    private WorldMapData worldMapData;
    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler mapDataHandler;
    public string playerID = "Player";
    public string autoSaveID = "AutoSave";
    public int MostRecentIntegerTimeID;
    private string selectedProfileId = "";
    // Set These Before Calling a Save File
    public static string userID = "";
    public static string timeID = "";


    public static DataPersistenceManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("Found more than ONe Data Persistence Manager in the Scence");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        if(disableDataPersistence)
        {
            Debug.LogWarning("Data Persistence is currently disabled;");
        }

        this.mapDataHandler = new FileDataHandler(Application.persistentDataPath, mapFileName, useEncryption);

        InitializeSelectedProfileId();
    }

    public void OnEnable()
    {
        // Reminder For Future Me you don't save on OnSceneUnloaded() because it already destroyed the previous scene
        // You have to just save everytime you scene or tile change manually :(
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnApplicationQuit()
    {
        SaveGame(autoSaveID, playerID);
    }

    // Reminder For Future Me you don't save on OnSceneUnloaded() because it already destroyed the previous scene
    // You have to just save everytime you scene or tile change manually :(
    // Adendum -  Now must state which save to change to by changing USer ID IMPORANT !!!!!!!!!!!!!!!!!!!!!!!!!!!
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Might want to change persistentDatapath
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame();
    }

    public void ChangeSelectedProfileID(string newProfileId)
    {
        // update the profile to use for saving and Loading
        this.selectedProfileId = newProfileId;

        //load the Game, which will use that profile, updating our game data Accordingly
        LoadGame();

    }

    public void DeleteProfileData(string profileId)
    {
        // delete the data for his profile ID
        mapDataHandler.Delete(profileId);

        // initialize the selected profile Id
        InitializeSelectedProfileId();

        //reload the game so that our data matches the newly selected profile id
        LoadGame();
    }

    public void DeleteUserData(string fullPath)
    {
        // delete the data 
        string fullerPath =  Path.Combine(selectedProfileId, autoSaveID, fullPath);
        mapDataHandler.Delete(fullerPath);
    }

    public void DeleteTileData(Vector2Int tileLocation)
    {
        string fileName = tileLocation.x.ToString() + "-" + tileLocation.y.ToString();
        mapDataHandler.Delete(selectedProfileId, autoSaveID, playerID, fileName);
    }

    private void InitializeSelectedProfileId()
    {
        
        string tempList = mapDataHandler.GetMostRecentlyUpdatedProfileId();
        try
        {
            this.selectedProfileId = tempList.Substring(0, 1);
            userID = playerID;
        }
        catch
        {
            Debug.LogWarning("need to create a new Game");
        }

        if(overrideSelectedProfileId )
        {
            this.selectedProfileId = testSelectedProfileId;
            Debug.LogWarning("Overrode Selected profile id with test id: " + testSelectedProfileId);
        }
    }

    public void NewGame()
    {
        this.mapData = new MapData();
        this.worldMapData = new WorldMapData();
    }

    public void NewTile()
    {
        this.tileData = new TileData();
    }

    public void LoadGame()
    {
        //return right away if data persistence is disabled
        if(disableDataPersistence)
        {
            return;
        }

        //Load Any saved Data from a file using the Data handler
        this.mapData = mapDataHandler.LoadMapData(selectedProfileId, autoSaveID, userID);

        //start a new game if the data is null and we're configured to initialize data for debugging Purposes
        if(this.mapData == null  && initializeDataIfNull)
        {
            NewGame();
        }

        //if no data can be loaded, initialize to a new game
        if(this.mapData == null)
        {
            Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
            return;
        }

        // Push the Loaded Data to all other scripts that need it
        foreach(IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(mapData);
        }
    }

    public MapData GetMapData()
    {
        if (mapData == null)
        {
            Debug.LogError("Need to Create MapData before Calling");
            return null;
        }

        return mapData;
    }

    public void SetMapData(MapData newMapData)
    {
        if (newMapData == null)
        {
            Debug.LogError("NewMapdataIsNull");
            return;
        }
        mapData = newMapData;
    }

    public TileData GetTileData(Vector2Int tileLocation)
    {
        string fileName = tileLocation.x.ToString() + "-" + tileLocation.y.ToString();
        //Load Any saved Data from a file using the Data handler
        TileData tileData = mapDataHandler.Load(selectedProfileId, timeID, userID, fileName);

        //if no data can be loaded, initialize to a new game
        if (tileData == null)
        {
            tileData = mapDataHandler.Load(selectedProfileId, timeID, fileName);
            Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
            if(tileData == null)
            {
                tileData = mapDataHandler.Load(selectedProfileId, fileName);
            }
        }

        return tileData;
    }

    // Attempts to get Recent tileData when Scene Loads
    public TileData GetTileData(string dirName, string subFolder, Vector2Int tileLocation)
    {
        string fileName = tileLocation.x.ToString() + "-" + tileLocation.y.ToString();
        TileData tileData = mapDataHandler.Load(selectedProfileId, dirName, subFolder, fileName);

        if (tileData == null)
        {
            Debug.Log("No data was found in recent Data. Attempting to load Frozen Tile for Current Position:" + tileLocation);
            tileData = mapDataHandler.Load(selectedProfileId, dirName, fileName);
            if (tileData == null)
            {
                Debug.Log("Failed to Find Frozen TileData");
            }
        }

        return tileData;
    }

    // Attempts to get Frozen Tile Data
    public TileData GetTileData(string dirName, Vector2Int tileLocation)
    {
        string fileName = tileLocation.x.ToString() + "-" + tileLocation.y.ToString();
        TileData tileData = mapDataHandler.Load(selectedProfileId, dirName, fileName);

        if (tileData == null)
        {
            tileData = null;
        }

        return tileData;
    }

    public GameData GetGameData()
    {
        gameData = mapDataHandler.LoadGameData(GameFileName);

        if (gameData == null)
        {
            gameData = new GameData();
        }

        return gameData;
    }

    public WorldMapData GetWorldMapData()
    {
        worldMapData = mapDataHandler.LoadWorldMapData(selectedProfileId, worldMapFileName);

        if (worldMapData == null)
        {
            Debug.LogError("No data was found. World Map Data needs to be created. This Shouldn't Happen");
            return null;
        }

        return worldMapData;
    }

    private void SaveGameDataBase()
    {
        //return right away if data persistence is disabled
        if (disableDataPersistence)
        {
            return;
        }

        //if we don't have any data to save, log a warning here
        if (this.gameData == null)
        {
            Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
            return;
        }
    }

    public void SaveGameData()
    {
        SaveGameDataBase();
        mapDataHandler.Save(gameData, worldMapFileName);
    }

    private void SaveWorldMapDataBase()
    {
        //return right away if data persistence is disabled
        if (disableDataPersistence)
        {
            return;
        }

        //if we don't have any data to save, log a warning here
        if (this.worldMapData == null)
        {
            Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
            return;
        }
    }

    public void SaveWorldMapData()
    {
        SaveWorldMapDataBase();
        mapDataHandler.Save(worldMapData, selectedProfileId, worldMapFileName);
    }

    private void SaveTileDataBase(TileData gameData)
    {
        //return right away if data persistence is disabled
        if (disableDataPersistence)
        {
            return;
        }

        //if we don't have any data to save, log a warning here
        if (gameData == null)
        {
            Debug.Log("No TileData, Should Probably load that");
            return;
        }
    }

    public void SaveTileData(int timeID, string subFolder, Vector2Int mapPosition)
    {
        SaveTileDataBase(tileData);
        string tilePosition =  mapPosition.x.ToString() + "-" + mapPosition.y.ToString();
        mapDataHandler.Save(tileData, selectedProfileId, timeID.ToString(), subFolder, tilePosition);
    }

    // For AutoSaves
    // Note - AutoSave right after any manual save EX - Story events, Maybe for manuel saves
    public void SaveTileData(TileData gameData, string dirName, string subFolder, Vector2Int mapPosition)
    {
        SaveTileDataBase(gameData);
        string tilePosition = mapPosition.x.ToString() + "-" + mapPosition.y.ToString();
        mapDataHandler.Save(gameData, selectedProfileId, dirName, subFolder, tilePosition);
    }

    // For Manuel Saves like Precognition
    // Automatically sets TimeID to AUtoSave
    public void SaveTileData(TileData gameData, string dirName, Vector2Int mapPosition)
    {
        SaveTileDataBase(gameData);
        string tilePosition = mapPosition.x.ToString() + "-" + mapPosition.y.ToString();
        mapDataHandler.Save(gameData, selectedProfileId, dirName, tilePosition);
    }

    private void SaveGameBase()
    {
        //return right away if data persistence is disabled
        if (disableDataPersistence)
        {
            return;
        }

        //if we don't have any data to save, log a warning here
        if (this.mapData == null)
        {
            Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
            return;
        }

        // pass data to other scirpts so they can update it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(mapData);
        }

        mapData.lastUpdated = System.DateTime.Now.ToBinary();

    }

    public void SaveGame(int timeID, string subFolder)
    {
        SaveGameBase();
        //Save that data to a file using the data handler
        mapDataHandler.Save(mapData, selectedProfileId, timeID.ToString(), subFolder);
    }

    // For AutoSaves
    // Note - AutoSave right after any manual save EX - Story events, Maybe for manuel saves
    public void SaveGame(string dirName, string subFolder)
    {
        SaveGameBase();
        //Save that data to a file using the data handler
        mapDataHandler.Save(mapData, selectedProfileId, dirName, subFolder);
    }

    // For Manuel Saves and Precognition
    public void SaveGame(string subFolder)
    {
        SaveGameBase();
        //Save that data to a file using the data handler
        mapDataHandler.Save(mapData, selectedProfileId, subFolder);
    }

    public void ChangeGameData(string subFolder)
    {
        this.tileData.saveToDelete = subFolder;
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        // FindObjectsofType takes in an optional boolean to include inactive gameObjects
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true)
            .OfType<IDataPersistence>();
         
        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    public bool HasGameData()
    {
        return mapData != null;
    }

    public string GetPlayerId()
    {
        return userID.ToString();
    }

    public Dictionary<string, MapData> GetAllProfilesMapData()
    {
        return mapDataHandler.LoadAllProfiles();
    }
}
