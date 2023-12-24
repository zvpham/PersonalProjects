using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/SlowedTime")]
public class SlowedTime : Status
{
    override public void ApplyEffect(Unit target, int newDuration)
    {
        if(AddStatusPreset(target, newDuration))
        {
            return;
        }
        target.ChangeTimeFlow(2f);
    }

    public override void ChangeQuicknessNonstandard(float value)
    {

    }

    public override void onLoadApply(Unit target)
    {
        target.ChangeTimeFlow(2f);
        AddStatusOnLoadPreset(target);
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
