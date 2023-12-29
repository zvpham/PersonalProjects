using Inventory.Model;
using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [SerializeField]
    public UIInventoryPage inventoryUI;

    [SerializeField]
    public InventorySO inventoryData;

    public List<InventoryItem> initialItems = new List<InventoryItem>();

    [SerializeField]
    public AudioClip dropClip;

    [SerializeField]
    public AudioSource audioSource;

    public Player player;

    public static InventorySystem Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Found more than ONe Data Persistence Manager in the Scence");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        player = Player.Instance;
        inventoryUI = UIInventoryPage.Instance;
        PrepareUI();
        PrepareInventoryData();
        PrepareSoulSlotData();
    }

    public void PrepareSoulSlotData()
    {
        foreach(SoulSlot soulSlot in inventoryUI.soulSlots)
        {
            soulSlot.soulSlotIndex = inventoryUI.soulSlots.IndexOf(soulSlot);
        }
    }

    public void PrepareInventoryData()
    {
        player = Player.Instance;
        inventoryUI = UIInventoryPage.Instance;
        inventoryData.Initialize();
        inventoryData.OnInventoryUpdated += UpdateInventoryUI;
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
        this.inventoryUI.OnEquipSoul += HandleEquipSoul;
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

    private void HandleEquipSoul(int itemIndex, SoulSlot soulSlot)
    {
        soulSlot.AddSoul((SoulItemSO)inventoryData.GetItemAt(itemIndex).item, player);
        inventoryData.RemoveItem(itemIndex, 1);
        player.UpdatePlayerActions();
    }

    public void OnLoadEquipSoul(int soulSlotIndex, SoulItemSO soul)
    {
        player = Player.Instance;
        inventoryUI = UIInventoryPage.Instance;
        inventoryUI.soulSlots[soulSlotIndex].AddSoul(soul, player, true);
        player.UpdatePlayerActions();
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
}
