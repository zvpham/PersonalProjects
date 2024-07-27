using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/MeleeAttack")]
public class MeleeAttack : Action
{
    public int minDamage;
    public int maxDamage;
    public int range = 1;
    public float effectAgainstArmorPercentage;
    public override void SelectAction(Unit self)
    {
        base.SelectAction(self);
        int actionIndex = self.actions.IndexOf(this);
        int amountOfActionPointsUsed = this.intialActionPointUsage + actionPointGrowth * self.amountActionUsedDuringRound[actionIndex];
        self.gameManager.spriteManager.ActivateMeleeAttackTargeting(self, false, self.currentActionsPoints, amountOfActionPointsUsed, range,
            CalculateAttackData);
        self.gameManager.spriteManager.meleeTargeting.OnFoundTarget += FoundTarget;
    }

    public void FoundTarget(List<Vector2Int> path, Unit movingUnit, Unit TargetUnit, bool foundTarget)
    {
        if(TargetUnit != null)
        {
            Debug.Log("Hit");
        }
        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
    }

    public List<AttackDataUI> CalculateAttackData(Unit targetUnit, List<Vector2Int> path)
    {
        AttackDataUI mainAttack = new AttackDataUI();
        mainAttack.data = minDamage.ToString() + " - " +  maxDamage.ToString();
        
        List<AttackDataUI> allAttackData =  new List<AttackDataUI>();
        allAttackData.Add(mainAttack);

        return allAttackData;
    }
}
