using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        Unit actingUnit = actionData.unit;
        List<EquipableAmmoSO> unitAmmo = new List<EquipableAmmoSO>();

        CheckCorrectAmmo(actingUnit.Item1, 0, unitAmmo, actingUnit);
        CheckCorrectAmmo(actingUnit.Item2, 1, unitAmmo, actingUnit);
        CheckCorrectAmmo(actingUnit.Item3, 2, unitAmmo, actingUnit);
        CheckCorrectAmmo(actingUnit.Item4, 3, unitAmmo, actingUnit);

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
            Debug.LogError("this should not happen, no ammo should have been detected in Check if Action is in Range not here");
            return -1;
        }


        CombatGameManager gameManager = actingUnit.gameManager;
        DijkstraMap map = gameManager.map;

        map.ResetMap(true, false);
        List<Vector2Int> moveablePositions;
        if (actionData.canMove)
        {
            moveablePositions = actionData.hexesUnitCanMoveTo[0];
        }
        else
        {
            moveablePositions = new List<Vector2Int>() { new Vector2Int(actingUnit.x, actingUnit.y) };
        }
        Debug.Log("Chekcing Weight");
        //Get Nodes that can be targeted by the acting unit
        List<DijkstraMapNode> nodesInTargetRange = map.GetNodesInTargetRange(moveablePositions, 0, actionData.enemyUnits, gameManager, actingUnit.moveModifier, maxRange);

        List<Vector2Int> positionsInTargetRange = new List<Vector2Int>();
        for (int i = 0; i < nodesInTargetRange.Count; i++)
        {
            positionsInTargetRange.Add(new Vector2Int(nodesInTargetRange[i].x, nodesInTargetRange[i].y));
        }
        List<Unit> enemyUnitsInRange =  new List<Unit>();
        for (int i = 0; i < actionData.enemyUnits.Count; i++)
        {
            if (positionsInTargetRange.Contains(actionData.enemyUnits[i]))
            {
                enemyUnitsInRange.Add(gameManager.grid.GetGridObject(actionData.enemyUnits[i]).unit);
            }
        }

        Dictionary<Team, Dictionary<MoveModifier, List<Unit>>> unitsOrganizedByMovement = new Dictionary<Team, Dictionary<MoveModifier, List<Unit>>>();
        List<Vector2Int> enemyPositionsInTargetRange = new List<Vector2Int>();
        foreach(Unit unit in enemyUnitsInRange)
        {
            enemyPositionsInTargetRange.Add(new Vector2Int(unit.x, unit.y));
            if (unitsOrganizedByMovement.ContainsKey(unit.team))
            {
                if (unitsOrganizedByMovement[unit.team].ContainsKey(unit.moveModifier))
                {
                    unitsOrganizedByMovement[unit.team][unit.moveModifier].Add(unit);
                }
                else
                {
                    unitsOrganizedByMovement[unit.team].Add(unit.moveModifier, new List<Unit> { unit });
                }
            }
            else
            {
                unitsOrganizedByMovement.Add(unit.team, new Dictionary<MoveModifier, List<Unit>>());
                unitsOrganizedByMovement[unit.team].Add(unit.moveModifier, new List<Unit> { unit });
            }
        }

        map.ResetMap(true, false);
        foreach (Team team in unitsOrganizedByMovement.Keys)
        {
            foreach (MoveModifier movemodifier in unitsOrganizedByMovement[team].Keys)
            {
                Unit tempUnit = unitsOrganizedByMovement[team][movemodifier][0];
                tempUnit.moveModifier.SetUnwalkable(gameManager, tempUnit);
                List<Vector2Int> unitLocations = new List<Vector2Int>();
                for(int i = 0; i < unitsOrganizedByMovement[team][movemodifier].Count; i++)
                {
                    Unit unit = unitsOrganizedByMovement[team][movemodifier][i];
                    unitLocations.Add(new Vector2Int(unit.x, unit.y));
                }
                map.SetGoalsNew(unitLocations, gameManager, movemodifier);

                tempUnit.moveModifier.SetWalkable(gameManager, tempUnit);
            }
        }
        int[,] threatGrid =  map.GetGridValues();

        //Get nodes that are in range of enemy units
        map.ResetMap(true, false);
        map.GetNodesInTargetRange(enemyPositionsInTargetRange, 0, actionData.enemyUnits, gameManager, actingUnit.moveModifier, maxRange);
        GridHex<DijkstraMapNode> grid = map.getGrid();
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        int totalMovingUnitHealth = actingUnit.currentHealth + actingUnit.currentArmor;
        int highestActionValue =  actionData.highestActionWeight;
        for (int i = 0; i < positionsInTargetRange.Count; i++)
        {
            //Check to see if node can hit a target
            DijkstraMapNode currentNode = map.getGrid().GetGridObject(positionsInTargetRange[i]);
            if (currentNode.value == -1)
            {
                continue;
            }
            Vector2Int newPosition = positionsInTargetRange[i];
            List<Action> movementActions = actionData.movementActions[newPosition.x, newPosition.y];
            actionData.prediction = new ActionPredictionData();
            if(movementActions != null)
            {
                for (int j = 0; j < movementActions.Count; j++)
                {
                    movementActions[j].CalculateActionConsequences(actionData, newPosition);
                }
            }

            float actionConsequenceModifier = (float)(totalMovingUnitHealth - actionData.prediction.Damage) / totalMovingUnitHealth;
            if (actionConsequenceModifier <= 0)
            {
                continue;
            }
            for (int j = 0; j < enemyUnitsInRange.Count; j++)
            {
                Unit enemyUnit = enemyUnitsInRange[j]; 
                Vector2Int enemyPosition = new Vector2Int(enemyUnit.x, enemyUnit.y);
                float heightModifier = 1f + ((elevationGrid[newPosition.x, newPosition.y] - elevationGrid[enemyPosition.x, enemyPosition.y]) * .2f);


                float coverModifer = 1;
                if (CheckIfUnitIsInCover(newPosition, enemyPosition, gameManager))
                {
                    coverModifer = 0.5f;
                }

                float threatModifier = 1.33f;
                //
                if (threatGrid[newPosition.x, newPosition.y] <= 18)
                {
                    threatModifier = 0.67f;
                }
                else if(threatGrid[newPosition.x, newPosition.y] <= 36)
                {
                    threatModifier = 1;
                }

                for(int k = 0; k < unitAmmo.Count; k++)
                {
                    if(unitAmmo[k] == null)
                    {
                        continue;
                    }
                    EquipableAmmoSO ammoData = unitAmmo[k];

                    int modifiedEffectiveRange =  (int) (effectiveRange * ammoData.rangeModifier);
                    int modifiedMaxRange = (int)(maxRange * ammoData.rangeModifier);
                    int hexDistanceFromTarget = grid.OffsetDistance(enemyPosition, newPosition);
                    float rangeModifier = 1f;
                    if (hexDistanceFromTarget > modifiedEffectiveRange)
                    {
                        rangeModifier = 0.5f;
                    }
                    else if (hexDistanceFromTarget > modifiedMaxRange)
                    {
                        continue;
                    }

                    if(ammoData.ignoreCoverModifer)
                    {
                        coverModifer = 1f;
                    }
                    if(ammoData.ignoreRangeModifier)
                    {
                        rangeModifier = 1f;
                    }

                    // have threat modifier used after the cull so ranged units incentivied to move to a much better shooting position
                    float totalModifiers = actionConsequenceModifier * rangeModifier * coverModifer * heightModifier * threatModifier;
                    //Debug.Log("test 4: " + positionsInTargetRange[i] + ", " + tempActionValue + ", " + actionConsequenceModifier + ", " + rangeModifier + ", " + coverModifer + ", " + heightModifier + ", " + threatModifier);
                    if(totalModifiers < 0.33f)
                    {
                        continue;
                    }
                    totalModifiers *= threatModifier;   
                    Debug.Log("test 4: " + positionsInTargetRange[i] + ", " + actionConsequenceModifier + ", " + rangeModifier + ", " + coverModifer + ", " + heightModifier + ", " + threatModifier);
                    AttackData currentAttackData = CalculateAttackData(actingUnit, enemyUnit, k);
                    unitAmmo[k].ModifyAttack(currentAttackData);
                    Tuple<int, int, List<Status>> expectedTargetDamage = enemyUnit.GetEstimatedDamageValues(currentAttackData);
                    bool targetExpectedToDie = expectedTargetDamage.Item1 >= enemyUnit.currentHealth;
                    int tempActionValue = -2;
                    tempActionValue += (int)(expectedTargetDamage.Item1 * gameManager.healthDamageModifier);
                    tempActionValue += expectedTargetDamage.Item2;
                    if (targetExpectedToDie)
                    {
                        tempActionValue += gameManager.killValue;
                    }
                    
                    tempActionValue = (int)(tempActionValue * enemyUnit.targetValue);
                    //Debug.Log("test 2: " + positionsInTargetRange[i] + ", " + tempActionValue);
                    tempActionValue = actionData.ModifyActionValue(actionData, newPosition, this, tempActionValue);
                    //Debug.Log("test 3: " + positionsInTargetRange[i] + ", " + tempActionValue);
                    tempActionValue = (int)(tempActionValue * actionConsequenceModifier * rangeModifier * coverModifer * heightModifier * threatModifier);   
                    if (highestActionValue < tempActionValue)
                    {
                        highestActionValue = tempActionValue;
                        actionData.desiredEndPosition = newPosition;
                        actionData.desiredTargetPositionEnd = new Vector2Int(enemyUnit.x, enemyUnit.y);
                        actionData.action = this;
                        actionData.itemIndex = k;
                    }
                }
            }
        }
        return highestActionValue;
    }

    public override void FindOptimalPosition(AIActionData actionData)
    {
        return;
    }

    public override bool CheckIfActionIsInRange(AIActionData actionData)
    {
        Unit actingUnit = actionData.unit;

        if(actionData.expectedCurrentActionPoints < actionPointUsage)
        {
            return false;
        }

        List<EquipableAmmoSO> unitAmmo = new List<EquipableAmmoSO>();

        CheckCorrectAmmo(actingUnit.Item1, 0, unitAmmo, actingUnit);
        CheckCorrectAmmo(actingUnit.Item2, 1, unitAmmo, actingUnit);
        CheckCorrectAmmo(actingUnit.Item3, 2, unitAmmo, actingUnit);
        CheckCorrectAmmo(actingUnit.Item4, 3, unitAmmo, actingUnit);

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
            Debug.Log("No AMmo");
            return false;
        }

        CombatGameManager gameManager = actingUnit.gameManager;     
        DijkstraMap map = gameManager.map;
        map.ResetMap(true, false);
        List<Vector2Int> moveablePositions;
        if(actionData.canMove)
        {
            moveablePositions = actionData.hexesUnitCanMoveTo[0];
        }
        else
        {
            moveablePositions = new List<Vector2Int>() { new Vector2Int(actingUnit.x, actingUnit.y)};
        }
        List<DijkstraMapNode> nodes = map.GetNodesInTargetRange(moveablePositions, 0, actionData.enemyUnits, gameManager, actingUnit.moveModifier, maxRange);
        List<Vector2Int> positionsInTargetRange = new List<Vector2Int>();
        for(int i = 0; i < nodes.Count; i++)
        {
            positionsInTargetRange.Add(new Vector2Int(nodes[i].x, nodes[i].y));
        }
        for(int i = 0; i < actionData.enemyUnits.Count; i++)
        {
            if (positionsInTargetRange.Contains(actionData.enemyUnits[i]))
            {
                return true;
            }
        }

        return false;
    }

    public override void AIUseAction(AIActionData AiactionData, bool finalAction = false)
    {
        Vector2Int targetHex = AiactionData.desiredTargetPositionEnd;
        Vector2Int endPosition = AiactionData.desiredEndPosition;

        List<Action> movementActions = AiactionData.movementActions[endPosition.x, endPosition.y];
        if(movementActions != null )
        {
            for (int i = 0; i < movementActions.Count; i++)
            {
                movementActions[i].AIUseAction(AiactionData);
            }
        }

        Unit movingUnit = AiactionData.unit;
        Unit targetUnit = movingUnit.gameManager.grid.GetGridObject(targetHex).unit;
        movingUnit.gameManager.grid.GetXY(movingUnit.transform.position, out int x, out int y);
        ActionData actionData = new ActionData();
        actionData.action = this;
        actionData.actingUnit = movingUnit;
        actionData.affectedUnits.Add(targetUnit);
        actionData.itemIndex = AiactionData.itemIndex;
        actionData.originLocation = new Vector2Int(x, y);
        movingUnit.gameManager.AddActionToQueue(actionData, false, false);
        movingUnit.gameManager.PlayActions();
    }

    public override void SelectAction(Unit self)
    {
        Debug.Log("select Ranged ATtack");
        base.SelectAction(self);
        int actionIndex = GetActionIndex(self);
        int amountOfActionPointsUsed = this.actionPointUsage;

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
        self.gameManager.spriteManager.ActivateRangedTargeting(self, false, self.currentMajorActionsPoints, amountOfActionPointsUsed, effectiveRange,
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

    public void FoundTarget(Unit movingUnit, Unit targetUnit, bool foundTarget, int itemIndex)
    {
        Debug.Log("Found Target");
        if (foundTarget)
        {
            movingUnit.gameManager.grid.GetXY(movingUnit.transform.position, out int x, out int y);
            ActionData actionData = new ActionData();
            actionData.action = this;
            actionData.actingUnit = movingUnit;
            actionData.affectedUnits.Add(targetUnit);
            actionData.originLocation = new Vector2Int(x, y);
            actionData.itemIndex = itemIndex;
            movingUnit.gameManager.AddActionToQueue(actionData, false, false);
            movingUnit.gameManager.PlayActions();
        }
        else
        {
            movingUnit.UseActionPoints(0);
        }
        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
    }


    public AttackData CalculateAttackData(Unit movingUnit, Unit targetUnit, int itemIndex)
    {
        int adjustedMinimumDamage = (int)(minDamage * movingUnit.GetMinimumDamageModifer());
        int adjustmedMaximumDamage = (int)(maxDamage * movingUnit.GetMaximumDamageModifer());

        Damage mainDamage = new Damage();
        mainDamage.minDamage = adjustedMinimumDamage;
        mainDamage.maxDamage = adjustmedMaximumDamage;
        mainDamage.damageType = DamageTypes.physical;


        AttackData tempAttackData = new AttackData(new List<Damage>() { mainDamage }, effectAgainstArmorPercentage, movingUnit);
        tempAttackData.ignoreArmour = ignoreArmor;
        tempAttackData.meleeContact = false;
        tempAttackData.ignoreShield = false;

        EquipableAmmoSO newAmmo = null;
        switch (itemIndex)
        {
            case 0:
                newAmmo = (EquipableAmmoSO)(movingUnit.Item1);
                break;
            case 1:
                newAmmo = (EquipableAmmoSO)(movingUnit.Item2);
                break;
            case 2:
                newAmmo = (EquipableAmmoSO)(movingUnit.Item3);
                break;
            case 3:
                newAmmo = (EquipableAmmoSO)(movingUnit.Item4);
                break;
        }
        newAmmo.ModifyAttack(tempAttackData);
        movingUnit.GetActionModifiers(this, tempAttackData);
        return tempAttackData;
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

        List<AttackDataUI> allAttackData = new List<AttackDataUI>();
        allAttackData.Add(mainAttack);

        for (int i = 0; i < attackData.Item3.Count; i++)
        {
            allAttackData.Add(attackData.Item3[i]);
        }

        return allAttackData;
    }

    public override void ConfirmAction(ActionData actionData)
    {
        Unit movingUnit = actionData.actingUnit;
        Unit targetUnit = actionData.affectedUnits[0];
        GridHex<GridPosition> map = movingUnit.gameManager.grid;
        map.GetXY(movingUnit.transform.position, out int x, out int y);
        Vector3Int movingUnitCube = map.OffsetToCube(x, y);

        map.GetXY(targetUnit.transform.position, out x, out y);
        map.OffsetToCube(x, y);
        Vector3Int targetUnitCube = map.OffsetToCube(x, y);
        int distanceBetweenUnits = map.CubeDistance(movingUnitCube, targetUnitCube);

        Debug.Log(distanceBetweenUnits);
        if (targetUnit != null && distanceBetweenUnits <= maxRange)
        {
            Debug.Log("Hit");

            List<AttackDataUI> attackDatas = CalculateAttackDisplayData(actionData.actingUnit, targetUnit, null);
            targetUnit.gameManager.spriteManager.ActivateCombatAttackUI(targetUnit, attackDatas, targetUnit.transform.position);

            MeleeAttackAnimation meleeAttackAnimation = (MeleeAttackAnimation)Instantiate(this.animation);
            meleeAttackAnimation.SetParameters(movingUnit, movingUnit.transform.position, targetUnit.transform.position);
            EquipableAmmoSO newAmmo;
            switch (actionData.itemIndex)
            {
                case 0:
                    newAmmo = (EquipableAmmoSO) (movingUnit.Item1);
                    break;
                case 1:
                    newAmmo = (EquipableAmmoSO)(movingUnit.Item2);
                    break;
                case 2:
                    newAmmo = (EquipableAmmoSO)(movingUnit.Item3);
                    break;
                case 3:
                    newAmmo = (EquipableAmmoSO)(movingUnit.Item4);
                    break;
            }
            AttackData currentAttackData =  CalculateAttackData(movingUnit, targetUnit, actionData.itemIndex);
            targetUnit.TakeDamage(movingUnit, currentAttackData, true);
            UseActionPreset(movingUnit);
        }
        else
        {
            movingUnit.UseActionPoints(0);
        }
    }

}

