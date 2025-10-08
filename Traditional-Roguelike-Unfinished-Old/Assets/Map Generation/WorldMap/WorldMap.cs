using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WorldMap")]
public class WorldMap : ScriptableObject
{
    public List<Vector3Int> tilesInspectorUse = new List<Vector3Int>();
    public int width;
    public int height;
}
