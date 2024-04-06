using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelingSystem : MonoBehaviour
{
    public int currentMainXP;
    public int mainLevel;
    public List<MainLevel> mainLevels;

    public int currentClassXP;
    public int classLevel;
    public int maxClassLevel;
    public List<int> classLevelThresholds;

    public void GainXP(Player player, int xp)
    {
        currentMainXP += xp;
        currentClassXP += xp;

        if(currentMainXP >= mainLevels[mainLevel - 1].XPThreshold)
        {
            mainLevels[mainLevel - 1].LevelUp(player);
            currentMainXP -= mainLevels[mainLevel - 1].XPThreshold;
            mainLevel += 1;
        }

        // This prevents player from saving levels beyond their current classLevel Capacity,
        // The 2 is for being able to levelup A new Class and having a small bank, so player doesn't have to imediately level to avoid xp loss
        maxClassLevel = 2;
        for(int i = 0; i < player.commonClasses.Count; i++)
        {
            maxClassLevel += player.commonClasses[i].classLevels.Count;
        }
        for (int i = 0; i < player.uncommonClasses.Count; i++)
        {
            maxClassLevel += player.uncommonClasses[i].classLevels.Count;
        }
        for (int i = 0; i < player.rareClasses.Count; i++)
        {
            maxClassLevel += player.rareClasses[i].classLevels.Count;
        }

        if (currentClassXP >= classLevelThresholds[classLevel - 1])
        {
            if(classLevel + 1 > maxClassLevel)
            {
                currentClassXP = classLevelThresholds[classLevel - 1];
            }
            else
            {
                player.availableClassLevelUps += 1;
                currentClassXP -= classLevelThresholds[classLevel - 1];
                classLevel += 1;
            }
        }
    }
    public void OnLoad(Player player)
    {
        for(int i = 0; i < mainLevel; i++)
        {
            mainLevels[i].LevelUp(player);
        }
    }
}
