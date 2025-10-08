using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Class/Class")]
public class Class : ScriptableObject
{
    public int classIndex;
    public List<ClassLevel> classLevels = new List<ClassLevel>();
    public int currentLevel;
    public string description;
    public ClassName className;
    public ClassName baseClass;
    public ClassRarity classRarity;
    public bool jobClass;
    public bool racialClass;

    public void AddClass(Unit unit)
    {
        unit.classes.Add(this);
        if(classRarity == ClassRarity.Common)
        {
            unit.commonClasses.Add(this);
        }
        else if (classRarity == ClassRarity.Uncommon)
        {
            unit.uncommonClasses.Add(this);
        }
        else if (classRarity == ClassRarity.Rare)
        {
            unit.rareClasses.Add(this);
        }

        for(int i = 0; i < currentLevel; i++)
        {
            classLevels[i].AddClassLevel(unit);
        }
    }

    public void RemoveClass(Unit unit)
    {
        unit.classes.Remove(this);
        if (classRarity == ClassRarity.Common)
        {
            unit.commonClasses.Remove(this);
        }
        else if (classRarity == ClassRarity.Uncommon)
        {
            unit.uncommonClasses.Remove(this);
        }
        else if (classRarity == ClassRarity.Rare)
        {
            unit.rareClasses.Remove(this);
        }

        for (int i = currentLevel; i >= 0; i--)
        {
            classLevels[i].RemoveClasslevel(unit);
        }
    }

    public void LevelUp(Unit unit, bool isPlayer = false)
    {
        currentLevel += 1;
        classLevels[currentLevel - 1].AddClassLevel(unit);
        if(isPlayer)
        {
            Player player = (Player)unit;
            player.UpdatePlayerActions();
        }
    }

    public void LevelDowN(Unit unit)
    {
        classLevels[currentLevel - 1].RemoveClasslevel(unit);
        currentLevel -= 1;
    }


}
