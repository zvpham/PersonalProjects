using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Passive/PassiveModifier/Boost")]
public class PassiveBoost : PassiveModifer
{
    public BoostType typeOfBoost;

    public override void Lock(Passive passive)
    {
        passive.ChangeBoost(typeOfBoost, false);
    }

    public override void Unlock(Passive passive)
    {
        passive.ChangeBoost(typeOfBoost, true);
    }
}
