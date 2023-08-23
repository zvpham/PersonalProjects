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
        target.statuses.Add(this);
        target.ChangeQuickness(0.5);
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
        target.ChangeQuickness(1 / 0.5);
        target.statuses.Remove(this);
        target.statusDuration.Remove(0);
        int statusindex = target.gameManager.allStatuses.IndexOf(this);
        target.gameManager.statusPriority.RemoveAt(statusindex);
        target.gameManager.allStatuses.RemoveAt(statusindex);
        target.gameManager.statusDuration.RemoveAt(statusindex);
    }
}
