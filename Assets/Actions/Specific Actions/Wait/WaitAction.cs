using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Wait")]
public class WaitAction : Action
{
    public override void Activate(Unit self)
    {

    }

    public override void PlayerActivate(Unit self)
    {
        Activate(self);
    }
}
