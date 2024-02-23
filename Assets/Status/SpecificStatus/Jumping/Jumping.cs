    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(menuName = "Status/Jumping")]
public class Jumping : MovementStatus
{
    override public void ApplyEffect(Unit target, int newDuration)
    {
        AddStatusPreset(target, newDuration);
        MovementStatusPreset(target, forcedMovementPath.Count * 0.5f, forcedMovementPath, false, true);
    }

    public override void ChangeQuicknessNonstandard(float value)
    {

    }

    public override void onLoadApply(Unit target)
    {
        AddStatusOnLoadPreset(target);
    }

    // Update is called once per frame  
    override public void RemoveEffect(Unit target)
    {
        RemoveStatusPreset(target);
    }

    public override bool OnHitUnit(Unit self, Unit target)
    {
        return false;
    }

    public override bool OnHitWall(Unit self, Wall wall)
    {
        if(wall.blockFlying)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void EndForcedMovement(Unit self)
    {
        self.gameManager.ChangeUnits(self.gameObject.transform.position, null, true);
        Unit unit = self.gameManager.grid.GetGridObject((int)self.gameObject.transform.position.x, (int)self.gameObject.transform.position.y);
        if (unit != null)
        {
            MeleeAttack.Attack(unit, self.toHitBonus, self.armorPenetration, self.strengthMod + 3);
            ForceMoveUnits.MoveUnit(unit);
        }
        self.gameManager.ChangeUnits(self.gameObject.transform.position, self);
    }
}
