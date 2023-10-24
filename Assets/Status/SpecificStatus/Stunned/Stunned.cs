using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/Stunned")]
public class Stunned : Status
{
    public override void ApplyEffect(Unit target)
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
        AddStatusPreset(target);
        target.onTurnStart += ApplyStun;
    }

    public override void ChangeQuicknessNonstandard(float value)
    {
        throw new System.NotImplementedException();
    }

    public override void onLoadApply(Unit target)
    {
        AddStatusOnLoadPreset(target);
        target.onTurnStart += ApplyStun;
    }

    public override void RemoveEffect(Unit target)
    {
        target.onTurnStart -= ApplyStun;
        RemoveStatusPreset(target);

    }

    public void ApplyStun()
    {
        targetUnit.TurnEnd();
    }
}
