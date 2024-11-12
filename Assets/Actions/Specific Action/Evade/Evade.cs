using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Evade")]
public class Evade : StatusAction
{
    public override int CalculateWeight(AIActionData actionData)
    {
        throw new System.NotImplementedException();
    }

    public override void FindOptimalPosition(AIActionData actionData)
    {
        return;
    }

    public override void SelectAction(Unit self)
    {
        base.SelectAction(self);
        self.gameManager.spriteManager.ActivateActionConfirmationMenu(
            () =>
            {
                ActionData newData = new ActionData();
                newData.action = this;
                newData.actingUnit = self;
                self.gameManager.AddActionToQueue(newData, false, false);
                self.gameManager.PlayActions();
            },
            () =>
            {

            });
    }

    public override void ConfirmAction(ActionData actionData)
    {
        Debug.Log("Confirm Action");
        Evade action = (Evade) actionData.action;
        action.status.AddStatus(actionData.actingUnit, 1);
        UseActionPreset(actionData.actingUnit);
    }

}
