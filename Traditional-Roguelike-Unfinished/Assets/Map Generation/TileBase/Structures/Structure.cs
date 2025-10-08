    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TileBase/Structure")]
public class Structure : ScriptableObject
{
    public List<GameObject> wallPrefabs;
    public List<WFCTemplate> WFCTemplates;
    public List<int> validWFCTrainerTemplates;
    public Encounter encounter;
}
