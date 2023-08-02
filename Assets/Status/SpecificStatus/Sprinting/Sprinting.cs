using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/Sprinting", order = 1)]
public class Sprinting : Status
{
    override public void ApplyEffect(Unit target)
    {
        for (int i = 0; i < target.statuses.Count; i++)
        {
            if(target.statuses[i].statusName.Equals(this.statusName))
            {
                if (target.statusDuration[i] < this.statusDuration)
                {
                    target.statusDuration[i] = this.statusDuration;
                }
                return;
            }
        }
        target.statuses.Add(this);
        target.ChangeQuickness(0.5);
        target.statusDuration.Add(statusDuration);
    }

    // Update is called once per frame
    override public void RemoveEffect(Unit target)
    {
        target.ChangeQuickness(1 / 0.5);
        target.statuses.Remove(this);
        target.statusDuration.Remove(0);
    }
}
