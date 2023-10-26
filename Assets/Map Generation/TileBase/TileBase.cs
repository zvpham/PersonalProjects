using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TileBase")]
public class TileBase : ScriptableObject
{
    public TileType tileType;
    public List<Structure> structures;
    public List<GameObject> setPieces;
    public List<Encounter> encounters;
    public int minDanger;
    public int maxDanger;
}
