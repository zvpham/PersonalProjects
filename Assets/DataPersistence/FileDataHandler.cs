using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Profiling;

public class FileDataHandler
{
    private string dataDirPath = "";

    private string dataFileName = "";

    private bool useEncryption = false;

    private readonly string encryptionCodeWord = "TouchGrass";

    private readonly string fileExtension = ".game";

    private readonly string backupExtension = ".bak";

    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    public WorldMapData LoadWorldMapData(string profileId, string fileName, bool allowedRestoreFromBackup = true)
    {
        // base Case - if the profileId is nll, return right away
        if (profileId == null || fileName == null)
        {
            return null;
        }

        // Use Path.Combine to Accound for Differenct OS's having different Path Seperators
        string fullPath = Path.Combine(dataDirPath, profileId, fileName + fileExtension);
        List<string> dataPath = new List<string>() { profileId, fileName };
        return LoadWorldMapBase(fullPath, dataPath, allowedRestoreFromBackup);
    }

    public WorldMapData LoadWorldMapBase(string fullPath, List<string> dataPath, bool allowedRestoreFromBackup = true)
    {
        WorldMapData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                //Optionally Decrypt Data
                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                //Deserialize the data from Json back into the C# object
                loadedData = JsonUtility.FromJson<WorldMapData>(dataToLoad);
            }
            catch (Exception e)
            {
                //Since We're calling Load recursively, we need to account for the case wehre
                // the Rollback succeseds, but data is still failing to load for some other reason
                // which without this scheck may happen an infinite amount of tims
                if (allowedRestoreFromBackup)
                {
                    Debug.LogWarning("Failed to Load data file. Attempting to roll back. \n" + e);
                    bool rollbackSuccess = AttemptRollback(fullPath);
                    if (rollbackSuccess)
                    {
                        // try to load again recursively for rollback\
                        switch (dataPath.Count)
                        {
                            case 0:
                                Debug.LogError("ROLLBACK SAVES ARE BROKEN HELP");
                                return null;
                            case 2:
                                loadedData = LoadWorldMapData(dataPath[0], dataPath[1], false);
                                break;
                        }
                    }
                }
                // if we hit this, one possibility is that the backup file is also corrupt
                else
                {
                    Debug.LogError("error occured when trying to load file at path: " + fullPath
                        + " and backup did not work.\n" + e);
                }
            }
        }
        return loadedData;
    }

    public TileData Load(string profileId, string fileName, bool allowedRestoreFromBackup = true)
    {
        // base Case - if the profileId is nll, return right away
        if(profileId == null || fileName == null)
        {
            return null;
        }

        // Use Path.Combine to Accound for Differenct OS's having different Path Seperators
        string fullPath = Path.Combine(dataDirPath, profileId, fileName + fileExtension);
        List<string> dataPath = new List<string>() { profileId, fileName};
        return LoadBase(fullPath, dataPath, allowedRestoreFromBackup);
    }

    public TileData Load(string profileId, string timeID, string fileName, bool allowedRestoreFromBackup = true)
    {
        // base Case - if the profileId is nll, return right away
        if (profileId == null || timeID == null || fileName == null)
        {
            return null;
        }

        // Use Path.Combine to Accound for Differenct OS's having different Path Seperators
        string fullPath = Path.Combine(dataDirPath, profileId, timeID, fileName + fileExtension);
        List<string> dataPath = new List<string>() { profileId, timeID, fileName};
        return LoadBase(fullPath, dataPath, allowedRestoreFromBackup);
    }

    public TileData Load(string profileId, string timeID, string subFolder, string fileName, bool allowedRestoreFromBackup = true)
    {
        // base Case - if the profileId is nll, return right away
        if (profileId == null || timeID == null || subFolder == null || fileName == null)
        {
            return null;
        }

        // Use Path.Combine to Accound for Differenct OS's having different Path Seperators
        string fullPath = Path.Combine(dataDirPath, profileId, timeID, subFolder, fileName + fileExtension);
        List<string> dataPath = new List<string>() { profileId, timeID, subFolder, fileName };
        return LoadBase(fullPath, dataPath, allowedRestoreFromBackup);
    }

    public TileData LoadBase(string fullPath, List<string> dataPath, bool allowedRestoreFromBackup = true)
    {
        TileData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                //Optionally Decrypt Data
                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                //Deserialize the data from Json back into the C# object
                loadedData = JsonUtility.FromJson<TileData>(dataToLoad);
            }
            catch (Exception e)
            {
                //Since We're calling Load recursively, we need to account for the case wehre
                // the Rollback succeseds, but data is still failing to load for some other reason
                // which without this scheck may happen an infinite amount of tims
                if (allowedRestoreFromBackup)
                {
                    Debug.LogWarning("Failed to Load data file. Attempting to roll back. \n" + e);
                    bool rollbackSuccess = AttemptRollback(fullPath);
                    if (rollbackSuccess)
                    {
                        // try to load again recursively for rollback\
                        switch (dataPath.Count)
                        {
                            case 0:
                                Debug.LogError("ROLLBACK SAVES ARE BROKEN HELP");
                                return null;
                            case 2:
                                 loadedData = Load(dataPath[0], dataPath[1], false);
                                break;
                            case 3:
                                loadedData = Load(dataPath[0], dataPath[1], dataPath[2], false);
                                break;
                            case 4:
                                loadedData = Load(dataPath[0], dataPath[1], dataPath[2], dataPath[3], false);
                                break;
                        }
                    }
                }
                // if we hit this, one possibility is that the backup file is also corrupt
                else
                {
                    Debug.LogError("error occured when trying to load file at path: " + fullPath
                        + " and backup did not work.\n" + e);
                }
            }
        }
        return loadedData;
    }

    public MapData LoadMapData(string profileId, string timeID, string subFolder, bool allowedRestoreFromBackup = true)
    {
        // base Case - if the profileId is nll, return right away
        if (profileId == null)
        {
            return null;
        }
        if (timeID == null)
        {
            return null;
        }
        if (subFolder == null)
        {
            return null;
        }

        // Use Path.Combine to Accound for Differenct OS's having different Path Seperators
        string fullPath = Path.Combine(dataDirPath, profileId, timeID, subFolder, dataFileName + fileExtension);
        List<string> dataPath = new List<string>() { profileId, timeID, subFolder };
        return LoadMapDataBase(fullPath, dataPath, allowedRestoreFromBackup);
    }

    public MapData LoadMapDataBase(string fullPath, List<string> dataPath, bool allowedRestoreFromBackup = true)
    {
        MapData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                //Optionally Decrypt Data
                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                //Deserialize the data from Json back into the C# object
                loadedData = JsonUtility.FromJson<MapData>(dataToLoad);
            }
            catch (Exception e)
            {
                //Since We're calling Load recursively, we need to account for the case wehre
                // the Rollback succeseds, but data is still failing to load for some other reason
                // which without this scheck may happen an infinite amount of tims
                if (allowedRestoreFromBackup)
                {
                    Debug.LogWarning("Failed to Load data file. Attempting to roll back. \n" + e);
                    bool rollbackSuccess = AttemptRollback(fullPath);
                    if (rollbackSuccess)
                    {
                        // try to load again recursively for rollback\
                        switch (dataPath.Count)
                        {
                            case 0:
                                Debug.LogError("ROLLBACK SAVES ARE BROKEN HELP");
                                return null;
                            case 1:
                                //ProfileID
                                //loadedData = Load(dataPath[0], false);
                                break;
                            case 2:
                                //Currenly not Implemented
                                break;
                            case 3:
                                loadedData = LoadMapData(dataPath[0], dataPath[1], dataPath[2], false);
                                break;
                        }
                    }
                }
                // if we hit this, one possibility is that the backup file is also corrupt
                else
                {
                    Debug.LogError("error occured when trying to load file at path: " + fullPath
                        + " and backup did not work.\n" + e);
                }
            }
        }
        return loadedData;
    }

    public void Save(MapData data, string profileId, string timeID, string subFolder)
    {

        // base Case - if the profileId is nll, return right away
        if (profileId == null)
        {
            return;
        }

        // Use Path.Combine to Accound for Differenct OS's having different Path Seperators
        string temp = Path.Combine(profileId, timeID, subFolder);
        string fullPath = Path.Combine(dataDirPath, temp, dataFileName + fileExtension);
        string backupFilePAth = fullPath + backupExtension;
        try
        {
            // Create the DIrectory the file will be written to if it doesn't alraeady exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the C# game data object into Json
            string dataToStore = JsonUtility.ToJson(data, true);

            //Optionally Encrypt Data
            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // write the serialized datea to the file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }

            // verify the newly saved file can be loaded successfully
            MapData verifiedGameDatya = LoadMapData(profileId, timeID, subFolder);
            //if the data can be verified, back it up;
            if(verifiedGameDatya != null)
            {
                File.Copy(fullPath, backupFilePAth, true);
            }
            // otherwise, something went wrong and we should throw up an exception
            else
            {
                Debug.LogError("Save file could not be verified a nd Backup could not be created.");
            }
        }
        catch (Exception e) 
        {
            Debug.LogError("Error occured whne trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    public void Save(MapData data, string profileId, string subFolder)
    {

        // base Case - if the profileId is null, return right away
        if (profileId == null || subFolder == null)
        {
            return;
        }

        // Use Path.Combine to Accound for Differenct OS's having different Path Seperators
        string fullPath = Path.Combine(dataDirPath, profileId, DataPersistenceManager.Instance.autoSaveID, subFolder, dataFileName + fileExtension);
        string backupFilePAth = fullPath + backupExtension;
        try
        {
            // Create the Directory the file will be written to if it doesn't alraeady exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the C# game data object into Json
            string dataToStore = JsonUtility.ToJson(data, true);

            //Optionally Encrypt Data
            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // write the serialized datea to the file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }

            // verify the newly saved file can be loaded successfully
            MapData verifiedGameDatya = LoadMapData(profileId, DataPersistenceManager.Instance.autoSaveID, subFolder);
            //if the data can be verified, back it up;
            if (verifiedGameDatya != null)
            {
                File.Copy(fullPath, backupFilePAth, true);
            }
            // otherwise, something went wrong and we should throw up an exception
            else
            {
                Debug.LogError("Save file could not be verified a nd Backup could not be created.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured whne trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    public void Save(TileData data, string profileId, string timeID, string subFolder, string dataFileName)
    {

        // base Case - if the profileId is nll, return right away
        if (profileId == null)
        {
            return;
        }

        // Use Path.Combine to Accound for Differenct OS's having different Path Seperators
        string temp = Path.Combine(profileId, timeID, subFolder);
        string fullPath = Path.Combine(dataDirPath, temp, dataFileName + fileExtension);
        string backupFilePAth = fullPath + backupExtension;
        try
        {
            // Create the DIrectory the file will be written to if it doesn't alraeady exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the C# game data object into Json
            string dataToStore = JsonUtility.ToJson(data, true);

            //Optionally Encrypt Data
            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // write the serialized datea to the file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }

            // verify the newly saved file can be loaded successfully
            TileData verifiedGameDatya = Load(profileId, timeID, subFolder, dataFileName);
            //if the data can be verified, back it up;
            if (verifiedGameDatya != null)
            {
                File.Copy(fullPath, backupFilePAth, true);
            }
            // otherwise, something went wrong and we should throw up an exception
            else
            {
                Debug.LogError("Save file could not be verified a nd Backup could not be created.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured whne trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    public void Save(TileData data, string profileId, string subFolder, string dataFileName)
    {

        // base Case - if the profileId is null, return right away
        if (profileId == null || subFolder == null)
        {
            return;
        }

        // Use Path.Combine to Accound for Differenct OS's having different Path Seperators
        string fullPath = Path.Combine(dataDirPath, profileId, DataPersistenceManager.Instance.autoSaveID, subFolder, dataFileName + fileExtension);
        string backupFilePAth = fullPath + backupExtension;
        try
        {
            // Create the Directory the file will be written to if it doesn't alraeady exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the C# game data object into Json
            string dataToStore = JsonUtility.ToJson(data, true);

            //Optionally Encrypt Data
            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // write the serialized datea to the file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }

            // verify the newly saved file can be loaded successfully
            TileData verifiedGameDatya = Load(profileId, DataPersistenceManager.Instance.autoSaveID, subFolder, dataFileName);
            //if the data can be verified, back it up;
            if (verifiedGameDatya != null)
            {
                File.Copy(fullPath, backupFilePAth, true);
            }
            // otherwise, something went wrong and we should throw up an exception
            else
            {
                Debug.LogError("Save file could not be verified a nd Backup could not be created.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured whne trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    public void Save(TileData data, string profileId, string dataFileName)
    {

        // base Case - if the profileId is null, return right away
        if (profileId == null || dataFileName == null)
        {
            return;
        }

        // Use Path.Combine to Accound for Differenct OS's having different Path Seperators
        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName + fileExtension);
        string backupFilePAth = fullPath + backupExtension;
        try
        {
            // Create the Directory the file will be written to if it doesn't alraeady exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the C# game data object into Json
            string dataToStore = JsonUtility.ToJson(data, true);

            //Optionally Encrypt Data
            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // write the serialized datea to the file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }

            // verify the newly saved file can be loaded successfully
            TileData verifiedGameDatya = Load(profileId, dataFileName);
            //if the data can be verified, back it up;
            if (verifiedGameDatya != null)
            {
                File.Copy(fullPath, backupFilePAth, true);
            }
            // otherwise, something went wrong and we should throw up an exception
            else
            {
                Debug.LogError("Save file could not be verified a nd Backup could not be created.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured whne trying to save data to file: " + fullPath + "\n" + e);
        }
    }


    public void Save(WorldMapData data, string profileId, string dataFileName)
    {

        // base Case - if the profileId is null, return right away
        if (profileId == null || dataFileName == null)
        {
            return;
        }

        // Use Path.Combine to Accound for Differenct OS's having different Path Seperators
        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName + fileExtension);
        string backupFilePAth = fullPath + backupExtension;
        try
        {
            // Create the Directory the file will be written to if it doesn't alraeady exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize the C# game data object into Json
            string dataToStore = JsonUtility.ToJson(data, true);

            //Optionally Encrypt Data
            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // write the serialized datea to the file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }

            // verify the newly saved file can be loaded successfully
            WorldMapData verifiedGameDatya = LoadWorldMapData(profileId, dataFileName);
            //if the data can be verified, back it up;
            if (verifiedGameDatya != null)
            {
                File.Copy(fullPath, backupFilePAth, true);
            }
            // otherwise, something went wrong and we should throw up an exception
            else
            {
                Debug.LogError("Save file could not be verified a nd Backup could not be created.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured whne trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    public void Delete(string profileId)
    {
        // base case - if the profileId is null return right away
        if(profileId == null)
        {
            return;
        }

        string fullpath = Path.Combine(dataDirPath, profileId);
        Debug.Log(fullpath);
        try
        {
            // ensure the data file exists at this path before deleting the directory

            if (Directory.Exists(fullpath))
            {
                // delete the profile folder and everything within it
                //use File.Delete() if you only want to delete a specific file
                Directory.Delete(fullpath, true);
            }
            else
            {
                Debug.LogWarning("Tried to delete profile data, but data was not found at path: " + fullpath);
            }
        }
        catch(Exception e)
        {
            Debug.LogError("Failed to delete Profile data for profileID: "
                + profileId + " at path: " + fullpath + "\n" + e);  
        }
    }

    public Dictionary<string, MapData> LoadAllProfiles()
    {
        Dictionary<string, MapData> profileDictionary = new Dictionary<string, MapData>();

        //Loop over all directory names in the data directionary path
        IEnumerable<DirectoryInfo> profileDirInfos = new DirectoryInfo(dataDirPath).EnumerateDirectories();

        foreach(DirectoryInfo profileDirInfo in profileDirInfos)
        {
            // Loop Though all directeries(TimeIDs and Autosave) in a Profile
            IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(Path.Combine(dataDirPath, profileDirInfo.Name)).EnumerateDirectories();

            foreach(DirectoryInfo dirInfo in dirInfos)
            {
                if (dirInfo.Name.Equals(DataPersistenceManager.Instance.autoSaveID))
                {
                    IEnumerable<DirectoryInfo> userInfos = new DirectoryInfo(Path.Combine(dataDirPath, profileDirInfo.Name, dirInfo.Name)).EnumerateDirectories();

                    foreach (DirectoryInfo userInfo in userInfos)
                    {
                        if (userInfo.Name.Equals(DataPersistenceManager.Instance.playerID))
                        {
                            // defensive programming - check if data file exists
                            // if it doesn't, then folder isn't a profile and should be skipped
                            string fullPath = Path.Combine(dataDirPath, profileDirInfo.Name, dirInfo.Name, userInfo.Name, dataFileName + fileExtension);
                            if (!File.Exists(fullPath))
                            {
                                Debug.LogWarning("Skipping Directory when loading all profiles because it does not contain data: " + profileDirInfo.Name +
                                    " " + dirInfo.Name + " " + userInfo.Name);
                                continue;
                            }

                            // Load the game data for this profile and put it in the Dictionary
                            MapData profileData = LoadMapData(profileDirInfo.Name, dirInfo.Name, userInfo.Name);

                            //defensive programming ensure the profie data isn't null,
                            // because if it is then something went wrong and we should let Ourselves know
                            if (profileData != null)
                            {
                                string temp = profileDirInfo.Name  + " "+ dirInfo.Name + " " + userInfo.Name;
                                profileDictionary.Add(temp, profileData);
                            }
                            else
                            {
                                Debug.LogError("Tried to Load Profile but something went Wrong. ProfileID: " + profileDirInfo.Name +
                                    " "  + dirInfo.Name + " " + userInfo.Name);
                            }
                        }
                    }
                }
            }
        }

        return profileDictionary;
    }

    public string GetMostRecentlyUpdatedProfileId()
    {
        string mostRecentProfileId = null;

        Dictionary<string, MapData> profilesGameData = LoadAllProfiles();
        foreach(KeyValuePair<string, MapData> pair in profilesGameData)
        {
            string profileId = pair.Key;
            MapData mapData = pair.Value;

            // SKip entry if null
            if(mapData == null)    
            {
                continue;
            }

            // if this is the first data we've come acrss that exists. It's the most recen so far
            if(mostRecentProfileId == null)
            {
                mostRecentProfileId = profileId;
            }
            //otherwise, compare to see which date is the most recent
            else
            {
                DateTime mostRecentDateTime = DateTime.FromBinary(profilesGameData[mostRecentProfileId].lastUpdated);
                DateTime newDateTime = DateTime.FromBinary(mapData.lastUpdated);

                // the greastest DateTime value is the most recent
                if(newDateTime > mostRecentDateTime)
                {
                    mostRecentProfileId = profileId;
                }
            }
        }
        return mostRecentProfileId;
    }
     
    private string EncryptDecrypt(string data)
    {
        string modifiedData = "";
        for(int i =  0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
        }
        return modifiedData;
    } 

    private bool AttemptRollback(string fullPath)
    {
        bool success = false;
        string backupFilePath = fullPath + backupExtension;

        try
        {
            // if the file exists, attempyt to roll back to it by overwriting the original file
            if(File.Exists(backupFilePath))
            {
                File.Copy(backupFilePath, fullPath, true);
                success = true;
                Debug.LogWarning("Had to roll back to backupFile at: " + backupFilePath);
            }
            // otherwise, we Don't yet have a backup file -  so theere's nothing to roll back to
            else
            {
                throw new Exception("Tried to roll back, but no backup file exists to rollback to");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured whn trying to roll back to backup file at: "
                + backupFilePath + "\n" + e);
        }
        return success;
    }
}
