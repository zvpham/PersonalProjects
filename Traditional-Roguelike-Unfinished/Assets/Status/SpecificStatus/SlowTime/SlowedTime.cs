using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/SlowedTime")]
public class SlowedTime : Status
{
    public float ChangeTimeFlowValue;
    override public void ApplyEffect(Unit target, int newDuration)
    {
        if(AddStatusPreset(target, newDuration))
        {
            return;
        }

        // Makes sure Slowed Time doesn't stack, but stronger SlowedTimes Will Still Occur
        if(!isFirstApply)
        {
            for (int i = 0; i < target.statuses.Count; i++)
            {
                if (target.statuses[i].statusName.Equals(this.statusName))
                {
                    SlowedTime originalSlowedTime = (SlowedTime)target.statuses[i];
                    if (originalSlowedTime.ChangeTimeFlowValue < ChangeTimeFlowValue)
                    {
                        target.ChangeTimeFlow(1 / ChangeTimeFlowValue);
                        foreach (Status status in target.statuses)
                        {
                            status.ChangeQuickness(1 / ChangeTimeFlowValue);
                        }
                        break;
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        target.ChangeTimeFlow(ChangeTimeFlowValue);
    }

    public override void ChangeQuicknessNonstandard(float value)
    {

    }

    public override void onLoadApply(Unit target)
    {
        target.ChangeTimeFlow(ChangeTimeFlowValue);
        AddStatusOnLoadPreset(target);
    }

    // Update is called once per frame
    override public void RemoveEffect(Unit target)
    {
        target.ChangeTimeFlow(1 / ChangeTimeFlowValue);
        foreach (Status status in target.statuses)
        {
            status.ChangeQuickness(1 / ChangeTimeFlowValue);
        }
        RemoveStatusPreset(target);
    }
}
