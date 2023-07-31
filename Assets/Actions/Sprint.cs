using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Sprint")]
public class Sprint : Action
{   
    public override void Activate(Unit self)
    {
        if(currentCooldown == 0)
        {
            Debug.Log("Sprint");
            status.ApplyEffect(self);
            currentCooldown = maxCooldown;
            isTurnActivated = true;
        }
    }

    new public void  CalculateWeight()
    {

    }
}
