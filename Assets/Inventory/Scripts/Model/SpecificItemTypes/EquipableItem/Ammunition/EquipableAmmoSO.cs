using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/EquipableAmmoSO")]
public class EquipableAmmoSO : EquipableItemSO
{
    // mainTwoMin = damage multiplier
    public AmmoType ammoType;
    public float armorPiercingModifier = 1f;
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
        attackData.minDamage = (int) (attackData.minDamage * this.mainTwoMin);
        attackData.maxDamage = (int) (attackData.maxDamage * this.mainTwoMin);
        attackData.armorDamagePercentage = armorPiercingModifier; 
    }
}

[SerializeField]
public enum AmmoType
{
    Bow,
    CrossBow
}
