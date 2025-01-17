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

    public override int CalculateWeight(AIActionData actionData)
    {
        return -2;
    }

    public override void FindOptimalPosition(AIActionData actionData)
    {
        return;
    }

    public override bool CheckIfActionIsInRange(AIActionData actionData)
    {
        return false;
    }

    public override void AIUseAction(AIActionData actionData)
    {
        throw new NotImplementedException();
    }

    public override void SelectAction(Unit self)
    {
        Debug.Log("select Ranged ATtack");
        base.SelectAction(self);
        int actionIndex = GetActionIndex(self);
        int amountOfActionPointsUsed = this.intialActionPointUsage + actionPointGrowth * self.actions[actionIndex].amountUsedDuringRound;

        List<EquipableAmmoSO> unitAmmo = new List<EquipableAmmoSO>();

        CheckCorrectAmmo(self.Item1, 0, unitAmmo, self);
        CheckCorrectAmmo(self.Item2, 1, unitAmmo, self);
        CheckCorrectAmmo(self.Item3, 2, unitAmmo, self);
        CheckCorrectAmmo(self.Item4, 3, unitAmmo, self);

        bool foundAmmo = false;
        for (int i = 0; i < unitAmmo.Count; i++)
        {
            if (unitAmmo[i] != null)
            {
                foundAmmo = true;
            }
        }

        if (!foundAmmo)
        {
            return;
        }

        Damage mainDamage = new Damage();
        mainDamage.minDamage = minDamage;
        mainDamage.maxDamage = maxDamage;
        mainDamage.damageType = DamageTypes.physical;

        AttackData newAttackData = new AttackData(new List<Damage>() {mainDamage}, effectAgainstArmorPercentage, self);
        self.gameManager.spriteManager.ActivateRangedTargeting(self, false, self.currentActionsPoints, amountOfActionPointsUsed, effectiveRange,
            newAttackData, unitAmmo);
        self.gameManager.spriteManager.rangedTargeting.OnFoundTarget += FoundTarget;
    }

    public override bool SpecificCheckActionUsable(Unit self)
    {
        List<EquipableAmmoSO> unitAmmo = new List<EquipableAmmoSO>();

        CheckCorrectAmmo(self.Item1, 0, unitAmmo, self);
        CheckCorrectAmmo(self.Item2, 1, unitAmmo, self);
        CheckCorrectAmmo(self.Item3, 2, unitAmmo, self);
        CheckCorrectAmmo(self.Item4, 3, unitAmmo, self);

        for(int i = 0; i < unitAmmo.Count; i++)
        {
            if (unitAmmo[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    public void CheckCorrectAmmo(EquipableItemSO item, int itemIndex, List<EquipableAmmoSO> unitAmmo, Unit self)
    {
        EquipableAmmoSO newAmmo = null;
        if (item != null && item.GetType() == typeof(EquipableAmmoSO) && self.itemUses[itemIndex] > 0)
        {
            EquipableAmmoSO ammo = (EquipableAmmoSO)item;
            if(ammo.ammoType == ammoType)
            {
                newAmmo = ammo;
            }
        }
        unitAmmo.Add(newAmmo);
    }

    public void FoundTarget(Unit movingUnit, Unit targetUnit, bool foundTarget)
    {
        Debug.Log("Found Target");
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

    public override void ConfirmAction(ActionData actionData)
    {
        throw new NotImplementedException();
    }

}

