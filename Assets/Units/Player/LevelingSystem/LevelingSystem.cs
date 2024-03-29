using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveling/LevelingSystem")]
public class LevelingSystem : ScriptableObject
{
    public int currentMainXP;
    public int mainLevel;
    public List<int> mainLevelThresholds;

    public int currentClassXP;
    public int classLevel;
    public List<int> classLevelThresholds;
}
