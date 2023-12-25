using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Action/Sprint")]
public class Sprint : SelfStatusAction
{
    public override int CalculateWeight(Unit self)
    {
        return weight;
    }

    public override void Activate(Unit self)
    {
        self.HandlePerformActions(actionType, actionName);
        startStatusPresets(self);
    }

    public override void PlayerActivate(Unit self)
    {
        Activate(self);
    }
}
