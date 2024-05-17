using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action : ScriptableObject
{
    public Sprite actionSprite;
    public int weight;
    public int coolDown;
    public int maxUses = 1;
    public bool consumableAction = false;
    public List<ActionType> actionTypes;

    public virtual void SelectAction(Unit self)
    {
        if (!CheckActionUsable(self))
        {
            return;
        }
    }

    public abstract void DeselectAction(Unit self);

    public abstract void Activate(Unit self);

    public abstract void PlayerActivate(Unit self);
    // Start is called before the first frame update

    public void UseActionPreset(Unit self)
    {
        if(consumableAction)
        {
            int actionIndex = self.actions.IndexOf(this);
            self.actionUses[actionIndex] -= 1;
        }
    }

    public bool CheckActionUsable(Unit self)
    {
        int actionIndex = self.actions.IndexOf(this);

        if (self.actionCooldowns[actionIndex] != 0 || self.actionUses[actionIndex] <= 0)
        {
            return false;
        }
        return true;
    }
}
