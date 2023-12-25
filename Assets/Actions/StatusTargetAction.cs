using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusTargetAction : TargetAction
{
    public int duration;
    public Status[] status;
    public bool startStatusPresets(Unit self)
    {
        foreach (Status statuseffect in status)
        {
            Status temp = Instantiate(statuseffect);
            temp.ApplyEffect(self, duration);
        }
        return true;
    }
}
