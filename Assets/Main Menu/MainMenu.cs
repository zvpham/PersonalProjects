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
        DataPersistenceManager.Instance.SaveGame(DataPersistenceManager.Instance.autoSaveID, DataPersistenceManager.Instance.playerID);
        DataPersistenceManager.userID = DataPersistenceManager.Instance.playerID;
        // Load the next Scene - which will in turn Load the game because of 
        // OnsceneLoaded() in the Data persistenceManager
        SceneManager.LoadSceneAsync("Game");
    }

    public void OnLoadGameClicked()
    {
        this.DeactivateMenu();
        saveSlotsMenu.ActivateMenu(true);
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
