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

    public abstract int CalculateWeight(AIActionData AiActionData);

    public abstract void FindOptimalPosition(AIActionData AiActionData);

    public abstract bool CheckIfActionIsInRange(AIActionData AiActionData);

    public virtual bool CanMove(AIActionData AiActionData)
    {
        return false;
    }

    // Returns the distance of the endPosition from goal
    public virtual Tuple<int, Vector2Int, List<Action>, List<Vector2Int>> MoveUnit(AIActionData AiActionData, bool IgnorePassives = false)
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
    public virtual int[,] GetMovementMap(AIActionData actionData)
    {
        Debug.LogError("The Abtract version should never be called");
        return null;
    }
}
