    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(menuName = "Status/Jumping")]
public class Jumping : MovementStatus
{
    override public void ApplyEffect(Unit target)
    {
        if (this.isFirstTurn)
        {
            if (target.forcedMovementPathData.forcedMovementSpeed == 0)
            {
                target.forcedMovementPathData.forcedMovementSpeed = target.forcedMovementPathData.forcedMovementPath.Count / 2f;
            }

            AddStatusPreset(target);
            AddUnusableStatuses(target);
            target.gameManager.ChangeUnits(target.gameObject.transform.position, null);
            target.hasLocationChangeStatus += 1;
            target.gameManager.isLocationChangeStatus += 1;
        }
        else
        {
            target.gameManager.ChangeUnits(target.gameObject.transform.position, null, true);
        }

        for (float i = 0; i < target.forcedMovementPathData.forcedMovementSpeed + target.forcedMovementPathData.excessForcedMovementSpeed;)
        {
            if (i > target.forcedMovementPathData.forcedMovementSpeed)
            {
                target.forcedMovementPathData.excessForcedMovementSpeed -= target.forcedMovementPathData.previousForcedMovementIterrationRate;
            }

            try
            {
                target.gameObject.transform.position = target.forcedMovementPathData.
                    forcedMovementPath[target.forcedMovementPathData.forcedPathIndex];
            }
            catch
            {
                break;
            }

            target.CheckForStatusFields(target.gameObject.transform.position);

            target.forcedMovementPathData.forcedPathIndex += 1;
            i += 1 * target.timeFlow;
            target.forcedMovementPathData.previousForcedMovementIterrationRate = 1 * target.timeFlow;
            this.Ivalue = i;
        }
        target.forcedMovementPathData.excessForcedMovementSpeed += this.Ivalue - target.forcedMovementPathData.forcedMovementSpeed;
        target.forcedMovementPathData.currentPathIndex = target.forcedMovementPathData.forcedPathIndex - 1;

        if ((Vector2)target.gameObject.transform.position == target.forcedMovementPathData.forcedMovementPath[target.forcedMovementPathData.forcedMovementPath.Count - 1])
        {
            RemoveEffect(target);
            return;
        }
        target.gameManager.ChangeUnits(target.gameObject.transform.position, target, true);

        for (int i = 0; i < target.statuses.Count; i++)
        {
            if (target.statuses[i].statusName.Equals(this.statusName))
            {
                if (target.statusDuration[i] < this.statusDuration)
                {
                    target.statusDuration[i] = this.statusDuration;
                }
                return;
            }
        }
    }

    public override void ChangeQuicknessNonstandard(float value)
    {

    }

    public override void onLoadApply(Unit target)
    {
        AddUnusableStatuses(target);
        AddStatusOnLoadPreset(target);
        target.gameManager.ChangeUnits(target.gameObject.transform.position, target, true);
        target.hasLocationChangeStatus += 1;
        target.gameManager.isLocationChangeStatus += 1;
    }

    // Update is called once per frame  
    override public void RemoveEffect(Unit target)
    {
        Unit unit = target.gameManager.grid.GetGridObject((int)target.gameObject.transform.position.x, (int)target.gameObject.transform.position.y);
        if (unit != null)
        {
            MeleeAttack.Attack(unit, target.toHitBonus, target.armorPenetration, target.strengthMod + 3);
            ForcedMovement.MoveUnit(unit);
        }
        target.gameManager.ChangeUnits(target.gameObject.transform.position, target);

        RemoveStatusPreset(target);
        target.gameManager.isLocationChangeStatus -= 1;
        target.hasLocationChangeStatus -= 1;

        if ((Vector2)target.gameObject.transform.position == target.forcedMovementPathData.forcedMovementPath
            [target.forcedMovementPathData.forcedMovementPath.Count - 1])
        {
            target.forcedMovementPathData.forcedMovementPath = new List<Vector2>();
            target.forcedMovementPathData.forcedMovementSpeed = 0;
            target.forcedMovementPathData.excessForcedMovementSpeed = 0;
            target.forcedMovementPathData.previousForcedMovementIterrationRate = 0;
            target.forcedMovementPathData.forcedPathIndex = 0;
            target.forcedMovementPathData.currentPathIndex = 0;
        }

        int expectedLocationIndex = target.gameManager.unitWhoHaveLocationChangeStatus.IndexOf(target);
        if (expectedLocationIndex != -1)
        {
            target.gameManager.unitWhoHaveLocationChangeStatus.RemoveAt(expectedLocationIndex);
            target.gameManager.expectedLocationChangeList.RemoveAt(expectedLocationIndex);

        }
        foreach (ActionTypes actionType in actionTypesNotPermitted)
        { 
            target.unusableActionTypes[actionType] = target.unusableActionTypes[actionType] - 1;
            if(target.unusableActionTypes[actionType] <= 0) { }
            target.unusableActionTypes.Remove(actionType);
        }
    }
}
