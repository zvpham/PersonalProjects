using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class OverWorldMenu : MonoBehaviour
{
    public InventorySystem inventorySystem;
    public MissionSelectSystem missionSelectSystem;
    public MissionStartSystem missionStartSystem;
    public OverworldGameManager gameManager;

    public UnityAction ChangeMenu;
    public void OnInventoryButtonClicked()
    {
        ChangeMenu?.Invoke();
        inventorySystem.OpenMenu();
    }

    public void OnMissionButtonClicked()
    {
        ChangeMenu?.Invoke();
        missionSelectSystem.OpenMenu();
    }

    public void OnAutoSaveClicked()
    {
        gameManager.MakeAutoSave();
    }

    public void OnMissionStartButtonClicked()
    {
        ChangeMenu?.Invoke();
        missionStartSystem.OpenMenu();
    }

    public void OnLaunchMissionButtonClicked()
    {
        gameManager.MakeAutoSave();
        gameManager.OpenCombatScene();
    }

    public void CloseMenu()
    {
        ChangeMenu?.Invoke();
    }
}
