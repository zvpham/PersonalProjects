using Inventory.Model;
using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using UnityEditorInternal.Profiling.Memory.Experimental;
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

    [SerializeField]
    public ResourceManager resourceManager;

    public Unit currentUnit;

    public void Awake()
    {
        if (!dispalyOnly)
        {
            initialItems = new List<InventoryItem>();
            for (int i = 0; i < resourceManager.allItems.Count; i++)
            {
                InventoryItem newItem = new InventoryItem();
                newItem.item = resourceManager.allItems[i];
                newItem.quantity = 10;

                initialItems.Add(newItem);
            }
        }
    }

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
        this.inventoryUI.OnStartDragging += HandleDragging;
        this.inventoryUI.OnEquipItem += HandleEquipItem;
        this.inventoryUI.OnUnequipItem += HandleUnequipItem;
        this.inventoryUI.OnSwapEquipSlot += HandleSwapEquipSlot;
        this.inventoryUI.OnSwapHands += HandleSwapHands;
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
            newItem.quantity = itemQuantity;
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

        inventoryUI.backUpMainHand.ClearItem();
        inventoryUI.backUpOffHand.ClearItem();

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
        if (unit.backUpMainHand != null)
        {
            inventoryUI.ItemEquip(unit.backUpMainHand, inventoryUI.backUpMainHand);
        }
        if (unit.backUpOffHand != null)
        {
            inventoryUI.ItemEquip(unit.backUpOffHand, inventoryUI.backUpOffHand);
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
        inventoryUI.UpdateWeightSecondary(currentUnit.backUpWeight, currentUnit.lowWeight, currentUnit.mediumWeight, currentUnit.highWeight);
    }

    private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
    {
        inventoryUI.ResetAllItems(inventoryState);
        foreach (var item in inventoryState)
        {
            EquipableItemSO equipData = (EquipableItemSO)item.Value.item;
            string ammoExtension = "";
            if (equipData.GetType() == typeof(EquipableAmmoSO))
            {
                ammoExtension = "X";
            }

                inventoryUI.UpdateData(item.Key, equipData.itemImage, item.Value.quantity, equipData.name, equipData.attributeOne, equipData.attributeTwo, equipData.mainCategoryOne, StatToString(equipData.mainOneMin, equipData.mainOneMax), equipData.mainCategoryTwo,
                StatToString((int)equipData.mainTwoMin, equipData.mainTwoMax), equipData.mainCategoryThree, StatToString(equipData.mainThreeMin,
                equipData.mainThreeMax), ammoExtension);
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
        else if(item.equipType == EquipType.BothHands &&(equipSlot.equipType == EquipType.MainHand || equipSlot.equipType == EquipType.OffHand))
        {
            EquipSlot mainHandEquipSlot;
            EquipSlot offHandEquipSlot ;

            if (equipSlot.isBackUp)
            {
                mainHandEquipSlot = inventoryUI.backUpMainHand;
                offHandEquipSlot = inventoryUI.backUpOffHand;
            }
            else
            {
                mainHandEquipSlot = inventoryUI.equipSlots[3];
                offHandEquipSlot = inventoryUI.equipSlots[4];
            }

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

        inventoryUI.UpdateUnitInfo(currentUnit);
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
            EquipSlot mainHandEquipSlot;
            EquipSlot offHandEquipSlot;
            if (equipSlot.isBackUp)
            {
                mainHandEquipSlot = inventoryUI.backUpMainHand;
                offHandEquipSlot = inventoryUI.backUpOffHand;
            }
            else
            {
                mainHandEquipSlot = inventoryUI.equipSlots[3];
                offHandEquipSlot = inventoryUI.equipSlots[4];
            }

            if (mainHandEquipSlot != null && mainHandEquipSlot.currentItem != null)
            {
                HandleUnequipItem(mainHandEquipSlot);
            }

            if (offHandEquipSlot != null && offHandEquipSlot.currentItem != null)
            {
                HandleUnequipItem(offHandEquipSlot);
            }

            mainHandEquipSlot.AddItem(newItem, currentUnit);
            offHandEquipSlot.currentItem = newItem;
            if (equipSlot.isBackUp)
            {
                currentUnit.backUpOffHand = newItem;
            }
            else
            {
                currentUnit.offHand = newItem;
            }
            inventoryUI.ConfirmItemEquip(mainHandEquipSlot);
            inventoryUI.ConfirmItemEquip(offHandEquipSlot);
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
            EquipSlot mainHandEquipSlot;
            if (equipSlot.isBackUp)
            {
                mainHandEquipSlot = inventoryUI.backUpMainHand;
            }
            else
            {
                mainHandEquipSlot = inventoryUI.equipSlots[3];
            }

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
        inventoryUI.UpdateUnitInfo(currentUnit);
        inventoryUI.UpdateWeight(currentUnit.currentWeight, currentUnit.lowWeight, currentUnit.mediumWeight, currentUnit.highWeight);
        inventoryUI.UpdateWeightSecondary(currentUnit.backUpWeight, currentUnit.lowWeight, currentUnit.mediumWeight, currentUnit.highWeight);
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

            EquipSlot mainHandEquipSlot;
            EquipSlot offHandEquipSlot;
            if (equipSlot.isBackUp)
            {
                mainHandEquipSlot = inventoryUI.backUpMainHand;
                offHandEquipSlot = inventoryUI.backUpOffHand;
                if (equipSlot.equipType == EquipType.MainHand)
                {
                    currentUnit.backUpOffHand = null;
                }
                else
                {
                    currentUnit.backUpMainHand = null;
                }
            }
            else
            {
                mainHandEquipSlot = inventoryUI.equipSlots[3];
                offHandEquipSlot = inventoryUI.equipSlots[4];
                if (equipSlot.equipType == EquipType.MainHand)
                {
                    currentUnit.offHand = null;
                }
                else
                {
                    currentUnit.mainHand = null;
                }
            }

            mainHandEquipSlot.ClearItem();
            offHandEquipSlot.ClearItem();
        }
        equipSlot.RemoveItem(currentUnit);
        inventoryUI.UpdateUnitInfo(currentUnit);
        inventoryUI.UpdateWeight(currentUnit.currentWeight, currentUnit.lowWeight, currentUnit.mediumWeight, currentUnit.highWeight);
        inventoryUI.UpdateWeightSecondary(currentUnit.backUpWeight, currentUnit.lowWeight, currentUnit.mediumWeight, currentUnit.highWeight);
    }

    private void HandleSwapHands(bool isMainHand)
    {

            EquipSlot backUpMainHandEquipSlot = inventoryUI.backUpMainHand;
            EquipSlot backUpoffHandEquipSlot = inventoryUI.backUpOffHand;
            EquipSlot mainHandEquipSlot = inventoryUI.equipSlots[3];
            EquipSlot offHandEquipSlot = inventoryUI.equipSlots[4];

        bool thereIsABothHandItem = false;
        bool BackUpHasBothHandItem = false;
        bool mainHasBothHandItem = false;

        if(backUpMainHandEquipSlot.currentItem != null && backUpMainHandEquipSlot.currentItem.equipType == EquipType.BothHands)
        {
            thereIsABothHandItem = true;
            BackUpHasBothHandItem = true; ;
        }

        if(mainHandEquipSlot.currentItem != null && mainHandEquipSlot.currentItem.equipType == EquipType.BothHands)
        {
            thereIsABothHandItem = true;
            mainHasBothHandItem = true;
        }

        if (thereIsABothHandItem)
        {
            if (mainHasBothHandItem && BackUpHasBothHandItem)
            {
                EquipableItemSO backupMainHandItem = backUpMainHandEquipSlot.currentItem;
                EquipableItemSO mainHandItem = mainHandEquipSlot.currentItem;
                mainHandEquipSlot.RemoveItem(currentUnit);
                backUpMainHandEquipSlot.RemoveItem(currentUnit);

                backUpoffHandEquipSlot.ClearItem();
                currentUnit.backUpOffHand = null;
                offHandEquipSlot.ClearItem();
                currentUnit.offHand = null;

                mainHandEquipSlot.AddItem(backupMainHandItem, currentUnit);
                offHandEquipSlot.currentItem = backupMainHandItem;
                currentUnit.offHand = backupMainHandItem;
                backUpMainHandEquipSlot.AddItem(mainHandItem, currentUnit);
                backUpoffHandEquipSlot.currentItem = mainHandItem;
                currentUnit.backUpOffHand = mainHandItem;

                inventoryUI.ConfirmItemEquip(mainHandItem, backUpMainHandEquipSlot);
                inventoryUI.ConfirmItemEquip(mainHandItem, backUpoffHandEquipSlot);
                inventoryUI.ConfirmItemEquip(backupMainHandItem, mainHandEquipSlot);
                inventoryUI.ConfirmItemEquip(backupMainHandItem, offHandEquipSlot);
            }
            else if (mainHasBothHandItem)
            {
                EquipableItemSO backupMainHandItem = backUpMainHandEquipSlot.currentItem;
                EquipableItemSO backUpOffHandItem = backUpoffHandEquipSlot.currentItem;
                EquipableItemSO mainHandItem = mainHandEquipSlot.currentItem;
                mainHandEquipSlot.RemoveItem(currentUnit);
                backUpMainHandEquipSlot.RemoveItem(currentUnit);
                backUpoffHandEquipSlot.RemoveItem(currentUnit);

                offHandEquipSlot.ClearItem();
                currentUnit.offHand = null;

                mainHandEquipSlot.AddItem(backupMainHandItem, currentUnit);
                offHandEquipSlot.AddItem(backUpOffHandItem, currentUnit);
                backUpMainHandEquipSlot.AddItem(mainHandItem, currentUnit);
                backUpoffHandEquipSlot.currentItem = mainHandItem;
                currentUnit.backUpOffHand = mainHandItem;

                inventoryUI.ConfirmItemEquip(mainHandItem, backUpMainHandEquipSlot);
                inventoryUI.ConfirmItemEquip(mainHandItem, backUpoffHandEquipSlot);
                inventoryUI.ConfirmItemEquip(backupMainHandItem, mainHandEquipSlot);
                inventoryUI.ConfirmItemEquip(backUpOffHandItem, offHandEquipSlot);
            }
            else
            {
                EquipableItemSO backupMainHandItem = backUpMainHandEquipSlot.currentItem;
                EquipableItemSO offHandItem = offHandEquipSlot.currentItem;
                EquipableItemSO mainHandItem = mainHandEquipSlot.currentItem;
                mainHandEquipSlot.RemoveItem(currentUnit);
                offHandEquipSlot.RemoveItem(currentUnit);
                backUpMainHandEquipSlot.RemoveItem(currentUnit);

                backUpoffHandEquipSlot.ClearItem();
                currentUnit.backUpOffHand = null;

                backUpMainHandEquipSlot.AddItem(mainHandItem, currentUnit);
                mainHandEquipSlot.AddItem(backupMainHandItem, currentUnit);
                backUpoffHandEquipSlot.AddItem(offHandItem, currentUnit);
                offHandEquipSlot.currentItem = backupMainHandItem;
                currentUnit.offHand = backupMainHandItem;

                inventoryUI.ConfirmItemEquip(mainHandItem, backUpMainHandEquipSlot);
                inventoryUI.ConfirmItemEquip(offHandItem, backUpoffHandEquipSlot);
                inventoryUI.ConfirmItemEquip(backupMainHandItem, mainHandEquipSlot);
                inventoryUI.ConfirmItemEquip(backupMainHandItem, offHandEquipSlot);
            }
        }
        else if(isMainHand)
        {
            EquipableItemSO backupMainHandItem = backUpMainHandEquipSlot.currentItem;
            EquipableItemSO mainHandItem = mainHandEquipSlot.currentItem;
            mainHandEquipSlot.RemoveItem(currentUnit);
            backUpMainHandEquipSlot.RemoveItem(currentUnit);

            mainHandEquipSlot.AddItem(backupMainHandItem, currentUnit);
            backUpMainHandEquipSlot.AddItem(mainHandItem, currentUnit);
            inventoryUI.ConfirmItemEquip(mainHandItem, backUpMainHandEquipSlot);
            inventoryUI.ConfirmItemEquip(backupMainHandItem, mainHandEquipSlot);
        }
        else
        {
            EquipableItemSO backupOffHandItem = backUpoffHandEquipSlot.currentItem;
            EquipableItemSO offHandItem = offHandEquipSlot.currentItem;
            offHandEquipSlot.RemoveItem(currentUnit);
            backUpoffHandEquipSlot.RemoveItem(currentUnit);

            offHandEquipSlot.AddItem(backupOffHandItem, currentUnit);
            backUpoffHandEquipSlot.AddItem(offHandItem, currentUnit);
            inventoryUI.ConfirmItemEquip(offHandItem, backUpoffHandEquipSlot);
            inventoryUI.ConfirmItemEquip(backupOffHandItem, offHandEquipSlot);
        }
        inventoryUI.UpdateUnitInfo(currentUnit);
        inventoryUI.UpdateWeight(currentUnit.currentWeight, currentUnit.lowWeight, currentUnit.mediumWeight, currentUnit.highWeight);
        inventoryUI.UpdateWeightSecondary(currentUnit.backUpWeight, currentUnit.lowWeight, currentUnit.mediumWeight, currentUnit.highWeight);
    }

    private void HandleSwapEquipSlot(EquipSlot currentlyDraggedEquipSlot, EquipSlot newEquipSlot)
    {
        EquipableItemSO newItem = currentlyDraggedEquipSlot.currentItem;
        if (currentlyDraggedEquipSlot.currentItem.equipType == EquipType.BothHands)
        {
            if (currentlyDraggedEquipSlot.isBackUp)
            {
                inventoryUI.backUpMainHand.ClearItem();
                inventoryUI.backUpOffHand.ClearItem();
            }
            else
            {
                inventoryUI.equipSlots[3].ClearItem();
                inventoryUI.equipSlots[4].ClearItem();
            }
        }
        else
        {
            currentlyDraggedEquipSlot.ClearItem();
        }

        if (newItem.equipType == EquipType.BothHands && (newEquipSlot.equipType == EquipType.MainHand || newEquipSlot.equipType == EquipType.OffHand))
        {
            EquipSlot mainHandEquipSlot;
            EquipSlot offHandEquipSlot;
            if (newEquipSlot.isBackUp)
            {
                mainHandEquipSlot = inventoryUI.backUpMainHand;
                offHandEquipSlot = inventoryUI.backUpOffHand;
            }
            else
            {
                mainHandEquipSlot = inventoryUI.equipSlots[3];
                offHandEquipSlot = inventoryUI.equipSlots[4];
            }

            bool twoHandedRemoved = false;
            if (mainHandEquipSlot != null && mainHandEquipSlot.currentItem != null)
            {
                if(mainHandEquipSlot.currentItem.equipType == EquipType.BothHands)
                {
                    twoHandedRemoved = true;
                }
                HandleUnequipItem(mainHandEquipSlot);
            }

            if (offHandEquipSlot != null && offHandEquipSlot.currentItem != null)
            {
                if (twoHandedRemoved)
                {
                    offHandEquipSlot.ClearItem();
                }
                else
                {
                    HandleUnequipItem(offHandEquipSlot);
                }
            }

            currentlyDraggedEquipSlot.RemoveItem(currentUnit);
            if (currentlyDraggedEquipSlot.isBackUp)
            {
                currentUnit.backUpMainHand = null;
                currentUnit.backUpOffHand = null;
            }
            else
            {
                currentUnit.mainHand = null;
                currentUnit.offHand = null;
            }

            mainHandEquipSlot.AddItem(newItem, currentUnit);
            offHandEquipSlot.currentItem = newItem;

            if (newEquipSlot.isBackUp)
            {
                currentUnit.backUpOffHand = newItem;
            }
            else
            {
                currentUnit.offHand = newItem;
            }


            inventoryUI.ConfirmItemEquip(newItem, mainHandEquipSlot);
            inventoryUI.ConfirmItemEquip(newItem, offHandEquipSlot);
        }
        else if (newItem.equipType == EquipType.Accessory && (newEquipSlot.equipType == EquipType.Accessory1 || newEquipSlot.equipType == EquipType.Accessory2 || newEquipSlot.equipType == EquipType.Accessory3 || newEquipSlot.equipType == EquipType.Accessory4))
        {
            newEquipSlot.AddItem(newItem, currentUnit);
            inventoryUI.ConfirmItemEquip(newItem ,newEquipSlot);
        }
        else if (newItem.equipType == newEquipSlot.equipType)
        {
            EquipSlot mainHandEquipSlot;
            if (newEquipSlot.isBackUp)
            {
                mainHandEquipSlot = inventoryUI.backUpMainHand;
            }
            else
            {
                mainHandEquipSlot = inventoryUI.equipSlots[3];
            }

            if (newItem.equipType == EquipType.OffHand && mainHandEquipSlot.currentItem != null && 
                mainHandEquipSlot.currentItem.equipType == EquipType.BothHands)
            {
                HandleUnequipItem(mainHandEquipSlot);
            }

            if (newEquipSlot.currentItem != null)
            {
                HandleUnequipItem(newEquipSlot);
            }

            newEquipSlot.AddItem(newItem, currentUnit);
            inventoryUI.ConfirmItemEquip(newItem, newEquipSlot);
            currentlyDraggedEquipSlot.RemoveItem(currentUnit);
        }
        else
        {
            currentlyDraggedEquipSlot.RemoveItem(currentUnit);
        }

        inventoryUI.UpdateUnitInfo(currentUnit);
        inventoryUI.UpdateWeight(currentUnit.currentWeight, currentUnit.lowWeight, currentUnit.mediumWeight, currentUnit.highWeight);
        inventoryUI.UpdateWeightSecondary(currentUnit.backUpWeight, currentUnit.lowWeight, currentUnit.mediumWeight, currentUnit.highWeight);
    }

    private void HandleOnUnitCategoryChanged(bool heroButtonPressed)
    {
        for(int i = 0; i < inventoryUI.equipSlots.Count; i++)
        {
            inventoryUI.equipSlots[i].disabledDueToMercenary = !heroButtonPressed;
        }
    }
}
