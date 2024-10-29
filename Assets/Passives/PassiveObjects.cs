using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveObjects
{
    public Unit originUnit;
    public Passive passive;


    public override string ToString()
    {
        return passive.passiveIndex.ToString() + ", " + originUnit.ToString();
    }
}
