using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveSlotsMenu : Menu
{
    [Header("Menu Navigation")]
    [SerializeField] private MainMenu mainmenu;


    [Header("Menu Buttons")]
    [SerializeField] private Button BackButton;

    [Header("Confirmation Popup")]
    [SerializeField] private ConfirmationPopupMenu confirmPopupMenu;

    private SaveSlot[] saveSlots;

    private bool isLoadingGame = false;

    private void Awake()
    {
        saveSlots = this.GetComponentsInChildren<SaveSlot>();
        gameObject.SetActive(false);
    }

    public void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        //Disable All Buttons
        DisableAllMenuButtons();

        // case - Loading game
        if (isLoadingGame)
        {
            DataPersistenceManager.Instance.ChangeSelectedProfileID(saveSlot.GetProfileId());
            SaveGameAndLoadScene();
        }
        // case - newGame, but the saveslot Has data
        else if (saveSlot.hasData)
        {
            confirmPopupMenu.ActivateMenu("Starting a New Game with this slot will override the currently saved data. Are you sure?",
                // function to execute if we select 'yes'
                () =>
                {
                    DataPersistenceManager.Instance.DeleteProfileData(saveSlot.GetProfileId());
                    DataPersistenceManager.Instance.ChangeSelectedProfileID(saveSlot.GetProfileId());
                    DataPersistenceManager.Instance.NewGame();
                    DataPersistenceManager.Instance.SaveGame(DataPersistenceManager.Instance.autoSaveID);
                    DataPersistenceManager.Instance.saveID = DataPersistenceManager.Instance.autoSaveID;
                    DataPersistenceManager.Instance.saveNumID = "0";
                    SceneManager.LoadSceneAsync("OverWorld");
                },
                // function to execute if we select 'Cancel'
                () =>
                {
                    this.ActivateMenu(isLoadingGame);
                });
        }
        // case - new game, and the saveslot has no data
        else
        {
            DataPersistenceManager.Instance.ChangeSelectedProfileID(saveSlot.GetProfileId());
            DataPersistenceManager.Instance.NewGame();
            DataPersistenceManager.Instance.SaveGame(DataPersistenceManager.Instance.autoSaveID);
            DataPersistenceManager.Instance.saveID = DataPersistenceManager.Instance.autoSaveID;
            DataPersistenceManager.Instance.saveNumID = "0";
            SceneManager.LoadSceneAsync("OverWorld");
        }
    }

    private void SaveGameAndLoadScene()
    {
        /*
        // save the game anytime before Loading a new Scene
        DataPersistenceManager.Instance.SaveGame(0, DataPersistenceManager.Instance.playerID);
        DataPersistenceManager.Instance.SaveGame(DataPersistenceManager.Instance.autoSaveID, DataPersistenceManager.Instance.playerID);
        */
        // Load the Scene -  Which will inturn save the game Because of OnSceneUnloaded() in the DataPersistenceManager
        SceneManager.LoadSceneAsync("Game");
    }

    public void OnClearClicked(SaveSlot saveSlot)
    {
        DisableAllMenuButtons();

        confirmPopupMenu.ActivateMenu("Starting a New Game with this slot will override the currently saved data. Are you sure?",
        // function to execute if we select 'yes'
        () =>
        {
            DataPersistenceManager.Instance.DeleteProfileData(saveSlot.GetProfileId());
            ActivateMenu(isLoadingGame);
        },
        // function to execute if we select 'Cancel'
        () =>
        {
            this.ActivateMenu(isLoadingGame);
        });
    }

    public void OnBackClicked()
    {
        mainmenu.ActivateMenu();
        this.DeactivateMenu();
    }

    public void ActivateMenu(bool isLoadingGame)
    {
        //sets the gameobject to be active
        this.gameObject.SetActive(true);

        // Set Mode
        this.isLoadingGame = isLoadingGame;

        // Load alll of the profiles that exist
        List<List<WorldMapData>> profilesGameData = DataPersistenceManager.Instance.GetAllProfilesMapData();

        //ensures the back button is enabled when we activate the menu
        BackButton.interactable = true;

        // Loop throguh each save slot in the UI and set the content Appropiately
        GameObject firstSelected = BackButton.gameObject;

        for (int i = 0; i < saveSlots.Length; i++)
        {

            WorldMapData profileData = null;

            for(int j = 0; j < profilesGameData.Count; j++)
            {
                if (saveSlots[i].GetProfileId() == profilesGameData[j][profilesGameData[j].Count - 1].selectedProfileId)
                {
                    profileData = profilesGameData[j][profilesGameData[j].Count - 1];
                }
            }

            saveSlots[i].SetData(profileData);
            if (profileData == null && isLoadingGame)
            {
                saveSlots[i].SetInteractable(false);
            }
            else
            {
                saveSlots[i].SetInteractable(true);
                if (firstSelected.Equals(BackButton.gameObject))
                {
                    firstSelected = saveSlots[i].gameObject;
                }
            }

            // Set the first Selected Button
            Button firstSelectedButton = firstSelected.GetComponent<Button>();
            this.SetFirstSelected(firstSelectedButton);
        }
    }

    public void DeactivateMenu()
    {
        this.gameObject.SetActive(false);
    }

    private void DisableAllMenuButtons()
    {
        foreach(SaveSlot saveSlot in saveSlots)
        {
            saveSlot.SetInteractable(false);
        }
        BackButton.interactable = false;
    }
}
