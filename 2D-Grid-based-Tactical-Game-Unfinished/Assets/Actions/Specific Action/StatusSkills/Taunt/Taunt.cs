using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/StatusAction/Taunt  ")]
public class Taunt : Action
{
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

    public override void ConfirmAction(ActionData actionData)
    {
        throw new System.NotImplementedException();
    }

    public override void FindOptimalPosition(AIActionData AiActionData)
    {
        throw new System.NotImplementedException();
    }
}
