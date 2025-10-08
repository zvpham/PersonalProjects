using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Wait")]
public class WaitAction : Action
{
    public override int CalculateWeight(Unit self)
    {
        throw new System.NotImplementedException();
    }

    public override void Activate(Unit self)
    {
        self.HandlePerformActions(actionType,actionName);
        affectedUnit.TurnEnd();
    }

    public override void PlayerActivate(Unit self)
    {
        Activate(self);
    }

    public override void AnimationEnd()
    {

    }
}
