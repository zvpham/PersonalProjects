using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveEffectArea
{
    public Unit originUnit;
    public UnitPassiveData passive; 
    public List<Vector2Int> passiveLocations = new List<Vector2Int>();
    public override string ToString()
    {
        return passive.passive.passiveIndex.ToString() + ", " + originUnit.ToString();
    }

    public Tuple<Passive, Vector2Int> GetTargetingData(Vector2Int orignalPosition, List<Vector2Int> path, List<Vector2Int> setPath, List<Vector2Int> passiveArea)
    {
        return passive.passive.GetTargetingData(orignalPosition, path, setPath, passiveArea);
    }
}
