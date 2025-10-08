using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHolder : MonoBehaviour
{
    public int elevation;
    public SpriteRenderer sprite;
    public int x, y;
    public List<TerrainHolder> walls =  new List<TerrainHolder>();
}
