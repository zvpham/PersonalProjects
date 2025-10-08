using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Faction/EnemyCompositionw")]
public class EnemyComposition : ScriptableObject
{
    public List<EnemyCompositionUnitType> unitComposition;
    public List<int> unitLevels;

    private int maxLevel = 7;
    public int seed;
    public List<UnitSuperClass> GetComposition(Faction faction)
    {
        if(unitComposition == null || unitLevels == null || unitComposition.Count != unitLevels.Count)
        {
            Debug.LogError("Get Composition has failed for: " + name);
            return null;
        }

        for (int i = 0; i < unitLevels.Count; i++)
        {
            if (unitLevels[i] < 0 || unitLevels[i] > maxLevel)
            {
                Debug.Log("Tried to access a unit level that doesn't exist: " + unitLevels[i]);
                return null;
            }
        }

        seed = System.DateTime.Now.Millisecond;
        List<UnitSuperClass> unitCompositions = new List<UnitSuperClass>();
        for(int i = 0; i < unitComposition.Count; i++)
        {
            Random.InitState(UseSeed());
            int unitChoiceIndex;
            switch (unitComposition[i])
            {
                case (EnemyCompositionUnitType.Melee):
                    unitChoiceIndex = Random.Range(0, faction.dangerLevelList[unitLevels[i]].meleeHeroes.Count);
                    unitCompositions.Add(faction.dangerLevelList[unitLevels[i]].meleeHeroes[unitChoiceIndex]);
                    break;

                case (EnemyCompositionUnitType.Ranged):
                    unitChoiceIndex = Random.Range(0, faction.dangerLevelList[unitLevels[i]].rangedHeroes.Count);
                    unitCompositions.Add(faction.dangerLevelList[unitLevels[i]].rangedHeroes[unitChoiceIndex]);
                    break;

                case (EnemyCompositionUnitType.Support):
                    unitChoiceIndex = Random.Range(0, faction.dangerLevelList[unitLevels[i]].supportHeroes.Count);
                    unitCompositions.Add(faction.dangerLevelList[unitLevels[i]].supportHeroes[unitChoiceIndex]);
                    break;

                case (EnemyCompositionUnitType.MeleeGroup):
                    unitChoiceIndex = Random.Range(0, faction.dangerLevelList[unitLevels[i]].meleeUnitGroups.Count);
                    unitCompositions.Add(faction.dangerLevelList[unitLevels[i]].meleeUnitGroups[unitChoiceIndex]);
                    break;

                case (EnemyCompositionUnitType.RangedGroup):
                    unitChoiceIndex = Random.Range(0, faction.dangerLevelList[unitLevels[i]].rangedUnitGroups.Count);
                    unitCompositions.Add(faction.dangerLevelList[unitLevels[i]].rangedUnitGroups[unitChoiceIndex]);
                    break;

                case (EnemyCompositionUnitType.SupportGroup):
                    unitChoiceIndex = Random.Range(0, faction.dangerLevelList[unitLevels[i]].supportHeroes.Count);
                    unitCompositions.Add(faction.dangerLevelList[unitLevels[i]].supportHeroes[unitChoiceIndex]);
                    break;
            }
        }

        return unitCompositions;
    }

    public int UseSeed()
    {
        seed += 1;
        return seed;
    }
}

public enum EnemyCompositionUnitType
{
    Melee,
    Ranged,
    Support,
    MeleeGroup,
    RangedGroup,
    SupportGroup
}

