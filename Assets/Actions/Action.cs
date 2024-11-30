using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action : ScriptableObject
{
    public Sprite actionSprite;
    public int baseActionWeight;
    public int coolDown;
    public int maxUses = 1;
    public int intialActionPointUsage = 1;
    public int actionPointGrowth = 1;
    public bool consumableAction = false;
    public bool oneTimeUsePerRound = false;
    public List<ActionType> actionTypes;

    public CustomAnimations animation;

    public abstract int CalculateWeight(AIActionData actionData);

    public abstract void FindOptimalPosition(AIActionData actionData);

    public abstract bool CheckIfActionIsInRange(AIActionData actionData);

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
        for(int i = 0; i < actionData.movementData.Count; i++)
        {
            int actionIndex = GetActionIndex(actionData.unit);
            if (actionData.movementData[i][tile.x, tile.y] + intialActionPointUsage + 
                (actionPointGrowth * unit.actions[actionIndex].amountUsedDuringRound) <= unit.currentActionsPoints)
            {
                return true;
            }
        }
        return false;
    }

    public void UseActionPreset(Unit self)
    {
        int actionIndex = GetActionIndex(self);
        Debug.Log("Action Uses: " + self.actions[actionIndex].amountUsedDuringRound);
        self.UseActionPoints(intialActionPointUsage + actionPointGrowth * self.actions[actionIndex].amountUsedDuringRound);
        self.actions[actionIndex].amountUsedDuringRound += 1;
        if (consumableAction)
        {
            self.actions[actionIndex].actionUsesLeft -= 1;
        }
    }

    public bool CheckActionUsable(Unit self)
    {
        int actionIndex = GetActionIndex(self);

        if (actionIndex != -1 && self.actions[actionIndex].actionCoolDown == 0 && self.actions[actionIndex].actionUsesLeft > 0 && 
            SpecificCheckActionUsable(self) && this.intialActionPointUsage + actionPointGrowth * self.actions[actionIndex].amountUsedDuringRound
            <= self.currentActionsPoints && (!oneTimeUsePerRound || (oneTimeUsePerRound && self.actions[actionIndex].amountUsedDuringRound == 0)))
        {
            return true;
        }
        return false;
    }


    public abstract void ConfirmAction(ActionData actionData);

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
        actionData.amountUsedDuringRound = 0;
        actionData.actionUsesLeft = maxUses;
        unit.actions.Add(actionData);
    }

    public void AddAction(Unit unit, int actionIndex)
    {
        UnitActionData actionData = new UnitActionData();
        actionData.action = this;
        actionData.active = true;
        actionData.actionCoolDown = 0;
        actionData.amountUsedDuringRound = 0;
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

    // For Movement Actions Only
    public virtual int[,] GetMovementMap(AIActionData actionData)
    {
        Debug.LogError("The Abtract version should never be called");
        return null;
    }
}
