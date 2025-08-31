using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Class/SkillSO")]
public class SkillSO : ScriptableObject
{
    public string descriptionKeyName;
    public Sprite skillImage;
    public Action action;
    public Passive passive;


    public void UnlockSkill(Unit unit)
    {
        action.AddAction(unit);
        passive.AddPassive(unit);
        passive.AddPassive(unit);
    }

    public void LockSkill(Unit unit)
    {
        action.RemoveAction(unit);
        passive.RemovePassive(unit);
        passive.RemovePassive(unit);
    }
}
