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
    public float effectAgainstArmorPercentage = 1f;
    public bool ignoreArmor = false;

    public override int CalculateWeight(AIActionData AiActionData)
    {
        if (!CheckActionUsable(AiActionData.unit))
        {
            return -2;
        }
        CombatGameManager gameManager = AiActionData.unit.gameManager;
        int highestActionValue = -2;
        Unit originUnit = AiActionData.unit;

        if (AiActionData.canMove)
        {
            for (int k = 0; k < AiActionData.enemyUnits.Count; k++)
            {
                int x = AiActionData.enemyUnits[k].x;
                int y = AiActionData.enemyUnits[k].y;
                int targetElevation = gameManager.spriteManager.elevationOfHexes[x, y];
                Unit targetUnit = gameManager.grid.GetGridObject(x, y).unit;
                DijkstraMapNode currentNode = gameManager.map.getGrid().GetGridObject(x, y);
                int highestValidElevation = -1;
                for (int i = 1; i <= range; i++)
                {
                    List<DijkstraMapNode> mapNodes = gameManager.map.getGrid().GetGridObjectsInRing(x, y, i);
                    int lowestActionPointsUsedForMovement = int.MaxValue;
                    for (int j = 0; j < mapNodes.Count; j++)
                    {
                        Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                        int originElevation = gameManager.spriteManager.elevationOfHexes[mapNodes[j].x, mapNodes[j].y];

                        if (gameManager.grid.GetGridObject(currentNodePosition.x, currentNodePosition.y).CheckIfTileIsEmpty() &&
                            AiActionData.unit.moveModifier.validElevationDifference(gameManager, currentNode, mapNodes[j], range)
                            && CheckIfTileIsInRange(currentNodePosition, AiActionData) && originElevation > highestValidElevation)
                        {
                            lowestActionPointsUsedForMovement = AiActionData.movementData[mapNodes[j].x, mapNodes[j].y];
                            AttackData attackData = CalculateAttackData(originUnit, targetUnit);
                            Tuple<int, int> expectedTargetDamage = targetUnit.GetEstimatedDamageValues(attackData);
                            bool targetExpectedToDie = expectedTargetDamage.Item1 >= targetUnit.currentHealth;
                            int tempActionValue = -2;
                            tempActionValue += (int)(expectedTargetDamage.Item1 * gameManager.healthDamageModifier);
                            tempActionValue += expectedTargetDamage.Item2;
                            if (targetExpectedToDie)
                            {
                                tempActionValue += gameManager.killValue;
                            }
                            tempActionValue = (int)(tempActionValue * targetUnit.targetValue);
                            tempActionValue = AiActionData.ModifyActionValue(AiActionData, currentNodePosition, this, tempActionValue);

                            if (highestActionValue < tempActionValue)
                            {
                                highestActionValue = tempActionValue;
                                AiActionData.desiredEndPosition = currentNodePosition;
                                AiActionData.desiredTargetPositionEnd = AiActionData.enemyUnits[k];
                                AiActionData.action = this;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            int x = AiActionData.unit.x;
            int y = AiActionData.unit.y;
            DijkstraMapNode currentNode = gameManager.map.getGrid().GetGridObject(x, y);
            List<DijkstraMapNode> mapNodes = gameManager.map.getGrid().GetGridObjectsInRing(x, y, 1);
            for (int j = 0; j < mapNodes.Count; j++)
            {
                Vector2Int targetUnitPosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                int originElevation = gameManager.spriteManager.elevationOfHexes[mapNodes[j].x, mapNodes[j].y];
                Unit targetUnit = gameManager.grid.GetGridObject(targetUnitPosition.x, targetUnitPosition.y).unit;
                if ( targetUnit != null && targetUnit.team != AiActionData.unit.team &&
                    AiActionData.unit.moveModifier.validElevationDifference(gameManager, currentNode, mapNodes[j], range))
                {
                    AttackData attackData = CalculateAttackData(originUnit, targetUnit);
                    Tuple<int, int> expectedTargetDamage = targetUnit.GetEstimatedDamageValues(attackData);
                    bool targetExpectedToDie = expectedTargetDamage.Item1 >= targetUnit.currentHealth;
                    int tempActionValue = -2;
                    tempActionValue += (int)(expectedTargetDamage.Item1 * gameManager.healthDamageModifier);
                    tempActionValue += expectedTargetDamage.Item2;
                    if (targetExpectedToDie)
                    {
                        tempActionValue += gameManager.killValue;
                    }
                    tempActionValue = (int)(tempActionValue * targetUnit.targetValue);
                    tempActionValue = AiActionData.ModifyActionValue(AiActionData, targetUnitPosition, this, tempActionValue);

                    if (highestActionValue < tempActionValue)
                    {
                        highestActionValue = tempActionValue;
                        AiActionData.desiredEndPosition = targetUnitPosition;
                        AiActionData.desiredTargetPositionEnd = targetUnitPosition;
                        AiActionData.action = this;
                    }
                }
            }
        }
        return highestActionValue;
    }

    public override bool CheckIfActionIsInRange(AIActionData AiActionData)
    {
        CombatGameManager gameManager = AiActionData.unit.gameManager;
        for(int k = 0; k < AiActionData.enemyUnits.Count; k++)
        {
            int x = AiActionData.enemyUnits[k].x;
            int y = AiActionData.enemyUnits[k].y; 
            int originElevation = gameManager.spriteManager.elevationOfHexes[x, y];
            for (int i = 1; i <= range; i++)
            {
                List<DijkstraMapNode> mapNodes = gameManager.map.getGrid().GetGridObjectsInRing(x, y, i);
                for (int j = 0; j < mapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                    int targetElevation = gameManager.spriteManager.elevationOfHexes[mapNodes[j].x, mapNodes[j].y];
                    if (gameManager.grid.GetGridObject(currentNodePosition.x, currentNodePosition.y).CheckIfTileIsEmpty() &&
                        (originElevation == targetElevation || (i == 1 && Mathf.Abs(originElevation - targetElevation) <= range))
                        && CheckIfTileIsInRange(currentNodePosition, AiActionData))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
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

    public override void AIUseAction(AIActionData AIActionData)
    {
        Vector2Int targetHex = AIActionData.desiredTargetPositionEnd;
        Vector2Int endPosition = AIActionData.desiredEndPosition;

        List<Action> movementActions = AIActionData.movementActions[endPosition.x, endPosition.y];
        for(int i = 0; i < movementActions.Count; i++)
        {
            movementActions[i].AIUseAction(AIActionData);
        }

        Unit movingUnit = AIActionData.unit;
        Unit targetUnit = movingUnit.gameManager.grid.GetGridObject(targetHex).unit;
        movingUnit.gameManager.grid.GetXY(movingUnit.transform.position, out int x, out int y);
        ActionData actionData = new ActionData();
        actionData.action = this;
        actionData.actingUnit = movingUnit;
        actionData.affectedUnits.Add(targetUnit);
        actionData.originLocation = new Vector2Int(x, y);
        movingUnit.gameManager.AddActionToQueue(actionData, false, false);
        movingUnit.gameManager.PlayActions();

    }

    public override void SelectAction(Unit self)
    {
        base.SelectAction(self);
        int actionIndex = GetActionIndex(self);
        int amountOfActionPointsUsed = this.intialActionPointUsage + actionPointGrowth * self.actions[actionIndex].amountUsedDuringRound;
        self.gameManager.spriteManager.ActivateMeleeAttackTargeting(self, false, self.currentActionsPoints, amountOfActionPointsUsed, range,
            CalculateAttackDisplayData);
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


    public AttackData CalculateAttackData(Unit movingUnit, Unit targetUnit)
    {
        int adjustedMinimumDamage = (int)(minDamage * movingUnit.GetMinimumDamageModifer());
        int adjustmedMaximumDamage = (int)(maxDamage * movingUnit.GetMaximumDamageModifer());

        Damage mainDamage = new Damage();
        mainDamage.minDamage = adjustedMinimumDamage;
        mainDamage.maxDamage = adjustmedMaximumDamage;
        mainDamage.damageType = DamageTypes.physical;


        AttackData tempAttackData = new AttackData(new List<Damage>() { mainDamage}, effectAgainstArmorPercentage, movingUnit);
        tempAttackData.ignroeArmour = ignoreArmor;

        movingUnit.GetActionModifiers(this, tempAttackData);
        AttackData modifiedAttackData = tempAttackData.GetCalculatedAttackData(targetUnit);
        AttackData finalAttackData = modifiedAttackData.GetCalculatedAttackData(targetUnit);

        return finalAttackData;
    }

    //Ignore Path (useless parameter)
    public List<AttackDataUI> CalculateAttackDisplayData(Unit movingUnit, Unit targetUnit, List<Vector2Int> path)
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
            Debug.Log("ACtions in Queue: " + targetUnit.gameManager.actionsInQueue.Count);
            for (int i = 0; i < targetUnit.gameManager.actionsInQueue.Count; i++)
            {
                if (targetUnit.gameManager.actionsInQueue[i].actingUnit == targetUnit)
                {
                    targetUnit.gameManager.actionsInQueue.Remove(targetUnit.gameManager.actionsInQueue[i]);
                    i--;
                }
            }
            Debug.Log("ACtions in Queue: After " + targetUnit.gameManager.actionsInQueue.Count);
            List<AttackDataUI> attackDatas = CalculateAttackDisplayData(actionData.actingUnit, targetUnit, null);
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
