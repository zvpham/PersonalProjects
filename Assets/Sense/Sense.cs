using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Sense : ScriptableObject
{
    public SenseNames senseName;
    public List<SenseTypes> senseTypes;
    public int range;

    public abstract void DetectNearbyUnits(Unit self);

    public abstract void PeripheralManagerDetectUnits(Unit self);
}
