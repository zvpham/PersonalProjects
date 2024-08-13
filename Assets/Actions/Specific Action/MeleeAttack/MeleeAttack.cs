using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/MeleeAttack")]
public class MeleeAttack : Action
{
    public int minDamage;
    public int maxDamage;
    public int range = 1;
    public float effectAgainstArmorPercentage;
    public bool ignoreArmor = false;
    public override void SelectAction(Unit self)
    {
        base.SelectAction(self);
        int actionIndex = self.actions.IndexOf(this);
        int amountOfActionPointsUsed = this.intialActionPointUsage + actionPointGrowth * self.amountActionUsedDuringRound[actionIndex];
        self.gameManager.spriteManager.ActivateMeleeAttackTargeting(self, false, self.currentActionsPoints, amountOfActionPointsUsed, range,
            CalculateAttackData);
        self.gameManager.spriteManager.meleeTargeting.OnFoundTarget += FoundTarget;
    }

    public void FoundTarget(Unit movingUnit, Unit targetUnit, bool foundTarget)
    {
        if(foundTarget)
        {
            GridHex<GridPosition> map = movingUnit.gameManager.grid;
            map.GetXY(movingUnit.transform.position, out int x, out int y);
            Vector3Int movingUnitCube =  map.OffsetToCube(x, y);

            map.GetXY(targetUnit.transform.position, out x, out y);
            map.OffsetToCube(x, y);
            Vector3Int targetUnitCube = map.OffsetToCube(x, y);
            int distanceBetweenUnits =  map.CubeDistance(movingUnitCube, targetUnitCube);

            Debug.Log(distanceBetweenUnits);
            if (targetUnit != null && distanceBetweenUnits <= range)
            {
                Debug.Log("Hit");
                MeleeAttackAnimation meleeAttackAnimation = (MeleeAttackAnimation) Instantiate(this.animation);
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
        
        Tuple<int, int, List<AttackDataUI>> attackData = targetUnit.CalculateEstimatedDamage(adjustedMinimumDamage, adjustmedMaximumDamage, true);

        adjustedMinimumDamage = attackData.Item1;
        adjustmedMaximumDamage = attackData.Item2;
        mainAttack.data = adjustedMinimumDamage.ToString() + " - " + adjustmedMaximumDamage.ToString();
        mainAttack.min = adjustedMinimumDamage;
        mainAttack.max = adjustmedMaximumDamage;
        mainAttack.attackDataType = attackDataType.Main;
        mainAttack.attackState = attackState.Benign;
        
        List<AttackDataUI> allAttackData =  new List<AttackDataUI>();
        allAttackData.Add(mainAttack);

        for (int i = 0; i < attackData.Item3.Count; i++)
        {
            allAttackData.Add(attackData.Item3[i]);
        }

        return allAttackData;
    }
}
