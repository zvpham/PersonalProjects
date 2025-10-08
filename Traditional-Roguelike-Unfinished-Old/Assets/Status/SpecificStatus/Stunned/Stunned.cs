using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/Stunned")]
public class Stunned : Status
{
    public override void ApplyEffect(Unit target, int newDuration)
    {
        if (AddStatusPreset(target, newDuration))
        {
            return;
        }
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
        affectedUnit.TurnEnd();
    }
}
