using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Profiling;
using System;

// save game before moving to any new scene

public class DataPersistenceManager : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] public bool disableDataPersistence = false;

    [SerializeField] private bool initializeDataIfNull = false;

    [SerializeField] private bool overrideSelectedProfileId = false;

    [SerializeField] private string testSelectedProfileId = "test";

    [Header("File Storage Config")]

    [SerializeField] private string fileName;
    [SerializeField] private string mapFileName; 
    [SerializeField] private string worldMapFileName;
    [SerializeField] private string gameFileName;
    [SerializeField] private bool useEncryption;

    private GameData mapData;
    private WorldMapData worldMapData;
    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler mapDataHandler;
    public string autoSaveID = "AutoSave";
    public string timeSaveID = "TimeID";
    public int MostRecentIntegerTimeID;
    private string selectedProfileId = "";
    public int autoSaveNumber;
    // Set These Before Calling a Save File
    public string saveNumID = "";
    public string saveID = "";
    public int maxAutoSaves = 30;
    public int maxTimeIDSaves = 40;


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


        this.mapDataHandler = new FileDataHandler(Application.persistentDataPath, worldMapFileName, useEncryption);

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

    // Reminder For Future Me you don't save on OnSceneUnloaded() because it already destroyed the previous scene
    // You have to just save everytime you scene or tile change manually :(
    // Adendum -  Now must state which save to change to by changing USer ID IMPORANT !!!!!!!!!!!!!!!!!!!!!!!!!!!
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Might want to change persistentDatapath
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
    }
    private void OnApplicationQuit()
    {
        //SaveGame(autoSaveID);
    }

    public void ChangeSelectedProfileID(string newProfileId)
    {
        // update the profile to use for saving and Loading
        this.selectedProfileId = newProfileId;
    }

    public void DeleteProfileData(string profileId)
    {
        // delete the data for his profile ID
        mapDataHandler.Delete(profileId);

        // initialize the selected profile Id
        InitializeSelectedProfileId();


    }

    public void DeleteUserData(string fullPath)
    {
        // delete the data 
        string fullerPath =  Path.Combine(selectedProfileId, autoSaveID, fullPath);
        mapDataHandler.Delete(fullerPath);
    }

    /*
    public void DeleteTileData(Vector2Int tileLocation)
    {
        string fileName = tileLocation.x.ToString() + "-" + tileLocation.y.ToString();
        mapDataHandler.Delete(selectedProfileId, autoSaveID, playerID, fileName);
    }
    */
    private void InitializeSelectedProfileId()
    {
        
        string tempList = mapDataHandler.GetMostRecentlyUpdatedProfileId();
        try
        {
            this.selectedProfileId = tempList.Substring(0, 1);
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
        this.mapData = new GameData();
        this.worldMapData = new WorldMapData();
    }

    public void LoadGame()
    {
        //return right away if data persistence is disabled
        if(disableDataPersistence)
        {
            return;
        }

        if(saveID == "" || saveNumID == "")
        {
            Debug.LogError("tried to load a save with no SaveID " + saveID + "or SaveNumID: " + saveNumID);
        }

        //Load Any saved Data from a file using the Data handler
        this.worldMapData = mapDataHandler.LoadWorldMapData(selectedProfileId, saveID, saveNumID, worldMapFileName);

        //start a new game if the data is null and we're configured to initialize data for debugging Purposes
        if(this.worldMapData == null  && initializeDataIfNull)
        {
            NewGame();
        }

        //if no data can be loaded, initialize to a new game
        if(this.worldMapData == null)
        {
            Debug.Log("No data was found. A New Game needs to be started before data can be loaded.");
            return;
        }

        // Push the Loaded Data to all other scripts that need it
        foreach(IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.LoadData(worldMapData);
        }
    }

    public GameData GetMapData()
    {
        if (mapData == null)
        {
            Debug.LogError("Need to Create MapData before Calling");
            return null;
        }

        return mapData;
    }

    public void SetMapData(GameData newMapData)
    {
        if (newMapData == null)
        {
            Debug.LogError("NewMapdataIsNull");
            return;
        }
        mapData = newMapData;
    }

   

    public GameData GetGameData()
    {
        gameData = mapDataHandler.LoadGameData(gameFileName);

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
        //mapDataHandler.Save(worldMapData, selectedProfileId, worldMapFileName);
    }

    private void SaveGameBase()
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

        // pass data to other scirpts so they can update it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(worldMapData);
        }

        worldMapData.lastUpdated = System.DateTime.Now.ToBinary();
        worldMapData.selectedProfileId = selectedProfileId;
    }

    // For TimeIDS (Manual Saves
    public void SaveGame(string dirFolder, int timeID)
    {
        SaveGameBase();
        //Save that data to a file using the data handler
        mapDataHandler.Save(worldMapData, selectedProfileId, dirFolder, timeID.ToString(), worldMapFileName);
    }

    // For AutoSaves
    // Note - AutoSave right after any manual save EX - Story events, Maybe for manuel saves
    public void SaveGame(string dirFolder, string subFolder)
    {
        SaveGameBase();
        //Save that data to a file using the data handler
        mapDataHandler.Save(worldMapData, selectedProfileId, dirFolder, subFolder, worldMapFileName);
    }

    public void SaveGame(string dirFolder)
    {
        SaveGameBase();

        if(dirFolder == autoSaveID)
        {
            worldMapData.isAutoSave = true;
        }

        int oldestSaveId = 0;
        //figure out what subfolder to save file to
        List<List<WorldMapData>> profilesData = GetAllProfilesMapData();
        for (int i = 0; i < profilesData.Count; i++)
        {
            if (profilesData[i].Count > 0 && profilesData[i][profilesData[i].Count - 1].selectedProfileId == selectedProfileId)
            {
                List<WorldMapData> profileData = profilesData[i];
                List<WorldMapData> autoSavesData = new List<WorldMapData>();
                List<WorldMapData> timeIDSavesData = new List<WorldMapData>();
                for (int j = 0; j < profileData.Count; j++)
                {
                    if (profileData[j].isAutoSave)
                    {
                        autoSavesData.Add(profileData[j]);
                    }
                    else
                    {
                        timeIDSavesData.Add(profileData[j]);
                    }
                }

                if (dirFolder == autoSaveID)
                {
                    if (autoSavesData.Count >= maxAutoSaves)
                    {
                        oldestSaveId = autoSavesData[0].saveNumber;
                        DateTime oldestDateTime = DateTime.FromBinary(autoSavesData[0].lastUpdated);
                        for (int k = 0; k < autoSavesData.Count; k++)
                        {
                            //Debug.Log("Hello: " + autoSavesData[k].saveNumber + " " + DateTime.FromBinary(autoSavesData[k].lastUpdated)  + " "  + oldestSaveId + " "+ oldestDateTime);
                            if (DateTime.FromBinary(autoSavesData[k].lastUpdated) < oldestDateTime)
                            {
                                //Debug.Log("Hello");
                                oldestSaveId = autoSavesData[k].saveNumber;
                                oldestDateTime = DateTime.FromBinary(autoSavesData[k].lastUpdated);
                            }
                        }
                    }
                    else
                    {
                        List<int> currentAutoSaves = new List<int>();
                        for (int k = 0; k < autoSavesData.Count; k++)
                        {
                            currentAutoSaves.Add(autoSavesData[k].saveNumber);
                        }

                        for (int l = 0; l < maxAutoSaves; l++)
                        {
                            if (!currentAutoSaves.Contains(l))
                            {
                                oldestSaveId = l;
                                break;
                            }
                        }
                    }
                }
                else if (dirFolder == timeSaveID)
                {
                    if (timeIDSavesData.Count >= maxTimeIDSaves)
                    {
                        Debug.LogError("Not Implemented Yet: Add Menu To Delete Save");
                        return;
                    }
                    else
                    {
                        List<int> currentTimeIDSaves = new List<int>();
                        for (int k = 0; k < timeIDSavesData.Count; k++)
                        {
                            currentTimeIDSaves.Add(timeIDSavesData[k].saveNumber);
                        }

                        for (int l = 0; l < maxTimeIDSaves; l++)
                        {
                            if (!currentTimeIDSaves.Contains(l))
                            {
                                oldestSaveId = l;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("Should be saving to either Time or AutoSave Folder not: " + dirFolder);
                }
            }
        }
        worldMapData.saveNumber = oldestSaveId;
        //Save that data to a file using the data handler
        mapDataHandler.Save(worldMapData, selectedProfileId, dirFolder, oldestSaveId.ToString(), worldMapFileName);
    }

    public void ChangeGameData(string subFolder)
    {
        //this.tileData.saveToDelete = subFolder;
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
        return mapDataHandler.LoadAllProfiles().Count > 0;
    }

    public List<List<WorldMapData>> GetAllProfilesMapData()
    {
        return mapDataHandler.LoadAllProfiles();
    }

    public WorldMapData GetMostRecentMapData()
    {
        Int32.TryParse(selectedProfileId, out int id);
        List<List<WorldMapData>> profileDatas = GetAllProfilesMapData();
        return profileDatas[id][profileDatas[id].Count - 1];
    }
}
