using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Class/SkillTree")]
public class SkillTree : ScriptableObject
{
    public string SkillTreeName;
    public SkillBranch branch1;
    public SkillBranch branch2;
}
