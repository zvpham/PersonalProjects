using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SelfStatusAction : Action
{
    public int duration;
    public Status[] status;
    public bool startStatusPresets(Unit self)
    {
        foreach (Status statuseffect in status)
        {
            Status temp = Instantiate(statuseffect);
            temp.ApplyEffect(self, duration);
            if (isActiveAction && temp.isActiveStatus)
            {
                temp.activeAction = this;
                actionIsActive = true;
            }
        }
        return true;
    }
}
