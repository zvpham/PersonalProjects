using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveEffectArea
{
    public Unit originUnit;
    public Passive passive;
    public List<Vector2Int> passiveLocations = new List<Vector2Int>();

    public override string ToString()
    {
        return passive.passiveIndex.ToString() + ", " + originUnit.ToString();
    }
}
