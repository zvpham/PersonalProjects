using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TileBase/Encounter/UnitComposition")]
public class UnitComposition : ScriptableObject
{
    public int dangerRating;
    public List<GameObject> units;
    public List<NumberRanges> numberOfUnits;
}
