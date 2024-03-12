using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Class/ClassLevel")]
public class ClassLevel : ScriptableObject
{
    public int Strength;
    public int Agility;
    public int Endurance;
    public int Wisdom;
    public int Intelligence;
    public int Charisma;
    public int Luck;
    public List<Action> ActionList;

    public bool IsActionInUnit(Action action, Unit self)
    {
        foreach (Action unitAction in self.actions)
        {
            if (action.actionName.Equals(unitAction.actionName))
            {
                return true;
            }
        }
        return false;
    }

    public void AddClassLevel(Unit self)
    {
        self.ChangeStr(Strength);
        self.ChangeAgi(Agility);
        self.ChangeEnd(Endurance);
        self.ChangeWis(Wisdom);
        self.ChangeInt(Intelligence);
        self.ChangeCha(Charisma);
        self.ChangeLuk(Luck);

        if (ActionList.Count != 0)
        {
            foreach (Action action in ActionList)
            {
                if (IsActionInUnit(action, self))
                {
                    continue;
                }
                else
                {
                    self.actions.Add(Instantiate(action));
                }
            }
        }
    }

    public void RemoveClasslevel(Unit self)
    {
        self.ChangeStr(-Strength);
        self.ChangeAgi(-Agility);
        self.ChangeEnd(-Endurance);
        self.ChangeWis(-Wisdom);
        self.ChangeInt(-Intelligence);
        self.ChangeCha(-Charisma);
        self.ChangeLuk(-Luck);

        if (ActionList.Count != 0)
        {
            foreach (Action action in ActionList)
            {
                if (IsActionInUnit(action, self))
                {
                    continue;
                }
                else
                {
                    self.actions.Add(Instantiate(action));
                }
            }
        }
    }
}
