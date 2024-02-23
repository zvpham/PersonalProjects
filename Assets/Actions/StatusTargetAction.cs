using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    public void startMovementStatusPreset(Unit self, List<Vector2> forcedMovementPath)
    {
        foreach (Status statuseffect in status)
        {
            Status temp = Instantiate(statuseffect);
            if (temp.isMovementStatus)
            {
                MovementStatus movementTemp = (MovementStatus)temp;
                movementTemp.forcedMovementPath = forcedMovementPath;
                movementTemp.ApplyEffect(self, duration);
            }
            else
            {
                temp.ApplyEffect(self, duration);
            }
        }
    }
}
