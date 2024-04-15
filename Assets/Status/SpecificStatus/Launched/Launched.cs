using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/Launched")]
public class Launched : MovementStatus
{
    //StatusIntData is launcher's strengthModifier
    public int launcherStrengthAdvantage;
    public FullDamage impactDamage;
    public MeleeAttackAnimation meleeAttackAnimationPrefab;
    public Unit targetUnit;
    public override void ApplyEffect(Unit target, int newDuration)
    {
        AddStatusPreset(target, newDuration);
        MovementStatusPreset(target, forcedMovementPath.Count, forcedMovementPath, true);
    }

    public override void ChangeQuicknessNonstandard(float value)
    {

    }

    public override void EndForcedMovement(Unit self)
    {
        self.gameManager.ChangeUnits(self.gameObject.transform.position, self);
    }

    public override bool OnHitUnit(Unit self, Unit target)
    {
        targetUnit =  target;
        Vector2 nextPostionForUnit =  target.transform.position;
        //Target will stop being Unit being moved because they are too strong
        if (target.strengthMod > statusIntData + launcherStrengthAdvantage)
        {
            self.TakeDamage(target, impactDamage, true);
            return true;
        }

        target.TakeDamage(target, impactDamage, true);

        if(target == null) 
        {
            self.TakeDamage(target, impactDamage, true);
            if(self == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        Launched newStatus = Instantiate(this);
        newStatus.affectedUnit = target;

        newStatus.launcherStrengthAdvantage = launcherStrengthAdvantage;
        ForcedMovement newForcedMovement = Instantiate(this.forcedMovement);
        newStatus.forcedMovement = newForcedMovement;
        newStatus.AddStatusPreset(target, currentStatusDuration);
        newForcedMovement.forcedPathIndex += 1;
        newForcedMovement.Ivalue += 1 * newForcedMovement.unit.timeFlow;
        newForcedMovement.previousForcedMovementIterrationRate = 1 * newForcedMovement.unit.timeFlow;
        
        newForcedMovement.status = newStatus;
        newForcedMovement.unit = target;
        newForcedMovement.forcedMovmentPriority = (int)(newForcedMovement.unit.gameManager.baseTurnTime * newForcedMovement.speed) + newForcedMovement.unit.gameManager.mainGameManger.least;
        newForcedMovement.unit.gameManager.forcedMovements.Add(newForcedMovement);
        newForcedMovement.forcedMovementSpeed += 1; 
        newForcedMovement.unit.gameManager.mainGameManger.forcedMovements.Add(newForcedMovement);
        newForcedMovement.unit.gameManager.expectedLocationChangeList.Add(0);
        newForcedMovement.unit.forcedMovement = newForcedMovement;

        Vector2 direction = newForcedMovement.forcedMovementPath[newForcedMovement.forcedMovementPath.Count - 1] - newForcedMovement.forcedMovementPath[newForcedMovement.forcedMovementPath.Count - 2];


        newForcedMovement.forcedMovementPath.Add(newForcedMovement.forcedMovementPath[newForcedMovement.forcedMovementPath.Count - 1] + direction);

        // IMPORTANT NOTE THE FUNCTIONS ONHITUNIT ONHITWALL AND ENDFORCEDMOVEMENT ARE INSTANCED BASED AND NOT JUST STATIC FUNCTION YOU DUMB DUMB (NEED TO USE A SPECFIC INSTANCED VERSION IF YOU WANT TO USE THAT VERSION)
        newForcedMovement.onHitUnit = newStatus.OnHitUnit;
        newForcedMovement.onHitWall = newStatus.OnHitWall;
        newForcedMovement.endForcedMovement = newStatus.EndForcedMovement;
        newForcedMovement.Activate();
        self.TakeDamage(target, impactDamage, true);


        if (target.transform.position == (Vector3) nextPostionForUnit || self == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void OnHItUnitEndAnimation()
    {

    }

    public override bool OnHitWall(Unit self, Wall wall)
    {
        if(wall.durability <= statusIntData * 3)
        {
            //Damage is for Onhit effects like touching an electric fence
            wall.TakeDamage(self, impactDamage, true);
            wall.Death();
            return false;
        }
        return true;
    }

    public override void onLoadApply(Unit target)
    {
        AddStatusOnLoadPreset(target);
    }

    override public void RemoveEffect(Unit target)
    {
        RemoveStatusPreset(target);
    }
}
