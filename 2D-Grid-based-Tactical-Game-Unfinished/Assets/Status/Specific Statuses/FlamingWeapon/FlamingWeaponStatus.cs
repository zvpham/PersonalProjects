using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/FlamingWeaponStatus")]
public class FlamingWeaponStatus : Status
{
    public Damage AddedFireDamage;

    public override void AddStatus(ActionStatusData actionStatusData)
    {
        AddStatusPreset(actionStatusData.targetUnit, actionStatusData.duration, false);
    }

    public override bool ContinueEvent(Action occuringAction, Passive occuringPassive)
    {
        return false;
    }

    public override void ModifiyAction(Action action, AttackData attackData)
    {
        if(action.actionTypes.Contains(ActionType.Attack))
        {
            if (attackData.allDamage != null)
            {
                attackData.allDamage.Add(AddedFireDamage);
                Modifier flameModifier = new Modifier(1f, "Flame Weapon", attackState.Benificial);
                attackData.modifiers.Add(flameModifier);
            }
        }
        else
        {
            Debug.LogError("tried to Add Damage When ALl Damage wasn't instantiated");
        }
        
    }

    public override void ModifyAttackData(AttackData attackData)
    {
        Modifier flameModifier = new Modifier(1f, "Flame Weapon", attackState.Benificial);
        attackData.modifiers.Add(flameModifier);
    }

    public override void RemoveStatus(Unit target)
    {
        RemoveStatusFinal(target);
    }
}
