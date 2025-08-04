using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is Used only for Inputing Data hwhen adding a status to a unit
public class ActionStatusData
{
    public Unit targetUnit;
    public int duration;
    public Action action;
    public ActionStatusData(Unit targetUnit, int duration)
    {
        this.targetUnit = targetUnit;
        this.duration = duration;
    }

    public ActionStatusData(Unit targetUnit, int duration, Action spell)
    {
        this.targetUnit = targetUnit;
        this.duration = duration;
        this.action = spell;
    }
}
