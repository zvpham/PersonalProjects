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

    public int soulSlotIndex = -1;
    public int physicalOrMentalSoulIndex = -1;

    public SoulSlotType soulSlotType;

    public event Action<SoulSlot> OnItemDroppedOn;

    public void OnDrop(PointerEventData eventData)
    {
        OnItemDroppedOn?.Invoke(this);
    }
     /*
    public void AddSoul(SoulItemSO soulItem, Player player, bool onLoad = false)
    {
        if (currentSoul != null)
        {
            RemoveSoul(player, onLoad);
            HandlAddSoul(soulItem, player, onLoad);
        }
        else
        {
            HandlAddSoul(soulItem, player, onLoad);
        }
    }

    private void HandlAddSoul(SoulItemSO soulItem, Player player, bool onLoad)
    {
        if (soulSlotType == SoulSlotType.physical)
        {
            soulItem.AddPhysicalSoul(player);
            currentSoul = soulItem;
            bool foundSoul = false;
            for (int i = 0; i < player.physicalSouls.Count; i++)
            {
                if (player.physicalSouls[i] == null)
                {
                    player.physicalSouls[i] = currentSoul;
                    physicalOrMentalSoulIndex = i;
                    foundSoul = true;
                }
            }
            if (!foundSoul)
            {
                physicalOrMentalSoulIndex = player.physicalSouls.Count;
                player.physicalSouls.Add(currentSoul);
            }
        }
        else if (soulSlotType == SoulSlotType.mental)
        {
            soulItem.AddMentalSoul(player);
            currentSoul = soulItem;
            bool foundSoul = false;
            for (int i = 0; i < player.mentalSouls.Count; i++)
            {
                if (player.mentalSouls[i] == null)
                {
                    player.mentalSouls[i] = currentSoul;
                    physicalOrMentalSoulIndex = i;
                    foundSoul = true;
                }
            }
            if (!foundSoul)
            {
                physicalOrMentalSoulIndex = player.mentalSouls.Count;
                player.mentalSouls.Add(currentSoul);
            }
        }

        if (!onLoad)
        {
            player.soulSlotIndexes.Add(soulSlotIndex);
            player.onLoadSouls.Add(soulItem);
        }
        else
        {
            contentImage.gameObject.SetActive(true);
            contentImage.sprite = currentSoul.itemImage;
        }
    }

    public void RemoveSoul(Player player, bool onLoad)
    {
        if (soulSlotType == SoulSlotType.physical)
        {
            currentSoul.RemovePhysicalSoul(player);
            player.physicalSouls[physicalOrMentalSoulIndex] = null;
        }

        if (soulSlotType == SoulSlotType.mental)
        {
            currentSoul.RemoveMentalSoul(player);
            player.mentalSouls[physicalOrMentalSoulIndex] = null;
        }

        if (!onLoad)
        {
            int index = player.soulSlotIndexes.IndexOf(soulSlotIndex);
            player.soulSlotIndexes.RemoveAt(index);
            player.onLoadSouls.RemoveAt(index);
        }
    }
    */
    public enum SoulSlotType
    {
        physical,
        mental
    }

}
