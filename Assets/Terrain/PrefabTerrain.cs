using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/PrefabTerrain")]
public class PrefabTerrain : ScriptableObject
{
    public List<Vector3Int> terrainElevation;
}
