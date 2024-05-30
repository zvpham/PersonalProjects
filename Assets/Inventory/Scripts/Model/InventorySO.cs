using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;

namespace Inventory.Model
{
    [CreateAssetMenu]
    public class InventorySO : ScriptableObject
    {

        [SerializeField]
        public List<InventoryItem> inventoryItems;

        [field: SerializeField]
        public int Size { get; private set; } = 10;

        public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;

        public void Initialize()
        {
            inventoryItems = new List<InventoryItem>();
            for (int i = 0; i < Size; i++) 
            {
                inventoryItems.Add(InventoryItem.GetEmptyItem());
            }
        }
        public int AddItem(ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            if (item.isStackable == false)
            {
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    if (isInventoryFull())
                    {
                        return quantity;
                    }

                    while (quantity > 0 && isInventoryFull() == false)
                    {
                       quantity -= AddItemToFirstFreeSlot(item, 1, itemState);
                    }
                    InformAboutChange();
                    return quantity;
                }
            }
            quantity = AddStackableItem(item, quantity);
            InformAboutChange();
            return quantity;
        }

        private int AddItemToFirstFreeSlot(ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            InventoryItem newItem = new InventoryItem()
            {
                item = item,
                quantity = quantity,
                itemState = new List<ItemParameter>(itemState == null ? item.DefaultParameterList : itemState)
            };

            for(int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].isEmpty)
                {
                    inventoryItems[i] = newItem;
                    return quantity;
                }
            }
            return 0;
        }
        private bool isInventoryFull()
        => inventoryItems.Where(item => item.isEmpty).Any() == false;

        private int AddStackableItem(ItemSO item, int quantity)
        {
            for(int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].isEmpty)
                {
                    continue;
                }
                if (inventoryItems[i].item.iD == item.iD)
                {
                    int amountPossibleToTake = inventoryItems[i].item.maxStackSize
                        - inventoryItems[i].quantity;

                    if(quantity > amountPossibleToTake)
                    {
                        inventoryItems[i] = inventoryItems[i].ChangeQuantity(inventoryItems[i].item.maxStackSize);
                        quantity -= amountPossibleToTake;
                    }
                    else
                    {
                        inventoryItems[i] = inventoryItems[i].ChangeQuantity(inventoryItems[i].quantity + quantity);
                        InformAboutChange();
                        return 0;
                    }
                } 
            }
            while (quantity > 0 &&  isInventoryFull() == false)
            {
                int newQuantity = Mathf.Clamp(quantity, 0, item.maxStackSize);
                quantity -= newQuantity;
                AddItemToFirstFreeSlot(item, newQuantity); 
            }
            return quantity;
        }

        public void AddItem(InventoryItem item)
        { 
            AddItem(item.item, item.quantity);
        }

        public Dictionary<int, InventoryItem> GetCurrentInventoryState()
        {
            Dictionary<int, InventoryItem> returnValue = new Dictionary<int, InventoryItem>();

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (inventoryItems[i].isEmpty)
                {
                    continue;
                }
                returnValue[i] = inventoryItems[i];
            }
            return returnValue;
        }

        public InventoryItem GetItemAt(int itemIndex)
        {
            return inventoryItems[itemIndex];
        }

        public void SwapItems(int itemIndex1,  int itemIndex2)
        {
            if(itemIndex1 != -1 && itemIndex2 != -1)
            {
                InventoryItem item1 = inventoryItems[itemIndex1];
                inventoryItems[itemIndex1] = inventoryItems[itemIndex2];
                inventoryItems[itemIndex2] = item1;
                InformAboutChange();
            }
        }

        private void InformAboutChange()
        {
            OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
        }

        public void RemoveItem(int itemIndex, int amount)
        {
            if(inventoryItems.Count > itemIndex)
            {
                if (inventoryItems[itemIndex].isEmpty)
                {
                    return;
                }
                int remainder = inventoryItems[itemIndex].quantity - amount;
                if (remainder <= 0)
                {
                    inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();
                }
                else
                {
                    inventoryItems[itemIndex] = inventoryItems[itemIndex]
                        .ChangeQuantity(remainder);
                }
                InformAboutChange();
            }
        }
    }

    [Serializable]
    public struct InventoryItem
    {
        public int quantity;
        public ItemSO item;
        public List<ItemParameter> itemState;
        public bool isEmpty => item == null;

        public InventoryItem ChangeQuantity(int newQuantity)
        {
            return new InventoryItem
            {
                item = this.item,
                quantity = newQuantity,
                itemState = new List<ItemParameter>(this.itemState)
            };
        }

        public static InventoryItem GetEmptyItem()
            => new InventoryItem
            {
                item = null,
                quantity = 0
            };

    }

}