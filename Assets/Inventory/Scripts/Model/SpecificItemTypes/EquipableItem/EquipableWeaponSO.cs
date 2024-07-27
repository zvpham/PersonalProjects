using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/EquipableWeapon")]
public class EquipableWeaponSO : EquipableItemSO
{
    public override void EquipItem(Unit unit)
    {
        base.EquipItem(unit);
    }

    public override void UnequipItem(Unit unit)
    {
        base.UnequipItem(unit);     
    }
}
