using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/SlowedTime")]
public class SlowedTime : Status
{
    override public void ApplyEffect(Unit target)
    {
        if (!(target.statuses.Count == 0))
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
        target.ChangeTimeFlow(2f);
        AddStatusPreset(target);
    }

    public override void ChangeQuicknessNonstandard(float value)
    {

    }

    // Update is called once per frame
    override public void RemoveEffect(Unit target)
    {
        target.ChangeTimeFlow(1 / 2f);
        foreach (Status status in target.statuses)
        {
            status.ChangeQuickness(1 / 2f);
        }
        RemoveStatusPreset(target);
    }
}
