using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "TileBase/TilePrefabBase")]
public class TilePrefabBase: TileBase
{
    [SerializeField]
    public bool haveMapGenerateWalls;
    [SerializeField]
    public bool haveMapGenerateEncounters;
    [SerializeField]
    public List<Vector3Int> mapTiles;
    [SerializeField]
    public List<Vector3Int> units;
}
    