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

        public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;
        public event Action<ItemSO, int> OnAddItem, OnAddItemToNewSlot, OnRemoveItem;

        public void Initialize()
        {
            inventoryItems = new List<InventoryItem>();
        }

        public void AddItem(InventoryItem item)
        {
            Debug.Log(item.ToString() + "," + item.quantity);
            AddItem(item.item, item.quantity);
        }

        public void AddItem(ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            if (item.isStackable == false)
            {
                Debug.LogError("Shouldn't Happen");
                return;
            }
            AddStackableItem(item, quantity);
            InformAboutChange();
            return;
        }

        private void AddItemToNewSlot(ItemSO item, int quantity, List<ItemParameter> itemState = null)
        {
            InventoryItem newItem = new InventoryItem()
            {
                item = item,
                quantity = quantity,
                itemState = new List<ItemParameter>(itemState == null ? item.DefaultParameterList : itemState)
            };

            inventoryItems.Add(newItem);
            OnAddItemToNewSlot?.Invoke(item, quantity);
            InformAboutChange();
            return;
        }

        private void AddStackableItem(ItemSO item, int quantity)
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
                        OnAddItem?.Invoke(item, quantity);
                        InformAboutChange();
                        return;
                    }
                } 
            }
            while (quantity > 0)
            {
                int newQuantity = Mathf.Clamp(quantity, 0, item.maxStackSize);
                quantity -= newQuantity;
                AddItemToNewSlot(item, newQuantity); 
            }
            return;
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
                ItemSO removedItem = inventoryItems[itemIndex].item;
                if (inventoryItems[itemIndex].isEmpty)
                {
                    return;
                }
                int remainder = inventoryItems[itemIndex].quantity - amount;
                if (remainder <= 0)
                {
                    inventoryItems.RemoveAt(itemIndex);
                }
                else
                {
                    inventoryItems[itemIndex] = inventoryItems[itemIndex]
                        .ChangeQuantity(remainder);
                }
                OnRemoveItem?.Invoke(removedItem, amount);
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
            InventoryItem newItem = new InventoryItem ()
            {
                item = this.item,
                quantity = newQuantity,
                itemState = new List<ItemParameter>(this.itemState)
            };

            Debug.Log(newItem.item);
            Debug.Log(newItem.quantity);
            return new InventoryItem ()
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