using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Class/SkillTree")]
public class SkillBranch : ScriptableObject
{
    public Image SkillBranchImage;
    public string SkillBranchNameKey;
    public List<SkillSO> skills;
    public List<SkillSO> startSkills;
    public GameObject skillBranchPrefab;
}
