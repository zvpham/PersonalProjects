using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "TileBase/Encounter/EncounterComposition")]
public class EncounterComposition : ScriptableObject
{
    public List<Danger> dangerType;
    public List<int> encounterAmount;
}
