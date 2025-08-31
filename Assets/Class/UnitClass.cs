using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Class/UnitClass")]
public class UnitClass : ScriptableObject
{
    public Sprite UIUnitProfile;
    public string classNameKey;

    public SkillTree skillTree1;
    public SkillTree skillTree2;
    public SkillTree skillTree3;
}
