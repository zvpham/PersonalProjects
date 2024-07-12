using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Class/UnitClass")]
public class UnitClass : ScriptableObject
{
    public Sprite UIUnitProfile;
    public string className;

    public SkillTree skillTree1;
    public SkillTree skillTree2;
}
