using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/EquipableArmor")]
public class EquipableArmorSO : EquipableItemSO
{
    public override void EquipItem(Unit unit, bool isBackUp)
    {
        base.EquipItem(unit, isBackUp);
        unit.maxArmor += (int) mainTwoMin;
    }

    public override void UnequipItem(Unit unit, bool isBackUp)
    {
        base.UnequipItem(unit, isBackUp);
        unit.maxArmor -= (int) mainTwoMin;
    }
}
