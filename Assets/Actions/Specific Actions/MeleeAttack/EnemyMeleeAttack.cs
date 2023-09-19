using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/EnemyMeleeAttack")]
public class EnemyMeleeAttack : Action
{
    public ActionTypes[] meleeActions = { ActionTypes.attack, ActionTypes.meleeAttack };
    public override void Activate(Unit self)
    {
        Unit unit;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                unit = self.gameManager.grid.GetGridObject((int)self.gameObject.transform.position.x + j, (int)self.gameObject.transform.position.y + i);
                if (unit != null && self.enemyList.Contains(unit))
                {
                    MeleeAttack.Attack(unit, self.toHitBonus, self.armorPenetration, self.strengthMod + 3);
                    self.HandlePerformActions(meleeActions, ActionName.MeleeAttack);
                    self.TurnEnd();
                }
            }
        }
    }

    public override int CalculateWeight(Unit self)
    {
        if(self.inMelee)
        {
            return weight;
        }
        return 0;
    }

    public override void PlayerActivate(Unit self)
    {
        throw new System.NotImplementedException();
    }
}
