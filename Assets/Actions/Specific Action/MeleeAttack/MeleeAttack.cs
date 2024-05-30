using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/MeleeAttack")]
public class MeleeAttack : Action
{
    public override void SelectAction(Unit self)
    {
        base.SelectAction(self);
        self.gameManager.spriteManager.ActivateMeleeAttackTargeting(self, false, self.currentActionsPoints, 1);
        self.gameManager.spriteManager.meleeTargeting.OnFoundTarget += FoundTarget;
    }

    public void FoundTarget(List<Vector2Int> path, Unit movingUnit, bool foundTarget)
    {
        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
    }
}
