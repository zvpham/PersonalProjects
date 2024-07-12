using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Class/SkillBranch")]
public class SkillBranch : ScriptableObject
{
    public string BranchName;
    public List<SkillSO> BranchSkills = new List<SkillSO>() { null, null, null, null };
}
