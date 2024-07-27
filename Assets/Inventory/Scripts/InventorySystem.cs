using Inventory.Model;
using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [SerializeField]
    public OverWorldMenu overWorldMenu;

    [SerializeField]
    public UIInventoryPage inventoryUI;

    [SerializeField]
    public InventorySO inventoryData;

    public bool dispalyOnly = false;

    public List<InventoryItem> initialItems = new List<InventoryItem>();

    [SerializeField]
    public AudioClip dropClip;

    [SerializeField]
    public AudioSource audioSource;

    [SerializeField]
    public CharacterSystem characterSystem;

    public Unit currentUnit;

    void Start()
    {
        PrepareUI();
        PrepareInventoryData();
        PrepareEquipSlotData();
        PrepareCharacterSystem();
    }

    public void PrepareCharacterSystem()
    {
        characterSystem.OnUnitClicked += HandleNewUnitSelected;
        characterSystem.OnChangedUnitCategory += HandleOnUnitCategoryChanged;
    }

    public void PrepareEquipSlotData()
    {
        foreach (EquipSlot equipSlot in inventoryUI.equipSlots)
        {
            equipSlot.EquipSlotIndex = inventoryUI.equipSlots.IndexOf(equipSlot);
        }
    }

    public void PrepareInventoryData()
    {
        if(!dispalyOnly)
        {
            inventoryData.Initialize();
            inventoryData.OnInventoryUpdated += UpdateInventoryUI;
        }
    }

    private void PrepareUI()
    {
        inventoryUI.InitializeInventoryUI();
        this.inventoryUI.OnSwapItems += HandleSwapItems;
        this.inventoryUI.OnStartDragging += HandleDragging;
        this.inventoryUI.OnEquipItem += HandleEquipItem;
        this.inventoryUI.OnUnequipItem += HandleUnequipItem;
        this.inventoryUI.OnProfileClicked += HandleProfileClicked;
    }

    public void LoadInitialItems()
    {
        if (!dispalyOnly)
        {
            foreach (InventoryItem item in initialItems)
            {
                if (item.isEmpty)
                {
                    continue;
                }
                inventoryData.AddItem(item);
            }
        }
    }

    public void AddItem(ItemSO item, int itemQuantity)
    {
        if (!dispalyOnly)
        {
            InventoryItem newItem = new InventoryItem();
            newItem.item = item;
            newItem.ChangeQuantity(itemQuantity);
            inventoryData.AddItem(newItem);
        }
    }

    public void OpenMenu()
    {
        inventoryUI.gameObject.SetActive(true);
        overWorldMenu.ChangeMenu += CloseMenu;
        characterSystem.OnOpenMenu();
    }

    public void CloseMenu()
    {
        inventoryUI.gameObject.SetActive(false);
        overWorldMenu.ChangeMenu -= CloseMenu;
    }

    public void HandleNewUnitSelected(Unit unit)
    {
        foreach (EquipSlot equipSlot in inventoryUI.equipSlots)
        {
            equipSlot.ClearItem();
        }

        currentUnit = unit;

        if (unit == null)
        {
            return;
        }

        if (unit.helmet != null)
        {
            inventoryUI.ItemEquip(unit.helmet, inventoryUI.equipSlots[0]);
        }
        if (unit.armor != null)
        {
            inventoryUI.ItemEquip(unit.armor, inventoryUI.equipSlots[1]);
        }
        if (unit.legs != null)
        {
            inventoryUI.ItemEquip(unit.legs, inventoryUI.equipSlots[2]);
        }
        if (unit.mainHand != null)
        {
            inventoryUI.ItemEquip(unit.mainHand, inventoryUI.equipSlots[3]);
        }
        if (unit.offHand != null)
        {
            inventoryUI.ItemEquip(unit.offHand, inventoryUI.equipSlots[4]);
        }
        if (unit.Item1 != null)
        {
            inventoryUI.ItemEquip(unit.Item1, inventoryUI.equipSlots[5]);
        }
        if (unit.Item2 != null)
        {
            inventoryUI.ItemEquip(unit.Item2, inventoryUI.equipSlots[6]);
        }
        if (unit.Item3 != null)
        {
            inventoryUI.ItemEquip(unit.Item3, inventoryUI.equipSlots[7]);
        }
        if (unit.Item4 != null)
        {
            inventoryUI.ItemEquip(unit.Item4, inventoryUI.equipSlots[8]);
        }
        inventoryUI.UpdateWeight(currentUnit.currentWeight, currentUnit.lowWeight, currentUnit.mediumWeight, currentUnit.highWeight);
    }

    private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
    {
        inventoryUI.ResetAllItems();
        foreach (var item in inventoryState)
        {
            EquipableItemSO equipData = (EquipableItemSO)item.Value.item;
            inventoryUI.UpdateData(item.Key, equipData.itemImage, item.Value.quantity, equipData.name, equipData.attributeOne, equipData.attributeTwo, equipData.mainCategoryOne, StatToString(equipData.mainOneMin, equipData.mainOneMax), equipData.mainCategoryTwo,
                StatToString(equipData.mainTwoMin, equipData.mainTwoMax), equipData.mainCategoryThree, StatToString(equipData.mainThreeMin, equipData.mainThreeMax));
        }
    }

    private string StatToString(int min, int max)
    {
        if (max == 0)
        {
            return min.ToString();
        }
        else
        {
            return min.ToString() + "-" + max.ToString();
        }
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
        inventoryUI.CreateDraggedItem(inventoryItem.item.itemImage);
    }

    private void HandleSwapItems(int itemIndex1, int itemIndex2)
    {
        inventoryData.SwapItems(itemIndex1, itemIndex2);
    }

    private void HandleProfileClicked(EquipableItemSO item, EquipSlot equipSlot)
    {
        if (currentUnit == null || item == null)
        {
            return;
        }

        if(item.equipType == EquipType.Accessory && (equipSlot.equipType == EquipType.Accessory1 || equipSlot.equipType == EquipType.Accessory2 || 
            equipSlot.equipType == EquipType.Accessory3 || equipSlot.equipType == EquipType.Accessory4))
        {
            equipSlot.currentItem = item;
            inventoryUI.ConfirmItemEquip(item, equipSlot);
        }
        else if(item.equipType == EquipType.BothHands && equipSlot.equipType == EquipType.MainHand)
        {
            EquipSlot mainHandEquipSlot = inventoryUI.equipSlots[3];
            EquipSlot offHandEquipSlot = inventoryUI.equipSlots[4];

            mainHandEquipSlot.currentItem = item;

            inventoryUI.ConfirmItemEquip(item, mainHandEquipSlot);
            inventoryUI.ConfirmItemEquip(item, offHandEquipSlot);
        }
        else if (item.equipType == equipSlot.equipType)
        {
            equipSlot.currentItem = item;
            inventoryUI.ConfirmItemEquip(item, equipSlot);
        }
        else
        {
            Debug.LogError("This Shouldn't happen: On Profile Clicked item doesn't match equip slot type when redisplaying Item UI");
        }
    }


    private void HandleEquipItem(int itemIndex, EquipSlot equipSlot)
    {
        if (currentUnit == null || !characterSystem.manageHeroes)
        {
            return;
        }

        EquipableItemSO newItem = (EquipableItemSO)inventoryData.GetItemAt(itemIndex).item;

        if (newItem.equipType == EquipType.BothHands && (equipSlot.equipType == EquipType.MainHand || equipSlot.equipType == EquipType.OffHand))
        {
            EquipSlot mainHandEquipSlot = inventoryUI.equipSlots[3];
            EquipSlot offHandEquipSlot = inventoryUI.equipSlots[4];

            if (mainHandEquipSlot != null && mainHandEquipSlot.currentItem != null)
            {
                HandleUnequipItem(mainHandEquipSlot);
            }

            if (offHandEquipSlot != null && offHandEquipSlot.currentItem != null)
            {
                HandleUnequipItem(offHandEquipSlot);
            }
            mainHandEquipSlot.AddItem(newItem, currentUnit);    
            inventoryUI.ConfirmItemEquip(mainHandEquipSlot);
            inventoryUI.ConfirmItemEquip(offHandEquipSlot);
            offHandEquipSlot.currentItem = newItem;
            inventoryData.RemoveItem(itemIndex, 1);
        }
        else if(newItem.equipType == EquipType.Accessory && (equipSlot.equipType == EquipType.Accessory1 || equipSlot.equipType == EquipType.Accessory2 || equipSlot.equipType == EquipType.Accessory3 || equipSlot.equipType == EquipType.Accessory4))
        {
            equipSlot.AddItem(newItem, currentUnit);
            inventoryUI.ConfirmItemEquip(equipSlot);
            inventoryData.RemoveItem(itemIndex, 1);
        }
        else if (newItem.equipType == equipSlot.equipType)
        {

            EquipSlot mainHandEquipSlot = inventoryUI.equipSlots[3];
            if (newItem.equipType == EquipType.OffHand && mainHandEquipSlot.currentItem != null && mainHandEquipSlot.currentItem.equipType == EquipType.BothHands)
            {
                HandleUnequipItem(mainHandEquipSlot);
            }

            if (equipSlot.currentItem != null)
            {
                HandleUnequipItem(equipSlot);
            }

            equipSlot.AddItem(newItem, currentUnit);
            inventoryUI.ConfirmItemEquip(equipSlot);
            inventoryData.RemoveItem(itemIndex, 1);
        }
        inventoryUI.UpdateWeight(currentUnit.currentWeight, currentUnit.lowWeight, currentUnit.mediumWeight, currentUnit.highWeight);
    }

    private void HandleUnequipItem(EquipSlot equipSlot)
    {
        if (!characterSystem.manageHeroes)
        {
            return;
        }

        inventoryData.AddItem(equipSlot.currentItem, 1);
        if (equipSlot.currentItem.equipType ==  EquipType.BothHands)
        {
            inventoryUI.equipSlots[3].ClearItem();
            inventoryUI.equipSlots[4].ClearItem();
        }
        equipSlot.RemoveItem(currentUnit);
        inventoryUI.UpdateWeight(currentUnit.currentWeight, currentUnit.lowWeight, currentUnit.mediumWeight, currentUnit.highWeight);
    }

    private void HandleOnUnitCategoryChanged(bool heroButtonPressed)
    {
        for(int i = 0; i < inventoryUI.equipSlots.Count; i++)
        {
            inventoryUI.equipSlots[i].disabledDueToMercenary = !heroButtonPressed;
        }
    }

    /*
    public void OnLoadEquipSoul(int soulSlotIndex, SoulItemSO soul)
    {
        player = Player.Instance;
        inventoryUI = UIInventoryPage.Instance;
        inventoryUI.soulSlots[soulSlotIndex].AddSoul(soul, player, true);
        player.UpdatePlayerActions();
    }
    */
}
