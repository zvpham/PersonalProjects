using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Action : ScriptableObject
{
    public Sprite actionSprite;
    public int baseActionWeight;
    public int actionPriority; // For AI use, current useCase For Deciding what order to try movementActions
    public int coolDown;
    public int maxUses = 1;
    public int actionPointUsage = 1;
    public bool consumableAction = false;
    public bool oneTimeUsePerRound = false;
    public List<ActionType> actionTypes;

    public CustomAnimations animation;

    public class CalculatedActionData
    {
        public int highestActionValue;
        public Vector2Int desiredEndPosition;
        public Vector2Int desiredTargetPositionEnd;
        public Action action;
        public int itemIndex;
        public bool completedCalculation;
    }

    public abstract int CalculateWeight(AIActionData AiActionData);

    // This is to just to get best weight for movement purposes I.E RangedMoveToOptimalPositionCombat
    public virtual int CalculateEnvironmentWeight(AIActionData AiActionData)
    {
        return baseActionWeight;
    }

    public abstract void FindOptimalPosition(AIActionData AiActionData);

    public abstract bool CheckIfActionIsInRange(AIActionData AiActionData);

    public virtual bool CanMove(AIActionData AiActionData)
    {
        return false;
    }

    // Returns the distance of the endPosition from goal
    public virtual Tuple<int, Vector2Int, List<Action>, List<Vector2Int>> MoveUnitToEnemyUnits(AIActionData AiActionData, bool IgnorePassives = false)
    {
        return new Tuple<int, Vector2Int, List<Action>, List<Vector2Int>>(int.MaxValue, Vector2Int.zero, null, new List<Vector2Int>());
    }
    public abstract void AIUseAction(AIActionData AiActionData, bool finalAction = false);

    public abstract void ConfirmAction(ActionData actionData);

    // Largely for movement actions so i can canlculate the expected consequences for a unit taking a specific move action
    //Updated prediction in AIactionData
    public virtual void CalculateActionConsequences(AIActionData AiActionData, Vector2Int desiredEndPosition)
    {
        return;
    }

    // Very Few Actions should have this, you can largley ingore this for estimating unit damage
    //AIActionData is for moving Unit, Acting Unit is the one performing the action.
    //Updated prediction in AIactionData
    public virtual void ExpectedEffectsOfPassivesActivations(AIActionData AiActionData, Unit ActingUnit)
    {

    }

    public virtual void SelectAction(Unit self)
    {
        if (!CheckActionUsable(self))
        {
            return;
        }

        //self.OnSelectedAction(this, targetingSystem);
    }

    public bool CheckIfTileIsInRange(Vector2Int tile, AIActionData actionData)
    {
        Unit unit = actionData.unit;
        int totalActionCost = actionData.movementData[tile.x, tile.y] + GetActionUseCost(unit);
        if (actionData.movementData[tile.x, tile.y] == int.MaxValue ||  totalActionCost > unit.currentMajorActionsPoints)
        {
            return false;
        }

        return true;
    }

    public void UseActionPreset(Unit self)
    {
        int actionIndex = GetActionIndex(self);
        self.UseActionPoints(actionPointUsage);
        if (consumableAction)
        {
            self.actions[actionIndex].actionUsesLeft -= 1;
        }
    }

    public bool CheckActionUsable(Unit self)
    {
        int actionIndex = GetActionIndex(self);

        if (actionIndex != -1 && self.actions[actionIndex].actionCoolDown == 0 && self.actions[actionIndex].actionUsesLeft > 0 && 
            SpecificCheckActionUsable(self) && this.actionPointUsage <= self.currentMajorActionsPoints 
            && (!oneTimeUsePerRound || (oneTimeUsePerRound && !self.actions[actionIndex].actionUsedDuringRound)))
        {
            return true;
        }
        return false;
    }

    public bool CheckIfUnitIsInCover(Vector2Int shootingPosition, Vector2Int targetPosition, CombatGameManager gameManager)
    {
        DijkstraMap map = gameManager.map;
        List<Vector2Int> potentialSpacesThatAreCover = new List<Vector2Int>();
        Vector3 startPosition = map.getGrid().GetWorldPosition(shootingPosition);
        Vector3 endPosition = map.getGrid().GetWorldPosition(targetPosition);
        float angle = (Mathf.Atan2(endPosition.y - startPosition.y, endPosition.x - startPosition.x) * Mathf.Rad2Deg);
        int adjustedAngle = Mathf.RoundToInt(angle);

        List<int> potentialCoverAngles = new List<int>();
        map.getGrid().GetXY(endPosition, out int x, out int y);
        List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(x, y, 1);
        for (int k = 0; k < mapNodes.Count; k++)
        {
            Vector2Int currentNodePosition = new Vector2Int(mapNodes[k].x, mapNodes[k].y);
            GridPosition gridPosition = gameManager.grid.GetGridObject(currentNodePosition);
            if (gameManager.spriteManager.elevationOfHexes[currentNodePosition.x, currentNodePosition.y] > gameManager.spriteManager.elevationOfHexes[targetPosition.x, targetPosition.y] || (gridPosition != null && gridPosition.unit != null))
            {
                Vector3 nodePosition = map.getGrid().GetWorldPosition(currentNodePosition);
                angle = (Mathf.Atan2(endPosition.y - nodePosition.y, endPosition.x - nodePosition.x) * Mathf.Rad2Deg);
                potentialCoverAngles.Add(Mathf.RoundToInt(angle));
                potentialSpacesThatAreCover.Add(currentNodePosition);
            }
        }

        for (int i = 0; i < potentialCoverAngles.Count; i++)
        {
            if (adjustedAngle == potentialCoverAngles[i] || (Mathf.Abs(Mathf.DeltaAngle(adjustedAngle, potentialCoverAngles[i])) < 60))
            {
                return true;
            }
        }
        return false;
    }

    public int[,] CreateThreatGrid(AIActionData actionData, List<Unit> threateningEnemioe)
    {
        CombatGameManager gameManager = actionData.unit.gameManager;
        DijkstraMap map = gameManager.map;

        // Generate Threat Grid
        Dictionary<Team, Dictionary<MoveModifier, List<Unit>>> unitsOrganizedByMovement = new Dictionary<Team, Dictionary<MoveModifier, List<Unit>>>();
        foreach (Unit unit in threateningEnemioe)
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

        map.ResetMap(true);
        foreach (Team team in unitsOrganizedByMovement.Keys)
        {
            foreach (MoveModifier movemodifier in unitsOrganizedByMovement[team].Keys)
            {
                Unit tempUnit = unitsOrganizedByMovement[team][movemodifier][0];
                tempUnit.moveModifier.SetUnwalkable(gameManager, tempUnit);
                List<Vector2Int> unitLocations = new List<Vector2Int>();
                for (int i = 0; i < unitsOrganizedByMovement[team][movemodifier].Count; i++)
                {
                    Unit unit = unitsOrganizedByMovement[team][movemodifier][i];
                    unitLocations.Add(new Vector2Int(unit.x, unit.y));
                }
                map.SetGoalsForThreatGrid(unitLocations, actionData.friendlyUnits, gameManager, movemodifier, debug: false);

                tempUnit.moveModifier.SetWalkable(gameManager, tempUnit);
            }
        }
        int[,] threatGrid = map.GetGridValues();
        return threatGrid;
    }

    public struct RangedAttackFiringData
    {
        public Vector2Int newPosition;
        public List<Unit> enemyUnitsInRange;
        public List<EquipableAmmoSO> unitAmmo;
        public float actionConsequence;
        public int[,] elevationGrid;
        public int[,] threatGrid;
        public float threatModifer;
    }

    public virtual bool SpecificCheckActionUsable(Unit self)
    {
        return true;
    }
    public void AddAction(Unit unit)
    {
        UnitActionData actionData = new UnitActionData();
        actionData.action = this;
        actionData.active = true;
        actionData.actionCoolDown = 0;
        actionData.actionUsedDuringRound = false;
        actionData.actionUsesLeft = maxUses;
        unit.actions.Add(actionData);
    }

    public void AddAction(Unit unit, int actionIndex)
    {
        UnitActionData actionData = new UnitActionData();
        actionData.action = this;
        actionData.active = true;
        actionData.actionCoolDown = 0;
        actionData.actionUsedDuringRound = false;
        actionData.actionUsesLeft = maxUses;
        unit.actions.Insert(actionIndex, actionData);
    }


    public void RemoveAction(Unit unit)
    {
        int unitActionIndex = GetActionIndex(unit);
        unit.actions.RemoveAt(unitActionIndex);
    }

    public int GetActionIndex(Unit unit)
    {
        for(int i = 0; i < unit.actions.Count; i++)
        {
            if (unit.actions[i].action == this)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetActionUseCost(Unit unit)
    {
        int actionIndex = GetActionIndex(unit);
        if(actionIndex == -1) 
        {
            return -1;
        }

        return this.actionPointUsage;
    }

    // For Movement Actions Only
    public virtual void GetMovementMap(AIActionData actionData)
    {
        Debug.LogError("The Abtract version should never be called");
        return;
    }
}
