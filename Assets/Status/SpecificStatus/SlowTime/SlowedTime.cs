using System.Collections;
using System.Collections.Generic;
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
        target.statuses.Add(this);    
        target.statusDuration.Add(statusDuration);
        target.gameManager.statusPriority.Add(target.gameManager.baseTurnTime);
        target.gameManager.allStatuses.Add(this);
        target.gameManager.statusDuration.Add(this.statusDuration);
        this.targetUnit = target;
    }

    public override void ChangeQuicknessNonstandard(float value)
    {

    }

    // Update is called once per frame
    override public void RemoveEffect(Unit target)
    {
        target.ChangeTimeFlow(1 / 2f);
        target.statuses.Remove(this);
        target.statusDuration.Remove(this.statusDuration);
        int statusindex = target.gameManager.allStatuses.IndexOf(this);
        target.gameManager.statusPriority.RemoveAt(statusindex);
        target.gameManager.allStatuses.RemoveAt(statusindex);
        target.gameManager.statusDuration.RemoveAt(statusindex);

        foreach (Status status in target.statuses)
        {
            status.ChangeQuickness(1 / 2f);
        }
    }
}
