using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/Sprinting", order = 1)]
public class Sprinting : Status
{
    override public void ApplyEffect(Unit target, int newDuration)
    {
        if(AddStatusPreset(target, newDuration))
        {
            return;
        }
        target.ChangeQuickness(0.5);
        target.PerformedAction += CancelStatusIfActionNotContainMatchingType;
    }

    public override void ChangeQuicknessNonstandard(float value)
    {

    }

    public override void onLoadApply(Unit target)
    {
        AddStatusOnLoadPreset(target);
        target.ChangeQuickness(0.5);
        target.PerformedAction += CancelStatusIfActionNotContainMatchingType;
    }

    // Update is called once per frame
    override public void RemoveEffect(Unit target)
    {
        target.PerformedAction -= CancelStatusIfActionNotContainMatchingType;
        target.ChangeQuickness(1 / 0.5);
        RemoveStatusPreset(target);
    }
}
