using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AreaPassive : Passive
{
    public override void AddPassive(Unit unit)
    {
        base.AddPassive(unit);
        PassiveEffectArea newPassiveEffectArea = new PassiveEffectArea();
        newPassiveEffectArea.passive = this;
        newPassiveEffectArea.originUnit = unit;
        newPassiveEffectArea.passiveLocations = new List<Vector2Int>();
        unit.passiveEffects.Add(newPassiveEffectArea);
    }

    public void GetReadyForCombat(Unit unit)
    {
        int passiveAreaIndex = -1;
        for(int i = 0; i < unit.passiveEffects.Count; i++)
        {
            if (unit.passiveEffects[passiveAreaIndex].passive == this)
            {
                passiveAreaIndex = i;
                break;
            }
        }
        unit.gameManager.passiveAreas.Add(unit.passiveEffects[passiveAreaIndex]);
    }
}
