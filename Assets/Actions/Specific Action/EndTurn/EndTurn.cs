using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/EndTurn")]
public class EndTurn : Action
{
    public override int CalculateWeight(AIActionData actionData)
    {
        return 1;
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
                ActionData newData =  new ActionData();
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
        Unit self = actionData.actingUnit;
        self.UseActionPoints(self.currentActionsPoints);
    }
}
