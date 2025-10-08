using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/MapTerrain")]
public class MapTerrain : ScriptableObject
{
    public PrefabTerrain prefabTerrain;
    public TerrainType terrainType;
}
