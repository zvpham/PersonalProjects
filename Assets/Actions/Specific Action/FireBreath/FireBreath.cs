using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/FireBreath")]
public class FireBreath : Action
{
    public override void SelectAction(Unit self)
    {
        base.SelectAction(self);
        self.gameManager.spriteManager.ActivateConeTargeting(self, false, self.currentActionsPoints, 2, 1);
        self.gameManager.spriteManager.coneTargeting.OnFoundTarget += FoundTarget;
    }

    public void FoundTarget(List<Vector2Int> path, List<Vector2Int> coneHexes, Unit movingUnit, bool foundTarget)
    {
        if(path.Count == 0)
        {

        }
        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
    }
}
