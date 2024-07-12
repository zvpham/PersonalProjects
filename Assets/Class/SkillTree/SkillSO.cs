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
        unit.actions.Add(action);
        unit.passives.Add(passive);
        passive.AddPassive(unit);
    }

    public void LockSkill(Unit unit)
    {
        unit.actions.Remove(action);
        unit.passives.Remove(passive);
        passive.RemovePassive(unit);
    }
}
