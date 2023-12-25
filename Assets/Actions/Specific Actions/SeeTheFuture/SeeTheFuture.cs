using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/SeeTheFuture")]
public class SeeTheFuture : SelfStatusAction
{
    public List<Vector2> pathAI;
    public override void Activate(Unit self)
    {
        self.HandlePerformActions(actionType, actionName);
        foreach (Status statuseffect in status)
        {
            Status temp = Instantiate(statuseffect);
            temp.ApplyEffectEnemy(self, duration);
        }
    }

    public override int CalculateWeight(Unit self)
    {
        int range = duration / 2;
        int index = self.closestEnemyIndex;
        pathAI = LineOfSightAI.MakeLine(self.transform.position, self.enemyList[index].transform.position, range, true);
        if (pathAI != null)
        {
            return weight;
        }
        return 0;
    }

    public override void PlayerActivate(Unit self)
    {
        self.HandlePerformActions(actionType, actionName);
        foreach (Status statuseffect in status)
        {
            Status temp = Instantiate(statuseffect);
            temp.ApplyEffectPlayer(self, duration);
        }
    }
}
