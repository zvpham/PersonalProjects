using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain/TerrainTile")]
public class TerrainTile : ScriptableObject
{
    public Sprite tileSprite;
    public int moveCost;
}
