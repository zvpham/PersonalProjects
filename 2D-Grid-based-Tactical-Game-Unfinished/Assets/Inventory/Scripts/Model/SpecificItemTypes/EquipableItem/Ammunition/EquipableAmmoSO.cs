using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/EquipableAmmoSO/AmmoSO")]
public class EquipableAmmoSO : EquipableItemSO
{
    // mainTwoMin = damage multiplier
    public AmmoType ammoType;
    public float armorPiercingModifier = 1f;
    public float rangeModifier = 1f;
    public bool ignoreRangeModifier = false;
    public bool ignoreCoverModifer = false;
    //public List<Status> statuses;
    public override void EquipItem(Unit unit, bool isBackUp)
    {
        base.EquipItem(unit, isBackUp);
    }

    public override void UnequipItem(Unit unit, bool isBackUp)
    {
        base.UnequipItem(unit, isBackUp);
    }

    public void ModifyAttack(AttackData attackData)
    {
        AttackData newAttackData = attackData;
        newAttackData.armorDamagePercentage = armorPiercingModifier;
    }
}

[SerializeField]
public enum AmmoType
{
    Bow,
    CrossBow
}