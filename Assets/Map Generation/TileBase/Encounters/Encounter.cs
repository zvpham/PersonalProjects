using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TileBase/Encounter/Encounter")]
public class Encounter : ScriptableObject
{
    public List<UnitComposition> easyUnitEncounters;
    public List<UnitComposition> mediumUnitEncounters;
    public List<UnitComposition> hardUnitEncounters;
    public List<EncounterComposition> easyCompositions;
    public List<EncounterComposition> mediumCompositions;
    public List<EncounterComposition> hardCompositions;
    public float easyDangerChance = .6f;
    public float mediumDangerChance = .25f;
    public float hardDangerChance = .15f;

    public Danger GetDangerRating(float dangerModifier, int seed)
    {
        if ((float)(easyDangerChance + mediumDangerChance + hardDangerChance) != 1f)
        {
            Debug.LogError("Redo Danger Values for:" +  name + " Encounter it is currently at: " + (easyDangerChance + mediumDangerChance + hardDangerChance));
            return Danger.Easy;
        }
        Random.InitState(seed);
        float probability = Random.Range(0f, 1f) + dangerModifier;
        if (probability >= easyDangerChance + mediumDangerChance)
        {
            return Danger.Hard;

        }
        else if (probability >= easyDangerChance)
        {
            return Danger.Medium;
        }
        else
        {
            return Danger.Easy;
        }
    }
}
