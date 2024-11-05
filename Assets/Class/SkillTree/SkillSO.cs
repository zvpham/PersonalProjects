using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Class/SkillSO")]
public class SkillSO : ScriptableObject
{
    public string description;
    public Sprite skillImage;
    public Action action;
    public Passive passive;

    public void UnlockSkill(Unit unit)
    {
        action.AddAction(unit);
        unit.passives.Add(passive);
        passive.AddPassive(unit);
    }

    public void LockSkill(Unit unit)
    {
        action.RemoveAction(unit);
        unit.passives.Remove(passive);
        passive.RemovePassive(unit);
    }
}
