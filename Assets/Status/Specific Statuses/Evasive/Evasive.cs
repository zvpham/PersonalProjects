using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stops attacks of oppurtunity from activating
[CreateAssetMenu(menuName = "Status/Evasive")]
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
        Debug.Log("Checck: " + occuringPassive.GetType());
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

