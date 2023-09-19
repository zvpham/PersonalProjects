using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/Sprinting", order = 1)]
public class Sprinting : Status
{
    override public void ApplyEffect(Unit target)
    {
        if(!(target.statuses.Count == 0))
        {
            for (int i = 0; i < target.statuses.Count; i++)
            {
                if (target.statuses[i].statusName.Equals(this.statusName))
                {
                    if (target.statusDuration[i] < this.statusDuration)
                    {
                        target.statusDuration[i] = this.statusDuration;
                    }
                    return;
                }
            }
        }
        AddStatusPreset(target);
        target.ChangeQuickness(0.5);
        target.PerformedAction += CancelStatusIfActionNotContainMatchingType;
    }

    public override void ChangeQuicknessNonstandard(float value)
    {

    }

    // Update is called once per frame
    override public void RemoveEffect(Unit target)
    {
        target.PerformedAction -= CancelStatusIfActionNotContainMatchingType;
        target.ChangeQuickness(1 / 0.5);
        RemoveStatusPreset(target);
    }
}
