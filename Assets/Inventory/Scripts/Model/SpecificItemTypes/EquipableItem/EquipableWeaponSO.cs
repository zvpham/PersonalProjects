using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/EquipableWeapon")]
public class EquipableWeaponSO : EquipableItemSO
{
    public override void EquipItem(Unit unit, bool isBackUp)
    {
        base.EquipItem(unit, isBackUp);
    }

    public override void UnequipItem(Unit unit, bool isBackUp)
    {
        base.UnequipItem(unit, isBackUp);     
    }
}
