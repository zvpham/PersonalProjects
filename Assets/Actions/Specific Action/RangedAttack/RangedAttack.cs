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
        CombatGameManager gameManager = actingUnit.gameManager;
        DijkstraMap map = gameManager.map;
        map.ResetMap(true, false);
        actingUnit.moveModifier.SetUnwalkable(gameManager, actingUnit);
        List<DijkstraMapNode> nodesInTargetRange = map.GetNodesInTargetRange(actionData.hexesUnitCanMoveTo, 0, null, null, gameManager, actingUnit.moveModifier, maxRange);
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

        foreach(Unit unit in enemyUnitsInRange)
        {
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
                List<int> moveAmounts = new List<int>();
                for(int i = 0; i < unitsOrganizedByMovement[team][movemodifier].Count; i++)
                {
                    Unit unit = unitsOrganizedByMovement[team][movemodifier][i];
                    unitLocations.Add(new Vector2Int(unit.x, unit.y));
                    moveAmounts.Add(unit.moveSpeedPerMoveAction * 2);
                }
                map.SetUnitThreatRanges(unitLocations, moveAmounts, movemodifier, gameManager);

                tempUnit.moveModifier.SetWalkable(gameManager, tempUnit);
            }
        }
        

        return 0;
    }
        
    public override void FindOptimalPosition(AIActionData actionData)
    {
        return;
    }

    public override bool CheckIfActionIsInRange(AIActionData actionData)
    {
        Unit actingUnit = actionData.unit;
        CombatGameManager gameManager = actingUnit.gameManager;     
        DijkstraMap map = gameManager.map;
        map.ResetMap(false, false);
        List<DijkstraMapNode> nodes =  map.GetNodesInTargetRange(actionData.hexesUnitCanMoveTo, 0, null, null, gameManager, actingUnit.moveModifier, maxRange);
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

    public override void AIUseAction(AIActionData actionData, bool finalAction = false)
    {
        throw new NotImplementedException();
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
                meleeAttackAnimation.SetParameters(movingUnit, movingUnit.transform.position, targetUnit.transform.position);
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

