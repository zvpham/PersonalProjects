using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Class/SkillSO")]
public class SkillSO : ScriptableObject
{
    //Skill Image is in Action or Passive
    //Skill Name Key is in Action or Passive
    public List<string> descriptionKeyNames;
    public Skill skill;
    public List<SkillModifierSO> modifiers;


    public void UnlockSkill(Unit unit)
    {
        skill.AddSkill(unit);
    }

    public void LockSkill(Unit unit)
    {
        skill.RemoveSkill(unit);
    }
}
