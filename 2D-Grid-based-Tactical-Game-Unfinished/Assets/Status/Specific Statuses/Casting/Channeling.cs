using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/Channeling")]
public class Channeling : Status
{
    public Action continueChanneling;
    public Action cancelChanneling;
    
    //Duration is the spellpoints requried
    public override void AddStatus(ActionStatusData actionStatusData)
    {
        Unit target = actionStatusData.targetUnit;
        int spellPointsRequired = actionStatusData.duration;
        SpellStatusData newStatus = new SpellStatusData(this, 20, true, spellPointsRequired, actionStatusData.action);
        newStatus.spellPointsRequired = spellPointsRequired;
        target.statuses.Add(newStatus);
        target.OnTurnStartTurnChange += ChangeTurnActionsToChannel;
        continueChanneling.AddAction(target);
        cancelChanneling.AddAction(target);
    }

    public void ChangeTurnActionsToChannel(Unit target)
    {
        for(int i = 0; i < target.actions.Count; i++)
        {
            if (target.actions[i].action != continueChanneling && target.actions[i].action != cancelChanneling)
            {
                target.actions[i].active = false;
            }
            else
            {
                target.actions[i].active = true;
            }
        }


    }

    public override bool ContinueEvent(Action occuringAction, Passive occuringPassive)
    {
        return false;
    }

    public override void ModifiyAction(Action action, AttackData attackData)
    {
        return;
    }

    public override void ModifyAttackData(AttackData attackData)
    {

    }

    public override void RemoveStatus(Unit target)
    {
        continueChanneling.RemoveAction(target);
        cancelChanneling.RemoveAction(target);
        RemoveStatusFinal(target);
    }
}
