using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stops attacks of oppurtunity from activating
[CreateAssetMenu(menuName = "Status/Evasive")]
public class Evasive : Status
{ 
    public override void AddStatus(ActionStatusData actionStatusData)
    {
        AddStatusPreset(actionStatusData.targetUnit, actionStatusData.duration, false);
    }

    public override bool ContinueEvent(Action occuringAction, Passive occuringPassive)
    {
        if(occuringPassive.GetType() == typeof(OpportunityAttack))
        {
            return false;
        }
        return true;
    }

    public override void ModifiyAction(Action action, AttackData attackData)
    {
        return;
    }

    public override void ModifyAttackData(AttackData attackData)
    {
        throw new System.NotImplementedException();
    }

    public override void RemoveStatus(Unit target)
    {
        throw new System.NotImplementedException();
    }
}

