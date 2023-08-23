using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using Inventory.Model;

public class SoulSlot : MonoBehaviour, IDropHandler
{
    public Image borderImage;
    public Image contentImage;

    public SoulItemSO currentSoul;

    public SoulSlotType soulSlotType;

    public event Action<SoulSlot> OnItemDroppedOn;

    public void OnDrop(PointerEventData eventData)
    {
        contentImage.gameObject.SetActive(true);
        OnItemDroppedOn?.Invoke(this);
    }

    public void AddSoul(SoulItemSO soulItem, Unit unit)
    {
        if (currentSoul != null)
        {
            RemoveSoul(unit);
            if (soulSlotType == SoulSlotType.physical)
            {
                soulItem.AddPhysicalSoul(unit);
                currentSoul = soulItem;
            }

            if (soulSlotType == SoulSlotType.mental)
            {
                soulItem.AddMentalSoul(unit);
                currentSoul = soulItem;
            }
        }
        else
        {
            if (soulSlotType == SoulSlotType.physical)
            {
                soulItem.AddPhysicalSoul(unit);
                currentSoul = soulItem;
            }

            if (soulSlotType == SoulSlotType.mental)
            {
                soulItem.AddMentalSoul(unit);
                currentSoul = soulItem;
            }
        }
    }

    public void RemoveSoul(Unit unit)
    {
        if (soulSlotType == SoulSlotType.physical)
        {
            currentSoul.RemovePhysicalSoul(unit);
        }

        if (soulSlotType == SoulSlotType.mental)
        {
            currentSoul.RemoveMentalSoul(unit);
        }

    }

    public enum SoulSlotType
    {
        physical,
        mental
    }

}
