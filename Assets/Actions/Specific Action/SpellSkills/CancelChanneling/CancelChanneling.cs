using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/CancelChanneling")]
public class CancelChanneling : Action
{
    public Status channeling;
    public override void SelectAction(Unit self)
    {
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
        Unit unit = actionData.actingUnit;
        for(int i = 0; i < unit.statuses.Count; i++)
        {
            if (unit.statuses[i].status == channeling)
            {
                unit.statuses[i].status.RemoveStatus(unit);
                break;
            }
        }
        unit.UseActionPoints(0);
    }

    public override void AIUseAction(AIActionData AiActionData, bool finalAction = false)
    {
        throw new System.NotImplementedException();
    }

    public override int CalculateWeight(AIActionData AiActionData)
    {
        throw new System.NotImplementedException();
    }

    public override bool CheckIfActionIsInRange(AIActionData AiActionData)
    {
        throw new System.NotImplementedException();
    }

    public override void FindOptimalPosition(AIActionData AiActionData)
    {
        throw new System.NotImplementedException();
    }
}
