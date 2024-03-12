using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Class/Class")]
public class Class : ScriptableObject
{
    public List<ClassLevel> classLevels = new List<ClassLevel>();
    public int currentLevel;
    public string description;
    public ClassName className;
    public ClassName baseClass;
    public ClassRarity classRarity;

}
