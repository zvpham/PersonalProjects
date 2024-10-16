using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/PrefabUnitTerrain")]
public class PrefabUnitTerrain : ScriptableObject
{
    public List<int> unitTeam;
    public List<Vector2Int> unitPositions;
    public List<battleLineData> units;
}
