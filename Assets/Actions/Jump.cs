using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Jump")]
public class Jump : Action
{
    public override void Activate(Unit self)
    {
        if (currentCooldown == 0)
        {
            Debug.Log("Jump");
            currentCooldown = maxCooldown;
            isTurnActivated = true;
        }
    }

    new public void CalculateWeight()
    {

    }
}