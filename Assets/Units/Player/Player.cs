using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Inventory.UI;
using Inventory.Model;
using UnityEngine.Events;
using Unity.VisualScripting;
using System.Text;
using System.Linq;

public class Player : Unit
{
    public static Player Instance;

    private InputManager inputManager;
    private KeyBindings keybindings;
    public InventorySystem inventorySystem;
    public GameMenu gameMenu;  
    public WorldMapTravel worldMapTravel;
    public CameraManager cameraManager;
    public CameraControllerPlayer gameCamera;
    public VirtualCursor virtualCursor;
    public List<InventoryItem> initialInventoryItemsForMapManager = new List<InventoryItem>();

    public int amountOfMenusOpen;
    public LevelingSystem levelingSystem;
    public int availableClassLevelUps = 1;
    public int maxCommonClasses;
    public int maxUncommonClasses;
    public int maxRareClasses;
    public int maxRacialClasses = 1;

    public ActionBar actionBar;

    public void Awake() 
    {
        Instance = this;
        OnDeath += OnPlayerDeath;
    }

    void Start()
    {
        index = 0;
        inputManager = InputManager.instance;
        keybindings = KeyBindings.instance;
        gameMenu = GameMenu.Instance;
        inventorySystem = InventorySystem.Instance;
        worldMapTravel = WorldMapTravel.Instance;
        actionBar = ActionBar.Instance;
        cameraManager = CameraManager.Instance;
        virtualCursor = VirtualCursor.Instance;
        virtualCursor.enabled = false;

        actionBar.player = this;
        gameMenu.player = this;

        gameCamera = cameraManager.gameCamera.GetComponent<CameraControllerPlayer>();
        gameCamera.target = gameObject;
        gameCamera.attachedToTarget = true;
        PerformedAction += ResetCameraPosition;
        ResetCameraPosition(null, ActionName.Wait);

        ChangeStr(0);
        ChangeAgi(0);
        ChangeEnd(0);
        ChangeWis(0);
        ChangeInt(0);
        ChangeCha(0);

        health = 100;
        /*
        // Equip all souls from stored in save data
        for(int i  = 0; i < soulSlotIndexes.Count; i++)
        {
            inventorySystem.OnLoadEquipSoul(soulSlotIndexes[i], onLoadSouls[i]);
        }
        */

        for(int i = 0; i < classes.Count; i++)
        {
            gameMenu.classPage.AddClass(classes[i]);
            for(int j = 0; j < classes[i].currentLevel; j++)
            {
                gameMenu.classPage.UILevelUpClass(classes[i]);
            }
        }

        baseActionTemplate = Instantiate(baseActionTemplate);
        foreach (Action templateAction in baseActionTemplate.Actions)
        {
            baseActions.Add(Instantiate(templateAction));
        }

        foreach (Sense templateSense in baseActionTemplate.Senses)
        {
            senses.Add(Instantiate(templateSense));
        }

        defaultMeleeDamage = baseActionTemplate.DefaultMelee;

        LoadClassesOnStart();

        originalSprite = GetComponent<SpriteRenderer>().sprite;

        //Placing Player on outer edge when entering map from world Map;
        if (gameManager.mainGameManger.mapManager.enteredTileThroughWorldMap)
        {
            int x = gameManager.mainGameManger.mapWidth / 2;
            int y = 0;
            Vector3 startPosition = new Vector3(0.5f, 0.5f, 0);
            bool foundStartingPosition = false;

            //Starting At Bottom Checking Places to right of bottom of screen for Empty Place
            for(int i = x; i < gameManager.mainGameManger.mapWidth; i++)
            {
                if(gameManager.obstacleGrid.GetGridObject(x,y) == null)
                {
                    Debug.Log(new Vector3(x, y, 0));
                    startPosition += gameManager.obstacleGrid.GetWorldPosition(x, y);
                    Debug.Log(startPosition);
                    foundStartingPosition = true;
                    break;
                }
            }
            //Starting At Bottom Checking Places to left of bottom of screen for Empty Place
            if(!foundStartingPosition)
            {
                for (int i = (gameManager.mainGameManger.mapWidth / 2) - 1; i > 0; i--)
                {
                    if (gameManager.obstacleGrid.GetGridObject(x, y) == null)
                    {
                        startPosition += gameManager.obstacleGrid.GetWorldPosition(x, y);
                        foundStartingPosition = true;
                        break;
                    }
                }
            }

            //Checking Left Side for Empty Space
            if (!foundStartingPosition)
            {
                for (int i = 0; i < gameManager.mainGameManger.mapHeight; i++)
                {
                    if (gameManager.obstacleGrid.GetGridObject(x, y) == null)
                    {
                        startPosition += gameManager.obstacleGrid.GetWorldPosition(x, y);
                        foundStartingPosition = true;
                        break;
                    }
                }
            }

            //Checking Top Side for Empty Space
            if (!foundStartingPosition)
            {
                for (int i = 0; i < gameManager.mainGameManger.mapWidth; i++)
                {
                    if (gameManager.obstacleGrid.GetGridObject(x, y) == null)
                    {
                        startPosition += gameManager.obstacleGrid.GetWorldPosition(x, y);
                        foundStartingPosition = true;
                        break;
                    }
                }
            }

            //Checking Right Side for Empty Space
            if (!foundStartingPosition)
            {
                for (int i = gameManager.mainGameManger.mapHeight - 1; i > 0; i--)
                {
                    if (gameManager.obstacleGrid.GetGridObject(x, y) == null)
                    {
                        startPosition += gameManager.obstacleGrid.GetWorldPosition(x, y);
                        foundStartingPosition = true;
                        break;
                    }
                }
            }

            if (foundStartingPosition)
            {
                gameObject.transform.position = startPosition;
            }
            else
            {
                Debug.LogError("Couldn't Find a start position");
            }
            gameManager.mainGameManger.mapManager.enteredTileThroughWorldMap = false;
        }

        gameManager.ChangeUnits(gameObject.transform.position, this, flyOnLoad);

        if (!gameManager.isNewSlate)
        {
            for (int i = 0; i < actions.Count; i++)
            {
                int index = actionNamesForCoolDownOnLoad.IndexOf(actions[i].actionName);
                if (index == -1)
                {
                    actions.RemoveAt(i);
                    i--;
                    continue;
                }
                actions[i].currentCooldown = currentCooldownOnLoad[index];
            }

            for (int i = 0; i < actionsThatHaveActiveStatus.Count; i++)
            {
                if (actionsThatHaveActiveStatus[i].x == -1)
                {
                    continue;
                }
                actions[(int)actionsThatHaveActiveStatus[i].x].actionIsActive = true;
                statuses[(int)actionsThatHaveActiveStatus[i].y].activeAction = actions[(int)actionsThatHaveActiveStatus[i].x];
            }
        }
        enabled = false;
        UpdatePlayerActions();
    }

    public void UpdatePlayerActions()
    {
        foreach(SoulSlot soulSlot in inventorySystem.inventoryUI.soulSlots)
        {
            if(soulSlot.contentImage != null)
            {
                break;
            }
        }
        keybindings.actionkeyBinds.Clear();

        foreach (Action action in actions)
        {
            if (action.actionName == ActionName.Sprint)
            {
                keybindings.actionkeyBinds.Add(action.actionName, new List<KeyCode>() { KeyCode.S});
            }
            if (action.actionName == ActionName.Jump)
            {
                keybindings.actionkeyBinds.Add(action.actionName, new List<KeyCode>() { KeyCode.Space });
            }
            if(action.actionName == ActionName.SlowTimeField)
            {
                keybindings.actionkeyBinds.Add(action.actionName, new List<KeyCode>() { KeyCode.T });
            }
            if (action.actionName == ActionName.FlameBreath)
            {
                keybindings.actionkeyBinds.Add(action.actionName, new List<KeyCode>() { KeyCode.F });
            }
            if(action.actionName == ActionName.SeeTheFuture)
            {
                keybindings.actionkeyBinds.Add(action.actionName, new List<KeyCode>() { KeyCode.G });
            }
            if (action.actionName == ActionName.Bonk)
            {
                keybindings.actionkeyBinds.Add(action.actionName, new List<KeyCode>() { KeyCode.B });
            }

            actionBar.AddAction(action.actionName, action.currentCooldown);
        }
        actionBar.UpdateActionsDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        if (notOnHold)
        {
            for (int i = 0; i < baseActions.Count; i++)
            {
                if (inputManager.GetKeyDown(baseActions[i].actionName))
                {
                    if (ContainsMatchingUnusableActionType(i, true))
                    {
                        break;
                    }
                    if (baseActions[i].startActionPresets(this))
                    {
                        baseActions[i].PlayerActivate(this);
                    }
                }
            }

            for (int i = 0; i < actions.Count; i++)
            {
                if (inputManager.GetKeyDown(actions[i].actionName))
                {
                    if (ContainsMatchingUnusableActionType(i, false))
                    {
                        break;
                    }
                    if (actions[i].startActionPresets(this))
                    {
                        actions[i].PlayerActivate(this);
                    }
                }
            }
        }
        if(!gameMenu.gameObject.activeInHierarchy)
        {
            if (inputManager.GetKeyDown(ActionName.InventoryMenu))
            {
                if (inventorySystem.inventoryUI.isActiveAndEnabled == false)
                {
                    inventorySystem.inventoryUI.Show();
                    foreach (var item in inventorySystem.inventoryData.GetCurrentInventoryState())
                    {
                        inventorySystem.inventoryUI.UpdateData(item.Key,
                            item.Value.item.itemImage,
                            item.Value.quantity);
                    }
                    notOnHold = false;
                    ChangeAmountOfMenusOpen(1);
                }
                else
                {
                    inventorySystem.inventoryUI.Hide();
                    ChangeAmountOfMenusOpen(0);
                }
            }

            if (inputManager.GetKeyDown(ActionName.ClassMenu))
            {
                ChangeAmountOfMenusOpen(1);
                virtualCursor.enabled = true;
                gameMenu.OpenMenu(gameMenu.pages.IndexOf(gameMenu.classPage));
                actionBar.gameObject.SetActive(false);
            }
        }

        if (inputManager.GetKeyDown(ActionName.OpenWorldMap))
        {
            if(forcedMovement == null)
            {
                gameMenu.CloseMenu();
                UseWorldMap();
            }
            else
            {
                Debug.Log("Can't Open World Map when moving");
            }
        }
    }

    public void CloseGameMenu()
    {
        virtualCursor.enabled = false;
        actionBar.gameObject.SetActive(true);
        ChangeAmountOfMenusOpen(-1);
    }

    public void ChangeAmountOfMenusOpen(int screenChange)
    {
        amountOfMenusOpen += screenChange;
        if (amountOfMenusOpen == 0)
        {
            notOnHold = true;
        }
        else
        {
            notOnHold = false;
        }
    }

    public void UseWorldMap()
    {
        if (cameraManager.worldMapCamera.isActiveAndEnabled == false)
        {
            cameraManager.gameCamera.gameObject.SetActive(false);
            cameraManager.worldMapCamera.gameObject.SetActive(true);
            cameraManager.worldMapCamera.enabled = true;
            worldMapTravel.StartWorldMapTravel(originalSprite);
            notOnHold = false;
            ChangeAmountOfMenusOpen(1);
        }
        else
        {
            cameraManager.worldMapCamera.gameObject.SetActive(false);
            cameraManager.gameCamera.gameObject.SetActive(true);
            cameraManager.worldMapCamera.enabled = false;
            worldMapTravel.EndWorldMapTravel();
            ChangeAmountOfMenusOpen(-1);
        }
    }

    public override void UnitMovement(Vector3 originalPosition, Vector3 newPosition, bool FlyAtOrigin, bool FlyAtDestination)
    {
        base.UnitMovement(originalPosition, newPosition, FlyAtOrigin, FlyAtDestination);
        PlayerUseSenses();
    }

    public void PlayerUseSenses()
    {
        List<Vector2Int> tempTilesBeingActivelySeen = gameManager.tilesBeingActivelySeen;
        for(int i = 0; i < tempTilesBeingActivelySeen.Count; i++)
        {
            gameManager.tileVisibilityStates[(int) (tempTilesBeingActivelySeen[i].x - gameManager.defaultGridPosition.x),
                (int)(tempTilesBeingActivelySeen[i].y - gameManager.defaultGridPosition.y)] = 1;
            gameManager.spriteGrid.GetGridObject(new Vector3(tempTilesBeingActivelySeen[i].x, tempTilesBeingActivelySeen[i].y, 0))
                .sprites[0].color =  new Color(0, 0, 0, gameManager.VisitedSpaceAlpha);
        }
        gameManager.tilesBeingActivelySeen = new List<Vector2Int>();

        for(int i = 0; i < senses.Count; i++)
        {
            senses[i].PlayerUseSense(this);
        }
    }

    public void ResetCameraPosition(ActionTypes[] actionTypes, ActionName actionName)
    {
        gameCamera.MoveCamera();
    }

    public override void ActivateTargeting()
    {
        base.ActivateTargeting();
        actionBar.gameObject.SetActive(false);
    }

    public override void DeactivateTargeting()
    {
        base.DeactivateTargeting();
        actionBar.gameObject.SetActive(true);
    }

    public void OnActionButtonPressed(ActionName actionName)
    {
        int actionUsedIndex = -1;
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i].actionName.Equals(actionName))
            { 
                if (ContainsMatchingUnusableActionType(i, false))
                {
                    break;
                }
                if (actions[i].startActionPresets(this))
                {
                    actionUsedIndex = i;
                    actions[i].PlayerActivate(this);
                }
            }
        }
        if(actionUsedIndex == -1)
        {
            return;
        }
        actionBar.UpdateCoolDowns(actionName, actions[actionUsedIndex].currentCooldown);
        actionBar.UpdateActionsDisplay();
    }

    public void AquireRandomRacialClass()
    {

    }

    public void AquireRandomJobClass()
    {

    }

    public override void TurnEnd()
    {
        base.TurnEnd();
        for (int i = 0; i < actions.Count; i++)
        {
            actionBar.UpdateCoolDowns(actions[i].actionName, actions[i].currentCooldown);
        }
        actionBar.UpdateActionsDisplay();
        gameManager.mainGameManger.mapManager.UpdatePreviousPositions();
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
    }

    public void OnPlayerDeath()
    {
        Instance = null;
        InventorySystem.Instance = null;
        OnDeath -= OnPlayerDeath;
    }

    public void GainXP(int XP)
    {   
        levelingSystem.GainXP(this, XP);
    }
}
