using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Class/UnitClass")]
public class UnitClass : ScriptableObject
{
    public Sprite UIUnitProfile;
    public string classNameKey;

    public SkillBranch skillTree1;
    public SkillBranch skillTree2;
    public SkillBranch skillTree3;
}
