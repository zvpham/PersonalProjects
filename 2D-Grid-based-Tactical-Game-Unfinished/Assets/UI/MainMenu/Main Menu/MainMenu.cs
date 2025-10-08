using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : Menu
{
    [Header("Menu Navigation")]
    [SerializeField] private SaveSlotsMenu saveSlotsMenu;

    [Header("Main Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button loadGameButton;

    private void Start()
    {
        DisableButtonsDependingOnData();
    }

    private void DisableButtonsDependingOnData()
    {
        if (!DataPersistenceManager.Instance.HasGameData())
        {
            continueGameButton.interactable = false;
            loadGameButton.interactable = false;
        }
    }

    public void OnNewGameClicked()
    {
        this.DeactivateMenu();
        saveSlotsMenu.ActivateMenu(false);
    }

    public void OnContinueGameClicked()
    {
        DisableMenuButtons();
        // save the game anytime before Loading a new Scene

        /*
        DataPersistenceManager.Instance.SaveGame(DataPersistenceManager.Instance.autoSaveID, DataPersistenceManager.Instance.playerID);
        */
        // Load the next Scene - which will in turn Load the game because of 
        // OnsceneLoaded() in the Data persistenceManager

        WorldMapData mostRecentSave =  DataPersistenceManager.Instance.GetMostRecentMapData();
        if (mostRecentSave.isAutoSave)
        {
            DataPersistenceManager.Instance.saveID = DataPersistenceManager.Instance.autoSaveID;
        }
        else
        {
            DataPersistenceManager.Instance.saveID = DataPersistenceManager.Instance.timeSaveID;
        }
        DataPersistenceManager.Instance.saveNumID = mostRecentSave.saveNumber.ToString();
        Debug.Log(mostRecentSave.saveNumber.ToString());
        if(mostRecentSave.inCombat == true)
        {
            SceneManager.LoadSceneAsync("Combat");
        }
        else
        {
            SceneManager.LoadSceneAsync("OverWorld");
        }
    }

    public void OnLoadGameClicked()
    {
        this.DeactivateMenu();
        saveSlotsMenu.ActivateMenu(true);
    }

    public void ActivateChooseClassMenu()
    {
        saveSlotsMenu.DeactivateMenu();
        Debug.LogError("Add An Start Options Menu, variable starts, amount of troops/leaders, theme etc");
    }

    private void DisableMenuButtons()
    {
        newGameButton.interactable = false;
        continueGameButton.interactable = false;
    }

    public void ActivateMenu()
    {
        this.gameObject.SetActive(true);
        DisableButtonsDependingOnData();
    }

    public void DeactivateMenu()
    {
        this.gameObject.SetActive(false);
    }
}
