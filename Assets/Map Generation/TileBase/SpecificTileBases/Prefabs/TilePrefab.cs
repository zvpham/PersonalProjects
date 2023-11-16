using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TilePrefab")]
public class TilePrefab : ScriptableObject
{
    public bool haveMapGenerateWalls;
    public bool haveMapGenerateEncounters;
    public Tuple<int, MapGenerationStates>[,] mapTiles;
    public int[,] units;
}
    