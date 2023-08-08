using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Sprint")]
public class Sprint : Action
{   
    public override void Activate(Unit self)
    {
        startStatusPresets(self);
    }

    public override void PlayerActivate(Unit self)
    {
        Activate(self);
    }

    new public void CalculateWeight()
    {

    }
}
