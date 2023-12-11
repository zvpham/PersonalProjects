using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WFCTemplate")]
public class WFCTemplate : ScriptableObject
{
    public int width;
    public int height;
    public bool ifRectangle;
    // Vector < x, y, z>
    // Z = 0 means that set WFC Tile Color to empty
    // Z = 1 means Let WFC TIle Color stay the Same
    public List<Vector3Int> templatePositions;
}
