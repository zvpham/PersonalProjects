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
    private Vector2 newPosition = new Vector2(0.0f, 0.0f);

    public static Player Instance;

    private InputManager inputManager;
    private KeyBindings keybindings;
    private InventorySystem inventorySystem;

    //For use in On Load Function Only
    public List<int> soulSlotIndexes = new List<int>();
    public List<SoulItemSO> onLoadSouls = new List<SoulItemSO>();

    public ActionBar actionBar;

    public void Awake() 
    {
        if (Instance != null)
        {
            Debug.LogWarning("Found more than One Player in the Scence");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        index = 0;
        inputManager = InputManager.instance;
        keybindings = KeyBindings.instance;
        inventorySystem = InventorySystem.Instance;
        actionBar = ActionBar.Instance;

        ChangeStr(0);
        ChangeAgi(0);
        ChangeEnd(0);
        ChangeWis(0);
        ChangeInt(0);
        ChangeCha(0);

        health = 100;

        // Equip all souls from stored in save data
        for(int i  = 0; i < soulSlotIndexes.Count; i++)
        {
            inventorySystem.OnLoadEquipSoul(soulSlotIndexes[i], onLoadSouls[i]);
        }

        baseActionTemplate = Instantiate(baseActionTemplate);
        foreach (Action templateAction in baseActionTemplate.Actions)
        {
            baseActions.Add(Instantiate(templateAction));
        }

        originalSprite = GetComponent<SpriteRenderer>().sprite;

        gameManager = GameManager.instance;
        gameManager.grid.SetGridObject(gameObject.transform.position, this);

        if (gameManager.isNewSlate)
        {
            gameManager.speeds.Insert(0, this.quickness);
            gameManager.priority.Insert(0, (int)(this.quickness * gameManager.baseTurnTime));
            gameManager.scripts.Insert(0, this);

            // Mainly For Debugging Purposes if trying new souls attached to prefab
            foreach (SoulItemSO physicalSoul in physicalSouls)
            {
                if (physicalSoul != null)
                {
                    physicalSoul.AddPhysicalSoul(this);
                }
            }

            foreach (SoulItemSO mentalSoul in mentalSouls)
            {
                if (mentalSoul != null)
                {
                    mentalSoul.AddMentalSoul(this);
                }
            }
        }
        else
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
        }

        if (inventorySystem.initialItems.Count != 0)
        {
            UpdatePlayerActions();
        }


        enabled = false;
    }

    public void OnEnable()
    {
        OnTurnStart(); 
    }

    public void UpdatePlayerActions(Dictionary<int, InventoryItem> inventoryState = null)
    {
        foreach(SoulSlot soulSlot in inventorySystem.inventoryUI.soulSlots)
        {
            if(soulSlot.contentImage != null)
            {
                break;
            }
        }
        keybindings.actionkeyBinds.Clear();

        Debug.Log("Player Action Update");
        foreach (Action action in actions)
        {
            if (action.actionName == ActionName.Sprint)
            {
                Debug.Log("Player Action Update SPRINGINT");
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
            }
            else
            {
                inventorySystem.inventoryUI.Hide();
                notOnHold = true;
            }
        }
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

    public override void onTurnEndPlayer()
    {
        for (int i = 0; i < actions.Count; i++)
        {
            actionBar.UpdateCoolDowns(actions[i].actionName, actions[i].currentCooldown);
        }
        actionBar.UpdateActionsDisplay();
    }
}
