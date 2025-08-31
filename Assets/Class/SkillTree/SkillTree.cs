using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Class/SkillTree")]
public class SkillTree : ScriptableObject
{
    public Image SkillTreeImage;
    public string SkillTreeNameKey;
    public List<SkillSO> skills;
}
