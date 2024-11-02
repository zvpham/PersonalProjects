using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stops attacks of oppurtunity from activating
public class Evasive : Status
{ 
    public override void AddStatus(Unit target, int newDuration)
    {
        StatusData newStatus =  new StatusData();
        newStatus.status = this;
        newStatus.duration = newDuration;
        target.statuses.Add(newStatus);
    }

    public override bool ContinueEvent(Action occuringAction, Passive occuringPassive)
    {
        if(occuringPassive.GetType() == typeof(OpportunityAttack))
        {
            return false;
        }
        return true;
    }

    public override void RemoveStatus(Unit target)
    {
        throw new System.NotImplementedException();
    }


}

