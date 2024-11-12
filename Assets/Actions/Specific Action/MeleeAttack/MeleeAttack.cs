using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/MeleeAttack")]
public class MeleeAttack : Action
{
    public int minDamage;
    public int maxDamage;
    public int range = 1;
    public float effectAgainstArmorPercentage;
    public bool ignoreArmor = false;

    public override int CalculateWeight(AIActionData actionData)
    {
        throw new System.NotImplementedException();
    }

    public override void FindOptimalPosition(AIActionData actionData)
    {
        /*
        Dictionary<Vector2Int, int> gridValues =  new Dictionary<Vector2Int, int>();
        Unit movingUnit =  actionData.unit;
        List<Vector2Int> mapNodes;
        Vector2Int newPosition;
        for(int i = 0; i < actionData.enemyUnits.Count; i++)
        {
            newPosition = new Vector2Int(actionData.enemyUnits[i].x, actionData.enemyUnits[i].y);
            for (int j = 1; j <= range; j++)
            {
                mapNodes = movingUnit.gameManager.map.getGrid().GetGridPositionsInRing(newPosition.x, newPosition.y, j);
                if (j == 1)
                {

                }
                else
                {
                    for (int k = 0; k < mapNodes.Count; k++)
                    {
                        Vector2Int surroundingNodePosition = new Vector2Int(mapNodes[k].x, mapNodes[k].y);
                        if (movingUnit.moveModifier.ValidMeleeAttack(movingUnit.gameManager, currentunitNode, surroundingUnitNode, range))
                        {
                            movingUnit.passiveEffects[passiveIndex].passiveLocations.Add(surroundingNodePosition);
                        }
                    }
                }
            }
        }
        */
    }

    public override void SelectAction(Unit self)
    {
        base.SelectAction(self);
        int actionIndex = GetActionIndex(self);
        int amountOfActionPointsUsed = this.intialActionPointUsage + actionPointGrowth * self.actions[actionIndex].amountUsedDuringRound;
        self.gameManager.spriteManager.ActivateMeleeAttackTargeting(self, false, self.currentActionsPoints, amountOfActionPointsUsed, range,
            CalculateAttackData);
        self.gameManager.spriteManager.meleeTargeting.OnFoundTarget += FoundTarget;
    }

    public void FoundTarget(Unit movingUnit, Unit targetUnit, bool foundTarget)
    {
        if(foundTarget)
        {
            movingUnit.gameManager.grid.GetXY(movingUnit.transform.position, out int x, out int y);
            ActionData actionData = new ActionData();
            actionData.action = this;
            actionData.actingUnit = movingUnit;
            actionData.affectedUnits.Add(targetUnit);
            actionData.originLocation = new Vector2Int(x, y);
            movingUnit.gameManager.AddActionToQueue(actionData, false, false);
            movingUnit.gameManager.PlayActions();
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
    public override void ConfirmAction(ActionData actionData)
    {
        Unit targetUnit = actionData.affectedUnits[0];
        GridHex<GridPosition> map = actionData.actingUnit.gameManager.grid;
        map.GetXY(actionData.actingUnit.transform.position, out int x, out int y);
        Vector3Int movingUnitCube = map.OffsetToCube(x, y);

        map.GetXY(targetUnit.transform.position, out x, out y);
        map.OffsetToCube(x, y);
        Vector3Int targetUnitCube = map.OffsetToCube(x, y);
        int distanceBetweenUnits = map.CubeDistance(movingUnitCube, targetUnitCube);

        Debug.Log(distanceBetweenUnits);
        if (targetUnit != null && distanceBetweenUnits <= range)
        {
            Debug.Log("Hit");
            for (int i = 0; i < targetUnit.gameManager.actionsInQueue.Count; i++)
            {
                if (targetUnit.gameManager.actionsInQueue[i].actingUnit == targetUnit)
                {
                    targetUnit.gameManager.actionsInQueue.Remove(targetUnit.gameManager.actionsInQueue[i]);
                }
            }

            List<AttackDataUI> attackDatas = CalculateAttackData(actionData.actingUnit, targetUnit, null);
            targetUnit.gameManager.spriteManager.ActivateCombatAttackUI(targetUnit, attackDatas, targetUnit.transform.position);


            MeleeAttackAnimation meleeAttackAnimation = (MeleeAttackAnimation)Instantiate(this.animation);
            meleeAttackAnimation.SetParameters(actionData.actingUnit.gameManager, actionData.actingUnit.transform.position, targetUnit.transform.position);
            int adjustedMinimumDamage = minDamage + (int)(minDamage * actionData.actingUnit.GetMinimumDamageModifer());
            int adjustmedMaximumDamage = maxDamage + (int)(maxDamage * actionData.actingUnit.GetMaximumDamageModifer());
            targetUnit.TakeDamage(actionData.actingUnit, new List<int>() { adjustedMinimumDamage }, new List<int>() { adjustmedMaximumDamage },
                true, false, ignoreArmor, true);
            UseActionPreset(actionData.actingUnit);
        }
    }
}
