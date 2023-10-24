using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    [SerializeField]
    private InventorySO inventoryData;

    public void Pickup(Item item, int itemIndexInList)
    {
        if(item != null)
        {
            int remainder = inventoryData.AddItem(item.inventoryItem, item.quantity);
            item.quantity = remainder;
            if (remainder == 0)
            {
                item.DestroyItem(itemIndexInList);
            }
        }
    }
} 
