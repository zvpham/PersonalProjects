using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillNode : SkillTreeNode
{
    public SkillSO skill;

    public override void Unlock(Unit unit)
    {
        base.Unlock(unit);
        skill.skill.AddSkill(unit);
    }

    public override void Lock(Unit unit)
    {
        base.Lock(unit);
        skill.skill.RemoveSkill(unit);
    }
}
