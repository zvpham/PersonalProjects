using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/EquipableArmor")]
public class EquipableArmorSO : EquipableItemSO
{
    public override void EquipItem(Unit unit)
    {
        base.EquipItem(unit);
        unit.maxArmor += mainTwoMin;
    }

    public override void UnequipItem(Unit unit)
    {
        base.UnequipItem(unit);
        unit.maxArmor -= mainTwoMin;
    }
}
