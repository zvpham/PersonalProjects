using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(menuName = "Status/Jumping")]
public class Jumping : Status
{
    //private int jumpProgress = 0;
    public float speed = 0;
    public float baseSpeed;
    public float baseTime = 1;
    public int pathIndex = 0;
    public float excessSpeed = 0;
    public float currentExcessSpeed = 0;
    private float prevIterationRate;
    override public void ApplyEffect(Unit target)
    {
        if(this.isFirstTurn)
        {
            this.speed = (this.path.Count - 1) / 2f;
            this.baseSpeed =  this.path.Count - this.speed;

            AddStatusPreset(target);
            ChangeQuicknessNonstandard(target.timeFlow);
            AddUnusableStatuses(target);

            target.gameManager.grid.SetGridObject(target.self.transform.position, null);
            target.hasLocationChangeStatus += 1;
            target.gameManager.isLocationChangeStatus += 1;
            target.self.transform.position = this.path[this.pathIndex];
            this.pathIndex++;
        }
        else
        {
            target.gameManager.flyingGrid.SetGridObject(target.self.transform.position, null);
        }

        for (float i = 0; i < this.speed + this.currentExcessSpeed;)
        {
            Debug.Log("Num Iterations " + i +  this.speed + this.currentExcessSpeed);
            if(i > this.speed)
            {
                this.currentExcessSpeed -= prevIterationRate;
            }
            try
            {
                target.self.transform.position = this.path[this.pathIndex];
            }
            catch
            {
                break;
            }
                    
            target.CheckForStatusFields(target.self.transform.position);

            this.pathIndex += 1;
            i += 1 * this.baseTime;
            this.prevIterationRate = 1 * this.baseTime;
            this.excessSpeed = this.speed - i;
        }
        Debug.Log("Excess Speed " + this.excessSpeed);
        this.currentExcessSpeed += this.excessSpeed;
        Debug.Log("Current Excess Speed " + this.currentExcessSpeed);
        this.currentProgress = (int) this.speed;
        //this.speed = this.baseSpeed; 


        if (target.self.transform.position == this.path[this.path.Count - 1])
        {
            RemoveEffect(target);
            return;
        }

        Debug.Log("Added Flying Log");
        target.gameManager.flyingGrid.SetGridObject(target.self.transform.position, target);

        for (int i = 0; i < target.statuses.Count; i++)
        {
            if(target.statuses[i].statusName.Equals(this.statusName))
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
         this.baseTime *= value;
    }

    // Update is called once per frame  
    override public void RemoveEffect(Unit target)  
    {
        Unit unit = target.gameManager.grid.GetGridObject((int)target.self.transform.position.x, (int)target.self.transform.position.y);
        if (unit != null)
        {
            Vector3 movementDirection = (this.path[this.path.Count - 1] - this.path[this.path.Count - 2]);
            MeleeAttack.Attack(unit, target.toHitBonus, target.armorPenetration, target.damage);
            ForcedMovement.MoveUnit(unit);
        }
        else
        {
            target.gameManager.grid.SetGridObject(target.self.transform.position, target);
        }

        RemoveStatusPreset(target);
        target.gameManager.isLocationChangeStatus -= 1;
        target.hasLocationChangeStatus -= 1;
        foreach (ActionTypes actionType in actionTypesNotPermitted)
        { 
            target.unusableActionTypes[actionType] = target.unusableActionTypes[actionType] - 1;
            if(target.unusableActionTypes[actionType] <= 0) { }
            target.unusableActionTypes.Remove(actionType);
        }
    }
}
