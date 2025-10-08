using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using Inventory.Model;
using static SoulSlot;
using Unity.VisualScripting;
using static UnityEditor.Progress;

public class EquipSlot : MonoBehaviour, IDropHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler
{ 
    public Image borderImage;
    public Image contentImage;

    public EquipableItemSO currentItem;
    public int EquipSlotIndex = -1;

    public EquipType equipType;
    public bool isBackUp = false;
    public bool disabledDueToMercenary = false;
    public bool missionStart = false;

    public event Action<EquipSlot> OnItemDroppedOn, OnItemClicked, OnItemBeginDrag;

    public void ClearItem()
    {
        contentImage.sprite = null;
        currentItem = null;
    }

    public void AddItem(EquipableItemSO item, Unit unit, bool onLoad = false)
    {
        if(unit == null || item == null)
        {
            return;
        }

        switch(equipType)
        {
            case (EquipType.Accessory1):
                unit.Item1 = item;
                break;
            case (EquipType.Accessory2):
                unit.Item2 = item;
                break;
            case (EquipType.Accessory3):
                unit.Item3 = item;
                break;
            case (EquipType.Accessory4):
                unit.Item4 = item;
                break;
            case (EquipType.Head):
                unit.helmet = item;
                break;
            case (EquipType.Body):
                unit.armor = item;
                break;
            case (EquipType.Boot):
                unit.legs = item;
                break;
            case (EquipType.MainHand):
                if (isBackUp)
                {
                    unit.backUpMainHand = item;
                }
                else
                {
                    unit.mainHand = item;
                }
                break;
            case (EquipType.OffHand):
                if (isBackUp)
                {
                    unit.backUpOffHand = item;
                }
                else
                {
                    unit.offHand = item;
                }
                break;
        }
        currentItem = item;
        item.EquipItem(unit, isBackUp);
    }

    public void RemoveItem(Unit unit, bool onLoad = false)
    {
        EquipableItemSO item = null;
        switch (equipType)
        {
            case (EquipType.Accessory1):
                item = unit.Item1;
                unit.Item1 = null;
                break;
            case (EquipType.Accessory2):
                item = unit.Item2;
                unit.Item2 = null;
                break;
            case (EquipType.Accessory3):
                item = unit.Item3;
                unit.Item3 = null;
                break;
            case (EquipType.Accessory4):
                item = unit.Item4;
                unit.Item4 = null;
                break;
            case (EquipType.Head):
                item = unit.helmet;
                unit.helmet = null;
                break;
            case (EquipType.Body):
                item = unit.armor;
                unit.armor = null;
                break;
            case (EquipType.Boot):
                item = unit.legs;
                unit.legs = null;
                break;
            case (EquipType.MainHand):
                if (isBackUp)
                {
                    item = unit.backUpMainHand;
                    unit.backUpMainHand = null;
                }
                else
                {
                    item = unit.mainHand;
                    unit.mainHand = null;
                }
                break;
            case (EquipType.OffHand):
                if (isBackUp)
                {
                    item = unit.backUpOffHand;
                    unit.backUpOffHand = null;
                }
                else
                {
                    item = unit.offHand;
                    unit.offHand = null;
                }
                break;
        }
        ClearItem();
        currentItem = null;
        if(item != null)
        {
            item.UnequipItem(unit, isBackUp);
        }
    }

    public void OnPointerClick(PointerEventData pointerData)
    {
        if (pointerData.button == PointerEventData.InputButton.Right)
        {

        }
        else
        {
            OnItemClicked?.Invoke(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentItem == null || disabledDueToMercenary || missionStart)
        {
            return;
        }
        OnItemBeginDrag?.Invoke(this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        OnItemDroppedOn?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {

    }
}
