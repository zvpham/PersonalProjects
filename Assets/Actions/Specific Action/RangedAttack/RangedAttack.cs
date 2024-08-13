using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/RangedAttack")]
public class RangedAttack : Action
{
    public int minDamage;
    public int maxDamage;
    public int effectiveRange = 4;
    public int maxRange = 8;
    public float effectAgainstArmorPercentage;
    public bool ignoreArmor = false;
    public AmmoType ammoType;
    public override void SelectAction(Unit self)
    {
        Debug.Log("select Ranged ATtack");
        base.SelectAction(self);
        int actionIndex = self.actions.IndexOf(this);
        int amountOfActionPointsUsed = this.intialActionPointUsage + actionPointGrowth * self.amountActionUsedDuringRound[actionIndex];

        List<EquipableAmmoSO> unitAmmo = new List<EquipableAmmoSO>();

        CheckCorrectAmmo(self.Item1, unitAmmo);
        CheckCorrectAmmo(self.Item2, unitAmmo);
        CheckCorrectAmmo(self.Item3, unitAmmo);
        CheckCorrectAmmo(self.Item4, unitAmmo);

        if(unitAmmo.Count <= 0)
        {
            Debug.Log("No ammo");
            return;
        }

        AttackData newAttackData = new AttackData(minDamage, maxDamage, effectAgainstArmorPercentage, self);
        self.gameManager.spriteManager.ActivateRangedTargeting(self, false, self.currentActionsPoints, amountOfActionPointsUsed, effectiveRange,
            newAttackData, unitAmmo);
        self.gameManager.spriteManager.rangedTargeting.OnFoundTarget += FoundTarget;
    }

    public override bool SpecificCheckActionUsable(Unit self)
    {
        List<EquipableAmmoSO> unitAmmo = new List<EquipableAmmoSO>();

        CheckCorrectAmmo(self.Item1, unitAmmo);
        CheckCorrectAmmo(self.Item2, unitAmmo);
        CheckCorrectAmmo(self.Item3, unitAmmo);
        CheckCorrectAmmo(self.Item4, unitAmmo);

        if (unitAmmo.Count <= 0)
        {
            return false;
        }

        return true;
    }

    public void CheckCorrectAmmo(EquipableItemSO item, List<EquipableAmmoSO> unitAmmo)
    {
        Debug.Log("Item: " + item);
        if(item != null)
        {
            Debug.Log("Item Type: " + item.GetType());
        }
        if (item != null && item.GetType() == typeof(EquipableAmmoSO))
        {
            EquipableAmmoSO ammo = (EquipableAmmoSO)item;
            if(ammo.ammoType == ammoType)
            {
                Debug.Log("ammo Count: " + unitAmmo.Count);
                unitAmmo.Add(ammo);
                Debug.Log("ammo Count2: " + unitAmmo.Count);
            }
        }
    }

    public void FoundTarget(Unit movingUnit, Unit targetUnit, bool foundTarget)
    {
        if (foundTarget)
        {
            GridHex<GridPosition> map = movingUnit.gameManager.grid;
            map.GetXY(movingUnit.transform.position, out int x, out int y);
            Vector3Int movingUnitCube = map.OffsetToCube(x, y);

            map.GetXY(targetUnit.transform.position, out x, out y);
            map.OffsetToCube(x, y);
            Vector3Int targetUnitCube = map.OffsetToCube(x, y);
            int distanceBetweenUnits = map.CubeDistance(movingUnitCube, targetUnitCube);

            Debug.Log(distanceBetweenUnits);
            if (targetUnit != null && distanceBetweenUnits <= effectiveRange)
            {
                Debug.Log("Hit");
                MeleeAttackAnimation meleeAttackAnimation = (MeleeAttackAnimation)Instantiate(this.animation);
                meleeAttackAnimation.SetParameters(movingUnit.gameManager, movingUnit.transform.position, targetUnit.transform.position);
                int adjustedMinimumDamage = minDamage + (int)(minDamage * movingUnit.GetMinimumDamageModifer());
                int adjustmedMaximumDamage = maxDamage + (int)(maxDamage * movingUnit.GetMaximumDamageModifer());
                targetUnit.TakeDamage(movingUnit, new List<int>() { adjustedMinimumDamage }, new List<int>() { adjustmedMaximumDamage },
                    true, false, ignoreArmor, true);
                UseActionPreset(movingUnit);
            }
            else
            {
                movingUnit.UseActionPoints(0);
            }
        }
        else
        {
            movingUnit.UseActionPoints(0);
        }
        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
    }

    public List<AttackDataUI> CalculateAttackData(Unit movingUnit, Unit targetUnit, List<Vector2Int> path)
    {
        AttackDataUI mainAttack = new AttackDataUI();
        int adjustedMinimumDamage = minDamage + (int)(minDamage * movingUnit.GetMinimumDamageModifer());
        int adjustmedMaximumDamage = maxDamage + (int)(maxDamage * movingUnit.GetMaximumDamageModifer());

        //adjustedMinimumDamage = targetUnit.CalculateEstimatedDamage(adjustedMinimumDamage, true);
        //adjustmedMaximumDamage = targetUnit.CalculateEstimatedDamage(adjustmedMaximumDamage, true);
        mainAttack.data = adjustedMinimumDamage.ToString() + " - " + adjustmedMaximumDamage.ToString();
        mainAttack.min = adjustedMinimumDamage;
        mainAttack.max = adjustmedMaximumDamage;
        mainAttack.attackDataType = attackDataType.Main;
        mainAttack.attackState = attackState.Benign;

        List<AttackDataUI> allAttackData = new List<AttackDataUI>();
        allAttackData.Add(mainAttack);

        return allAttackData;
    }
}

[SerializeField]
public struct AttackData
{
    public int minDamage;
    public int maxDamage;
    public float armorDamagePercentage;
    public Unit originUnit;
    //List<Status> 

    public AttackData(int minDamage, int maxDamage, float armorDamagePercentage, Unit originUnit)
    {
        this.minDamage = minDamage;
        this.maxDamage = maxDamage;
        this.armorDamagePercentage = armorDamagePercentage;
        this.originUnit = originUnit;
    }

    public List<AttackDataUI> CalculateAttackData(Unit targetUnit)
    {
        AttackDataUI mainAttack = new AttackDataUI();
        int adjustedMinimumDamage = minDamage + (int)(minDamage * originUnit.GetMinimumDamageModifer());
        int adjustmedMaximumDamage = maxDamage + (int)(maxDamage * originUnit.GetMaximumDamageModifer());

        Tuple<int, int, List<AttackDataUI>> attackData = targetUnit.CalculateEstimatedDamage(adjustedMinimumDamage, adjustmedMaximumDamage, true);
        adjustedMinimumDamage = attackData.Item1;
        adjustmedMaximumDamage = attackData.Item2;
        if(adjustedMinimumDamage > adjustmedMaximumDamage)
        {
            adjustmedMaximumDamage = adjustedMinimumDamage;
        }

        if (adjustmedMaximumDamage == 0)
        {
            mainAttack.data = 0.ToString();
        }
        else
        {
            mainAttack.data = adjustedMinimumDamage.ToString() + " - " + adjustmedMaximumDamage.ToString();
        }

        mainAttack.min = adjustedMinimumDamage;
        mainAttack.max = adjustmedMaximumDamage;
        mainAttack.attackDataType = attackDataType.Main;
        mainAttack.attackState = attackState.Benign;

        List<AttackDataUI> allAttackData = new List<AttackDataUI>();
        allAttackData.Add(mainAttack);
        for(int i = 0; i < attackData.Item3.Count; i++)
        {
            allAttackData.Add(attackData.Item3[i]);
        }

        return allAttackData;
    }

}

