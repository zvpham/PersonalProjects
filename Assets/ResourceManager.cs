using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ResourceManager")]
public class ResourceManager : ScriptableObject
{
    public List<Sprite> hexBaseSprites;
    public List<Tile> BaseTile;
}
