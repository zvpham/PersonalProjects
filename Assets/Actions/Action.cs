using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action : ScriptableObject
{
    public Sprite actionSprite;
    public int weight;
    public int coolDown;
    public int maxUses = 1;
    public int intialActionPointUsage = 1;
    public int actionPointGrowth = 1;
    public bool consumableAction = false;
    public List<ActionType> actionTypes;

    public CustomAnimations animation;
    public TargetingSystem targetingSystem;

    public virtual void SelectAction(Unit self)
    {
        if (!CheckActionUsable(self))
        {
            return;
        }

        //self.OnSelectedAction(this, targetingSystem);
    }

    public void UseActionPreset(Unit self)
    {
        int actionIndex = self.actions.IndexOf(this);
        Debug.Log("Action Uses: " + self.amountActionUsedDuringRound[actionIndex]);
        self.UseActionPoints(intialActionPointUsage + actionPointGrowth * self.amountActionUsedDuringRound[actionIndex]);
        self.amountActionUsedDuringRound[actionIndex] += 1;
        if (consumableAction)
        {
            self.actionUses[actionIndex] -= 1;
        }
    }

    public bool CheckActionUsable(Unit self)
    {
        int actionIndex = self.actions.IndexOf(this);

        if (actionIndex != -1 && self.actionCooldowns[actionIndex] == 0 && self.actionUses[actionIndex] > 0 && SpecificCheckActionUsable(self) &&
            this.intialActionPointUsage + actionPointGrowth * self.amountActionUsedDuringRound[actionIndex] <= self.currentActionsPoints)
        {
            return true;
        }
        return false;
    }

    public virtual bool SpecificCheckActionUsable(Unit self)
    {
        return true;
    }
}
