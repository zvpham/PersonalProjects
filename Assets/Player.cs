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

    //private GameMana  r gameManager;
    private InputManager inputManager;
    private KeyBindings keybindings;

    [SerializeField]
    private UIInventoryPage inventoryUI;

    [SerializeField]
    private InventorySO inventoryData;

    public List<InventoryItem> initialItems = new List<InventoryItem>();

    [SerializeField]
    private AudioClip dropClip;

    [SerializeField]
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        PrepareUI();
        PrepareInventoryData();

        originalSprite = GetComponent<SpriteRenderer>().sprite;

        gameManager = GameManager.instance;
        gameManager.speeds.Insert(0, this.quickness);
        gameManager.priority.Insert(0, (int)(this.quickness * gameManager.baseTurnTime));
        gameManager.scripts.Insert(0, this);
        gameManager.locations.Insert(0, transform.position);


        index = 0;
        inputManager = InputManager.instance;
        keybindings = KeyBindings.instance;
        Debug.Log("Player Start");

        if (initialItems.Count != 0)
        {
            UpdatePlayerActions();
        }


        enabled = false;
    }

    public void UpdatePlayerActions(Dictionary<int, InventoryItem> inventoryState = null)
    {

        if (inventoryData.inventoryItems.Count != 0)
        {
            keybindings.actionkeyBinds.Clear();
            foreach (InventoryItem space in inventoryData.inventoryItems)
            {
                if (space.item != null)
                {
                    SoulItemSO soul = (SoulItemSO)space.item;
                    soul.AddPhysicalSoul(this);
                    soul.AddMentalSoul(this);
                }
            }
            foreach (Action action in actions)
            {
                if (action.actionName == ActionName.Sprint)
                {
                    keybindings.actionkeyBinds.Add(action.actionName, new List<KeyCode>() { KeyCode.Keypad0 });
                }
                if (action.actionName == ActionName.Jump)
                {
                    keybindings.actionkeyBinds.Add(action.actionName, new List<KeyCode>() { KeyCode.Space });
                }
            }
        }

    }

    private void PrepareInventoryData()
    {
        inventoryData.Initialize();
        inventoryData.OnInventoryUpdated += UpdateInventoryUI;
        inventoryData.OnInventoryUpdated += UpdatePlayerActions;
        foreach (InventoryItem item in initialItems)
        {
            if (item.isEmpty)
            {
                continue;
            }
            inventoryData.AddItem(item);
        }
    }

    private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
    {
        inventoryUI.ResetAllItems();
        foreach (var item in inventoryState)
        {
            inventoryUI.UpdateData(item.Key, item.Value.item.itemImage, item.Value.quantity);
        }
    }

    private void PrepareUI()
    {
        inventoryUI.InitializeInventoryUI(inventoryData.Size);
        this.inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
        this.inventoryUI.OnSwapItems += HandleSwapItems;
        this.inventoryUI.OnStartDragging += HandleDragging;
        this.inventoryUI.OnItemActionRequested += HandleItemActionRequest;
    }

    private void HandleItemActionRequest(int itemIndex)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
        if (inventoryItem.isEmpty)
        {
            return;
        }
        IItemAction itemAction = inventoryItem.item as IItemAction;
        if (itemAction != null)
        {
            inventoryUI.ShowItemAction(itemIndex);
            inventoryUI.AddAction(itemAction.ActionName, () => PerformAction(itemIndex));
        }
        IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
        if (destroyableItem != null)
        {
            inventoryUI.AddAction("Drop", () => DropItem(itemIndex, inventoryItem.quantity));
        }
    }

    private void DropItem(int itemIndex, int quantity)
    {
        inventoryData.RemoveItem(itemIndex, quantity);
        inventoryUI.ResetSelection();
        //audioSource.PlayOneShot(dropClip);
    }

    public void PerformAction(int itemIndex)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
        if (inventoryItem.isEmpty)
        {
            return;
        }
        IDestroyableItem destroyableItem = inventoryItem.item as IDestroyableItem;
        if (destroyableItem != null)
        {
            inventoryData.RemoveItem(itemIndex, 1);
        }
        IItemAction itemAction = inventoryItem.item as IItemAction;
        if (itemAction != null)
        {
            itemAction.PerformAction(this.gameObject, inventoryItem.itemState);
            if (inventoryData.GetItemAt(itemIndex).isEmpty)
            {
                inventoryUI.ResetSelection();
            }
            //audioSource.PlayOneShot(itemAction.actionSFX);
        }
    }

    private void HandleDragging(int itemIndex)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
        if (inventoryItem.isEmpty)
        {
            return;
        }
        inventoryUI.CreateDraggedItem(inventoryItem.item.itemImage, inventoryItem.quantity);
    }

    private void HandleSwapItems(int itemIndex1, int itemIndex2)
    {
        inventoryData.SwapItems(itemIndex1, itemIndex2);
    }

    private void HandleDescriptionRequest(int itemIndex)
    {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);
        if (inventoryItem.isEmpty)
        {
            inventoryUI.ResetSelection();
            return;
        }
        ItemSO item = inventoryItem.item;
        string description = PrepareDescription(inventoryItem);
        inventoryUI.UpdateDescription(itemIndex, item.itemImage, item.itemName, description);

    }

    private string PrepareDescription(InventoryItem inventoryItem)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(inventoryItem.item.description);
        sb.AppendLine();
        for (int i = 0; i < inventoryItem.itemState.Count; i++)
        {
            sb.Append($"{inventoryItem.itemState[i].itemParameter.ParameterName}" +
                $": {inventoryItem.itemState[i].value} / " +
                $"{inventoryItem.item.DefaultParameterList[i].value}");
        }
        return sb.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (notOnHold)
        {
            for (int i = 0; i < baseActions.Count; i++)
            {
                if (inputManager.GetKeyDownAction(baseActions[i].actionName))
                {
                    if (ContainsMatchingActionType(i))
                    {
                        break;
                    }
                    if (baseActions[i].startActionPresets())
                    {
                        baseActions[i].PlayerActivate(this);
                        TurnEnd();
                    }
                }
            }

            for (int i = 0; i < actions.Count; i++)
            {
                if (inputManager.GetKeyDownAction(actions[i].actionName))
                {
                    if (ContainsMatchingActionType(i))
                    {
                        break;
                    }
                    if (actions[i].startActionPresets())
                    {
                        actions[i].PlayerActivate(this);
                        TurnEnd();
                    }
                }
            }
            if (inputManager.GetKeyDown(ActionName.InventoryMenu))
            {
                if (inventoryUI.isActiveAndEnabled == false)
                {
                    inventoryUI.Show();
                    foreach (var item in inventoryData.GetCurrentInventoryState())
                    {
                        inventoryUI.UpdateData(item.Key,
                            item.Value.item.itemImage,
                            item.Value.quantity);
                    }
                    notOnHold = false;
                }
                else
                {
                    inventoryUI.Hide();
                    notOnHold = true;
                }
            }

        }
    }
    public bool ContainsMatchingActionType(int i)
    {
        if (baseActions[i].actionType.Length != 0 && unusableActionTypes.Count > 0)
        {
            foreach (ActionTypes actionType in baseActions[i].actionType)
            {
                if (unusableActionTypes.ContainsKey(actionType))
                {
                    //Debug.Log("Can't Use Action" + baseActions[i].actionName.ToString());
                    return true;
                }
            }
        }
        //Debug.Log("Can Use Action" + baseActions[i].actionName.ToString());
        return false;
    }
}
