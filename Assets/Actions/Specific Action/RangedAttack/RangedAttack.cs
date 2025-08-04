using Inventory.Model;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Action;
using static SpriteManager;

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
        CalculatedActionData calculatedData;
        if (actionData.AIState == AITurnStates.Skirmish)
        {
            calculatedData = SkirmishPreset(actionData, false);
        }
        else
        {
            calculatedData = CombatFiringPosition(actionData);
        }
        int highestActionValue = -2;
        if (calculatedData.completedCalculation)
        {
            highestActionValue = calculatedData.highestActionValue;
            actionData.desiredEndPosition = calculatedData.desiredEndPosition;
            actionData.desiredTargetPositionEnd = new List<Vector2Int>() { calculatedData.desiredTargetPositionEnd };
            actionData.action = calculatedData.action;
            actionData.action = calculatedData.action;
            actionData.itemIndex = calculatedData.itemIndex;
        }

        return highestActionValue;
    }

    public override int CalculateEnvironmentWeight(AIActionData AiActionData)
    {
        return (minDamage + maxDamage) / 2;
    }

    // Should only be called if no enemies in range
    public override void FindOptimalPosition(AIActionData actionData)
    {
        CalculatedActionData calculatedData = new CalculatedActionData();
        switch (actionData.AIState)
        {
            case (AITurnStates.Combat):
                calculatedData = CombatFiringOptimalPosition(actionData);
                break;
            case (AITurnStates.Skirmish):
                calculatedData = SkirmishPreset(actionData, true);
                break;
            case (AITurnStates.Agressive):
                calculatedData = CombatFiringOptimalPosition(actionData);
                break;
            case (AITurnStates.SuperiorRanged):
                break;
            case (AITurnStates.Convoy):
                Debug.LogWarning("Shouldn't be happening");
                break;
        }

        if (calculatedData.completedCalculation)
        {
            actionData.desiredEndPosition = calculatedData.desiredEndPosition;
            actionData.desiredTargetPositionEnd = new List<Vector2Int>() { calculatedData.desiredTargetPositionEnd };
            actionData.action = calculatedData.action;
            actionData.itemIndex = calculatedData.itemIndex;
        }

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
        List<Vector2Int> moveablePositions = new List<Vector2Int>();
        if(actionData.canMove)
        {
            for(int i = 0; i < actingUnit.currentMajorActionsPoints; i++)
            {
                for(int j = 0; j < actionData.hexesUnitCanMoveTo[i].Count; j++)
                {
                    moveablePositions.Add(actionData.hexesUnitCanMoveTo[i][j]);
                }
            }
        }
        else
        {
            moveablePositions = new List<Vector2Int>() { new Vector2Int(actingUnit.x, actingUnit.y)};
        }
        List<DijkstraMapNode> nodes = map.GetNodesInRangedRange(moveablePositions, 0, actionData.enemyUnits, gameManager, actingUnit.moveModifier, maxRange);
        List<Vector2Int> positionsInTargetRange = new List<Vector2Int>();
        for(int i = 0; i < nodes.Count; i++)    
        {
            positionsInTargetRange.Add(new Vector2Int(nodes[i].x, nodes[i].y));
            //Debug.Log("Positions in Range: " + positionsInTargetRange[i]);
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
        Unit movingUnit = AiactionData.unit;
        if (AiactionData.desiredTargetPositionEnd == null || AiactionData.desiredTargetPositionEnd.Count == 0)
        {
            Debug.LogError("This should not happen TargetPosition Was not found when action called: " + name);
            movingUnit.EndTurnAction();
        }
        Vector2Int targetHex = AiactionData.desiredTargetPositionEnd[0];
        Vector2Int endPosition = AiactionData.desiredEndPosition;

        List<Action> movementActions = AiactionData.movementActions[endPosition.x, endPosition.y];
        if(movementActions != null )
        {
            for (int i = 0; i < movementActions.Count; i++)
            {
                movementActions[i].AIUseAction(AiactionData);
            }
        }   

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

        Damage mainDamage = new Damage(minDamage, maxDamage, DamageTypes.physical);

        AttackData newAttackData = new AttackData(new List<Damage>() {mainDamage}, effectAgainstArmorPercentage, self);
        self.GetActionModifiers(this, newAttackData);
        Debug.Log("amOUNT OF dAAmage numbers: " +  newAttackData.allDamage.Count);
        RangedTargetingData rangedTargetingData = new RangedTargetingData(self, this, false, true, true, self.currentMajorActionsPoints, amountOfActionPointsUsed, effectiveRange, maxRange, 1,
            newAttackData, unitAmmo);

        self.gameManager.spriteManager.ActivateRangedTargeting(rangedTargetingData);
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

    public void FoundTarget(Unit movingUnit, List<Unit> targetUnits, bool foundTarget, int itemIndex)
    {
        Debug.Log("Found Target");
        if (foundTarget)
        {
            movingUnit.gameManager.grid.GetXY(movingUnit.transform.position, out int x, out int y);
            ActionData actionData = new ActionData();
            actionData.action = this;
            actionData.actingUnit = movingUnit;
            actionData.affectedUnits = new List<Unit>(targetUnits) {};
            Debug.Log("Num Targests: " + targetUnits.Count);
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
        Damage mainDamage = new Damage(adjustedMinimumDamage, adjustmedMaximumDamage, DamageTypes.physical);
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
        Debug.Log("Num Targests Confirm: " + actionData.affectedUnits.Count);
        Unit movingUnit = actionData.actingUnit;
        Unit targetUnit = actionData.affectedUnits[0];
        GridHex<GridPosition> map = movingUnit.gameManager.grid;
        map.GetXY(movingUnit.transform.position, out int x, out int y);
        Vector3Int movingUnitCube = map.OffsetToCube(x, y);

        map.GetXY(targetUnit.transform.position, out x, out y);
        map.OffsetToCube(x, y);
        Vector3Int targetUnitCube = map.OffsetToCube(x, y);
        int distanceBetweenUnits = map.CubeDistance(movingUnitCube, targetUnitCube);

        if (targetUnit != null && distanceBetweenUnits <= maxRange && movingUnit.currentMajorActionsPoints > 0)
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
            Debug.LogWarning("Tried to use Ranged Attacked, but failed");
            movingUnit.UseActionPoints(0);
        }
    }

    public List<EquipableAmmoSO> FiringPositionSetup(AIActionData actionData)
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
            return null;
        }
        return unitAmmo;
    }


    public void FindingFiringPosition(AIActionData actionData, RangedAttackFiringData firingData, CalculatedActionData calculatedActionData)
    {
        Vector2Int newPosition = firingData.newPosition;
        List<Unit> enemyUnitsInRange = firingData.enemyUnitsInRange;
        List<EquipableAmmoSO> unitAmmo = firingData.unitAmmo;
        float actionConsequence = firingData.actionConsequence;
        int[,] elevationGrid = firingData.elevationGrid;
        int[,] threatGrid = firingData.threatGrid;
        Unit actingUnit = actionData.unit;
        CombatGameManager gameManager = actingUnit.gameManager;
        GridHex<GridPosition> grid = gameManager.grid;
        //Debug.Log("Check: " + newPosition + ", " + actionData.movementData[newPosition.x, newPosition.y]);
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

            float threatModifier = 0.67f + firingData.threatModifer * ( ((float)(threatGrid[newPosition.x, newPosition.y] - 1)) / 18);
            //Debug.Log(" danger" + newPosition + " " + threatGrid[newPosition.x, newPosition.y]);
            if (threatModifier > 3)
            {
                threatModifier = 3;
            }

            for (int k = 0; k < unitAmmo.Count; k++)
            {
                if (unitAmmo[k] == null)
                {
                    continue;
                }
                EquipableAmmoSO ammoData = unitAmmo[k];

                int modifiedEffectiveRange = (int)(effectiveRange * ammoData.rangeModifier);
                int modifiedMaxRange = (int)(maxRange * ammoData.rangeModifier);
                int hexDistanceFromTarget = grid.OffsetDistance(enemyPosition, newPosition);
                float rangeModifier = 1f;
                //Debug.Log("test 3: " + newPosition + ", " + hexDistanceFromTarget + ", " + enemyPosition + ", " + newPosition);
                if (hexDistanceFromTarget > modifiedEffectiveRange)
                {
                    rangeModifier = 0.5f;
                }

                else if (hexDistanceFromTarget > modifiedMaxRange)
                {
                    continue;
                }

                if (ammoData.ignoreCoverModifer)
                {
                    coverModifer = 1f;
                }

                if (ammoData.ignoreRangeModifier)
                {
                    rangeModifier = 1f;
                }

                float totalModifiers = actionConsequence * rangeModifier * coverModifer * heightModifier * threatModifier;
                //Debug.Log("test 4: " + newPosition + ", " + actionConsequence + ", " + rangeModifier + ", " + coverModifer + ", " + heightModifier + ", " + threatModifier + ", Total Modifiers" + totalModifiers);
                /*
                // have threat modifier used after the cull so ranged units incentivied to move to a much better shooting position
                float totalModifiers = actionConsequence * rangeModifier * coverModifer * heightModifier;
                Debug.Log("test 4: " + newPosition +  ", " +  actionConsequence + ", "+ rangeModifier + ", " + coverModifer + ", " + heightModifier + ", " + threatModifier + ", Total Modifiers" + totalModifiers);
                if (totalModifiers < 0.5f)
                {
                    continue;
                }
                Debug.Log("test 4: " + newPosition + ", " + actionConsequence + ", " + rangeModifier + ", " + coverModifer + ", " + heightModifier + ", " + threatModifier);
                totalModifiers *= threatModifier;
                */
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
                //Debug.Log("test 2: " + newPosition + ", " + tempActionValue);
                tempActionValue = actionData.ModifyActionValue(actionData, newPosition, this, tempActionValue);
                //Debug.Log("test 3: " + newPosition + ", " + tempActionValue);
                tempActionValue = (int)(tempActionValue * totalModifiers);
                //Debug.Log("Test end Action Value: " + newPosition + ", " + tempActionValue + ", " + calculatedActionData.highestActionValue);   

                if (tempActionValue > calculatedActionData.highestActionValue)
                {
                   // Debug.Log("Current highest Value: " + newPosition + ", " + tempActionValue + ", " + calculatedActionData.highestActionValue);
                    calculatedActionData.completedCalculation = true;
                    calculatedActionData.highestActionValue = tempActionValue;
                    calculatedActionData.desiredEndPosition = newPosition;
                    calculatedActionData.desiredTargetPositionEnd = new Vector2Int(enemyUnit.x, enemyUnit.y);
                    calculatedActionData.action = this;
                    calculatedActionData.itemIndex = k;
                }
            }
        }
    }

    //no restricution on movement and find best firing position on  target
    // Find best target to hit through target value, in range, threat range, line of sight,
    // action cost for move and fire with bias towards moving to best position and firing rather than two subotpimal fires, (probably mroe intereseting)
    public CalculatedActionData CombatFiringPosition(AIActionData actionData)
    {
        CalculatedActionData calculatedActionData = new CalculatedActionData();
        calculatedActionData.completedCalculation = false;
        Unit actingUnit = actionData.unit;
        CombatGameManager gameManager = actingUnit.gameManager;
        DijkstraMap map = gameManager.map;
        List<EquipableAmmoSO> unitAmmo = FiringPositionSetup(actionData);

        if (unitAmmo == null)
        {
            Debug.LogError("this should not happen, no ammo should have been detected in Check if Action is in Range not here");
            return calculatedActionData;
        }

        map.ResetMap(true, false);
        List<Vector2Int> MovablePositions = new List<Vector2Int>();
        if (actionData.canMove)
        {
            for (int i = 0; i < actingUnit.currentMajorActionsPoints; i++)
            {
                for (int j = 0; j < actionData.hexesUnitCanMoveTo[i].Count; j++)
                {
                    MovablePositions.Add(actionData.hexesUnitCanMoveTo[i][j]);
                }
            }
        }
        else
        {
            MovablePositions = new List<Vector2Int>() { new Vector2Int(actingUnit.x, actingUnit.y) };
        }

        //Get Nodes that can be targeted by the acting unit
        List<DijkstraMapNode> nodesInTargetRange = map.GetNodesInRangedRange(MovablePositions, 0, actionData.enemyUnits, gameManager, actingUnit.moveModifier, maxRange);

        List<Vector2Int> positionsInTargetRange = new List<Vector2Int>();
        for (int i = 0; i < nodesInTargetRange.Count; i++)
        {
            positionsInTargetRange.Add(new Vector2Int(nodesInTargetRange[i].x, nodesInTargetRange[i].y));
        }

        // Generate Threat Grid
        List<Unit> enemyUnitsInRange = new List<Unit>();
        List<Vector2Int> enemyPositionsInTargetRange = new List<Vector2Int>();
        List<Unit> threateningEnemioe = new List<Unit>();
        for (int i = 0; i < actionData.enemyUnits.Count; i++)
        {
            Unit enemyUnit = gameManager.grid.GetGridObject(actionData.enemyUnits[i]).unit;
            if (positionsInTargetRange.Contains(actionData.enemyUnits[i]))
            {
                enemyUnitsInRange.Add(enemyUnit);
                enemyPositionsInTargetRange.Add(actionData.enemyUnits[i]);
                Debug.Log("Enemy Positions: " + enemyPositionsInTargetRange[i]);
            }

            if(!actionData.markedUnits.Contains(actionData.enemyUnits[i]))
            {
                threateningEnemioe.Add(enemyUnit);
            }
        }

        int[,] threatGrid = CreateThreatGrid(actionData, threateningEnemioe);

        //Get nodes that are in range of enemy units
        map.ResetMap(true, false);
        //Debug.Log("Max Range: " + maxRange);
        map.SetGoalForNodesInTargetRange(enemyPositionsInTargetRange, maxRange);
        int[,] rangeGrid =  map.GetGridValues();
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        int totalMovingUnitHealth = actingUnit.currentHealth + actingUnit.currentArmor;
        calculatedActionData.highestActionValue = actionData.highestActionWeight;

        RangedAttackFiringData firingData = new RangedAttackFiringData();
        firingData.enemyUnitsInRange = enemyUnitsInRange;
        firingData.unitAmmo = unitAmmo;
        firingData.elevationGrid = elevationGrid;
        firingData.threatGrid = threatGrid;
        firingData.threatModifer = 0.33f;

        for (int i = 0; i < positionsInTargetRange.Count; i++)
        {
            //Check to see if node can hit a target
            Vector2Int currentPosition = positionsInTargetRange[i];
            int rangeValue = rangeGrid[currentPosition.x, currentPosition.y];
            //Debug.Log("test 1: " + positionsInTargetRange[i] + ", "  + rangeValue);
            if (rangeValue == -1)
            {
                continue;
            }

            Vector2Int newPosition = positionsInTargetRange[i];
            List<Action> movementActions = actionData.movementActions[newPosition.x, newPosition.y];
            actionData.prediction = new ActionPredictionData();
            if (movementActions != null)
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

            firingData.newPosition = newPosition;
            firingData.actionConsequence = actionConsequenceModifier;
            FindingFiringPosition(actionData, firingData, calculatedActionData);
        }
        Debug.Log("Final Posisition: " + calculatedActionData.desiredEndPosition);
        return calculatedActionData;
    }

    //no restricution on movement and find best firing position on  target
    // Find best target to hit through target value, in range, threat range, line of sight,
    // action cost for move and fire with bias towards moving to best position and firing rather than two subotpimal fires, (probably mroe intereseting)
    public CalculatedActionData CombatFiringOptimalPosition(AIActionData actionData)
    {
        CalculatedActionData calculatedActionData = new CalculatedActionData();
        calculatedActionData.completedCalculation = false;
        Unit actingUnit = actionData.unit;
        CombatGameManager gameManager = actingUnit.gameManager;
        DijkstraMap map = gameManager.map;
        List<EquipableAmmoSO> unitAmmo = FiringPositionSetup(actionData);

        if (unitAmmo == null)
        {
            Debug.LogError("this should not happen, no ammo should have been detected in Check if Action is in Range not here");
            return calculatedActionData;
        }

        map.ResetMap(true, false);
        List<Vector2Int> targetPositions;
        if (actionData.enemyUnits.Count > 0)
        {
            targetPositions = actionData.enemyUnits;
        }
        else
        {
            targetPositions = actionData.enemyTeamStartingPositions;
        }
        //Get Nodes that can be targeted by the acting unit
        List<DijkstraMapNode> nodesInTargetRange = map.GetNodesInRangedRange(targetPositions, 0, actionData.enemyUnits, gameManager, actingUnit.moveModifier, maxRange);

        List<Vector2Int> positionsInTargetRange = new List<Vector2Int>();
        for (int i = 0; i < nodesInTargetRange.Count; i++)
        {
            //Debug.Log("Check Nodes in Range: " + new Vector2Int(nodesInTargetRange[i].x, nodesInTargetRange[i].y));
            positionsInTargetRange.Add(new Vector2Int(nodesInTargetRange[i].x, nodesInTargetRange[i].y));
        }

        List<Unit> threateningEnemioe = new List<Unit>();
        List<Unit> enemyUnitsInRange =  new List<Unit>();
        for (int i = 0; i < actionData.enemyUnits.Count; i++)
        {
            Unit enemyUnit = gameManager.grid.GetGridObject(actionData.enemyUnits[i]).unit;
            enemyUnitsInRange.Add(enemyUnit);
            if (!actionData.markedUnits.Contains(actionData.enemyUnits[i]))
            {
                threateningEnemioe.Add(enemyUnit);
            }
        }

        // Generate Threat Grid
        int[,] threatGrid = CreateThreatGrid(actionData, threateningEnemioe);

        // get moving full move grid (not limited to just to actions out)
        map.ResetMap(true);
        actingUnit.moveModifier.SetUnwalkable(gameManager, actingUnit);
        map.SetGoalsNew(new List<Vector2Int>() { new Vector2Int(actingUnit.x, actingUnit.y) }, gameManager, actingUnit.moveModifier, debug: false);
        int[,] actingUnitMoveGrid =  map.GetGridValues();
        for(int i = 0; i < gameManager.mapSize; i++)
        {
            for(int j = 0; j < gameManager.mapSize; j++)
            {
                actingUnitMoveGrid[i, j] = (actingUnitMoveGrid[i, j] +  17) / actingUnit.moveSpeedPerMoveAction;
            }
        }

        //Get nodes that are in range of enemy units
        map.ResetMap(true, false);
        map.SetGoalForNodesInTargetRange(actionData.enemyUnits, maxRange);
        int[,] rangeGrid = map.GetGridValues();
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        calculatedActionData.highestActionValue = actionData.highestActionWeight;
        RangedAttackFiringData firingData = new RangedAttackFiringData();
        firingData.enemyUnitsInRange = enemyUnitsInRange;
        firingData.unitAmmo = unitAmmo;
        firingData.elevationGrid = elevationGrid;
        firingData.threatGrid = threatGrid;
        firingData.actionConsequence = 1f;
        firingData.threatModifer = 0.33f;

        for (int i = 0; i < positionsInTargetRange.Count; i++)
        {
            //Check to see if node can hit a target
            Vector2Int newPosition = positionsInTargetRange[i];
            int rangeValue = rangeGrid[newPosition.x, newPosition.y];
            Debug.Log("test 1: " + positionsInTargetRange[i] + ", " + rangeValue);
            if (rangeValue == -1)
            {
                continue;
            }

            firingData.newPosition = newPosition;
            FindingFiringPosition(actionData, firingData, calculatedActionData);
        }
        return calculatedActionData;
    }

    public CalculatedActionData SkirmishPreset(AIActionData actionData, bool findOptimalPosition)
    {
        CalculatedActionData calculatedActionData = new CalculatedActionData();
        calculatedActionData.completedCalculation = false;
        Unit actingUnit = actionData.unit;
        CombatGameManager gameManager = actingUnit.gameManager;
        DijkstraMap map = gameManager.map;
        List<EquipableAmmoSO> unitAmmo = FiringPositionSetup(actionData);

        if (unitAmmo == null)
        {
            Debug.LogError("this should not happen, no ammo should have been detected in Check if Action is in Range not here");
            return calculatedActionData;
        }

        // Generate Threat Grid
        List<Unit> threateningEnemioe = new List<Unit>();
        List<Unit> enemyUnitsInRange = new List<Unit>();
        for (int i = 0; i < actionData.enemyUnits.Count; i++)
        {
            Unit enemyUnit = gameManager.grid.GetGridObject(actionData.enemyUnits[i]).unit;
            enemyUnitsInRange.Add(enemyUnit);
            if (!actionData.markedUnits.Contains(actionData.enemyUnits[i]))
            {
                threateningEnemioe.Add(enemyUnit);
            }
        }

        int[,] threatGrid = CreateThreatGrid(actionData, threateningEnemioe);

        //Get nodes that are in range of enemy units
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        calculatedActionData.highestActionValue = actionData.highestActionWeight;
        RangedAttackFiringData firingData = new RangedAttackFiringData();
        firingData.enemyUnitsInRange = enemyUnitsInRange;
        firingData.unitAmmo = unitAmmo;
        firingData.elevationGrid = elevationGrid;
        firingData.threatGrid = threatGrid;
        firingData.actionConsequence = 1f;
        firingData.threatModifer = 0.66f;

        if(findOptimalPosition)
        {
            return SkirmishOptimalFiringPosition(actionData, firingData, calculatedActionData);
        }
        else
        {
            return SkirmishFiringPosition(actionData, firingData, calculatedActionData);
        }
    }

    // Unit will Actually Fire
    public CalculatedActionData SkirmishFiringPosition(AIActionData actionData, RangedAttackFiringData firingData, CalculatedActionData calculatedActionData)
    {
        CombatGameManager gameManager = actionData.unit.gameManager;
        Unit actingUnit = actionData.unit;
        DijkstraMap map = gameManager.map;
        map.ResetMap(true, false);
        List<Vector2Int> targetPositions;
        if (actionData.enemyUnits.Count > 0)    
        {
            targetPositions = actionData.enemyUnits;
        }
        else
        {
            targetPositions = actionData.enemyTeamStartingPositions;
        }

        //Get Nodes that acting unit can be in and be in range of aan Enemy Unit
        List<DijkstraMapNode> nodesInTargetRange = map.GetNodesInRangedRange(targetPositions, 0, actionData.enemyUnits, gameManager, actingUnit.moveModifier, maxRange);
        List<Vector2Int> positionsInTargetRange = new List<Vector2Int>();
        for (int i = 0; i < nodesInTargetRange.Count; i++)
        {
            //Debug.Log("Check Nodes in Range: " + new Vector2Int(nodesInTargetRange[i].x, nodesInTargetRange[i].y));
            positionsInTargetRange.Add(new Vector2Int(nodesInTargetRange[i].x, nodesInTargetRange[i].y));
        }


        for (int i = 0; i < positionsInTargetRange.Count; i++)
        {
            //Check to see if node can hit a target
            Vector2Int newPosition = positionsInTargetRange[i];
            Debug.Log("Check: " + positionsInTargetRange[i] + ", " + actionData.movementData[positionsInTargetRange[i].x, positionsInTargetRange[i].y]);

            // Remove positions that the acting unit can't move to and shoot from
            if(actingUnit.currentMajorActionsPoints < actionPointUsage + actionData.movementData[newPosition.x, newPosition.y])
            {
                continue;
            }

            firingData.newPosition = newPosition;
            FindingFiringPosition(actionData, firingData, calculatedActionData);
            Debug.Log("Value of Highest Action Value: " + calculatedActionData.highestActionValue);
        }
        return calculatedActionData;
    }

    public CalculatedActionData SkirmishOptimalFiringPosition(AIActionData actionData, RangedAttackFiringData firingData, CalculatedActionData calculatedActionData)
    {
        Unit actingUnit = actionData.unit;
        CombatGameManager gameManager = actingUnit.gameManager;
        DijkstraMap map = gameManager.map;

        map.ResetMap(true, false);
        List<Vector2Int> targetPositions;
        if (actionData.enemyUnits.Count > 0)
        {
            targetPositions = actionData.enemyUnits;
        }
        else
        {
            targetPositions = actionData.enemyTeamStartingPositions;
        }

        //Get Nodes that acting unit can be in and be in range
        List<DijkstraMapNode> nodesInTargetRange = map.GetNodesInRangedRange(targetPositions, 0, actionData.enemyUnits, gameManager, actingUnit.moveModifier, maxRange);

        List<Vector2Int> positionsInTargetRange = new List<Vector2Int>();
        for (int i = 0; i < nodesInTargetRange.Count; i++)
        {
            //Debug.Log("Check Nodes in Range: " + new Vector2Int(nodesInTargetRange[i].x, nodesInTargetRange[i].y));
            positionsInTargetRange.Add(new Vector2Int(nodesInTargetRange[i].x, nodesInTargetRange[i].y));
        }

        //Get nodes that are in range of enemy units
        map.ResetMap(true, false);
        map.SetGoalForNodesInTargetRange(actionData.enemyUnits, maxRange);


        for (int i = 0; i < positionsInTargetRange.Count; i++)
        {
            //Check to see if node can hit a target
            Vector2Int newPosition = positionsInTargetRange[i];
            Debug.Log("Check: " + positionsInTargetRange[i] + ", " + actionData.movementData[positionsInTargetRange[i].x, positionsInTargetRange[i].y]);

            firingData.newPosition = newPosition;
            FindingFiringPosition(actionData, firingData, calculatedActionData);
            Debug.Log("Value of Highest Action Value: " + calculatedActionData.highestActionValue);
        }
        return calculatedActionData;
    }

    // Look For Best position To Fire From (No Actual Shooting)
    /*
    public CalculatedActionData SkirmishOptimalFiringPosition(AIActionData actionData)
    {
        CalculatedActionData calculatedActionData = new CalculatedActionData();
        calculatedActionData.completedCalculation = false;
        Unit actingUnit = actionData.unit;
        CombatGameManager gameManager = actingUnit.gameManager;
        DijkstraMap map = gameManager.map;
        List<EquipableAmmoSO> unitAmmo = FiringPositionSetup(actionData);

        if (unitAmmo == null)
        {
            Debug.LogError("this should not happen, no ammo should have been detected in Check if Action is in Range not here");
            return calculatedActionData;
        }

        map.ResetMap(true, false);
        List<Vector2Int> targetPositions;
        if (actionData.enemyUnits.Count > 0)
        {
            targetPositions = actionData.enemyUnits;
        }
        else
        {
            targetPositions = actionData.enemyTeamStartingPositions;
        }

        //Get Nodes that acting unit can be in and be in range
        List<DijkstraMapNode> nodesInTargetRange = map.GetNodesInRangedRange(targetPositions, 0, actionData.enemyUnits, gameManager, actingUnit.moveModifier, maxRange);

        List<Vector2Int> positionsInTargetRange = new List<Vector2Int>();
        for (int i = 0; i < nodesInTargetRange.Count; i++)
        {
            //Debug.Log("Check Nodes in Range: " + new Vector2Int(nodesInTargetRange[i].x, nodesInTargetRange[i].y));
            positionsInTargetRange.Add(new Vector2Int(nodesInTargetRange[i].x, nodesInTargetRange[i].y));
        }

        List<Unit> threateningEnemioe = new List<Unit>();
        List<Unit> enemyUnitsInRange = new List<Unit>();
        for (int i = 0; i < actionData.enemyUnits.Count; i++)
        {
            Unit enemyUnit = gameManager.grid.GetGridObject(actionData.enemyUnits[i]).unit;
            enemyUnitsInRange.Add(enemyUnit);
            if (!actionData.markedUnits.Contains(actionData.enemyUnits[i]))
            {
                threateningEnemioe.Add(enemyUnit);
            }
        }

        // Generate Threat Grid
        int[,] threatGrid = CreateThreatGrid(actionData, threateningEnemioe);

        // get moving full move grid (not limited to just to actions out)
        map.ResetMap(true);
        actingUnit.moveModifier.SetUnwalkable(gameManager, actingUnit);
        map.SetGoalsNew(new List<Vector2Int>() { new Vector2Int(actingUnit.x, actingUnit.y) }, gameManager, actingUnit.moveModifier, debug: false);
        int[,] actingUnitMoveGrid = map.GetGridValues();
        for (int i = 0; i < gameManager.mapSize; i++)
        {
            for (int j = 0; j < gameManager.mapSize; j++)
            {
                actingUnitMoveGrid[i, j] = (actingUnitMoveGrid[i, j] + 17) / actingUnit.moveSpeedPerMoveAction;
            }
        }

        //Get nodes that are in range of enemy units
        map.ResetMap(true, false);
        map.SetGoalForNodesInTargetRange(actionData.enemyUnits, maxRange);
        int[,] rangeGrid = map.GetGridValues();
        int[,] elevationGrid = gameManager.spriteManager.elevationOfHexes;
        calculatedActionData.highestActionValue = actionData.highestActionWeight;
        RangedAttackFiringData firingData = new RangedAttackFiringData();
        firingData.enemyUnitsInRange = enemyUnitsInRange;
        firingData.unitAmmo = unitAmmo;
        firingData.elevationGrid = elevationGrid;
        firingData.threatGrid = threatGrid;
        firingData.actionConsequence = 1f;
        firingData.threatModifer = 0.66f;

        for (int i = 0; i < positionsInTargetRange.Count; i++)
        {
            //Check to see if node can hit a target
            Vector2Int newPosition = positionsInTargetRange[i];
            int rangeValue = rangeGrid[newPosition.x, newPosition.y];
            Debug.Log("Check: " + positionsInTargetRange[i] + ", " + actionData.movementData[positionsInTargetRange[i].x, positionsInTargetRange[i].y] + ", " + rangeValue);
            if (rangeValue == -1)
            {
                continue;
            }

            firingData.newPosition = newPosition;
            FindingFiringPosition(actionData, firingData, calculatedActionData);
            Debug.Log("Value of Highest Action Value: " + calculatedActionData.highestActionValue);
        }
        return calculatedActionData;
    }
    */
}

