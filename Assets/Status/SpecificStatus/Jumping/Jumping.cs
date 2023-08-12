using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(menuName = "Status/Jumping")]
public class Jumping : Status
{
    //private int jumpProgress = 0;
    public int speed = 0;
    override public void ApplyEffect(Unit target)
    {
        if(this.isFirstTurn)
        {
            this.currentProgress = 0;
            if(this.path.Count != 2)
            {
                this.speed = (this.path.Count / 2) + 1;
            }
            else
            {
                this.speed = this.path.Count / 2;
            }
            this.targetUnit = target;
            target.statuses.Add(this);
            this.isFirstTurn = false;
            target.statusDuration.Add(this.statusDuration);
            target.gameManager.statusDuration.Add(this.statusDuration);
            target.gameManager.isLocationChangeStatus += 1;
            target.gameManager.statusPriority.Add(target.gameManager.baseTurnTime);
            target.hasLocationChangeStatus += 1;
            target.gameManager.allStatuses.Add(this);
            target.gameManager.grid.SetGridObject(target.self.transform.position, null);
            AddUnusableStatuses(target);
        }
        else
        {
            target.gameManager.flyingGrid.SetGridObject(target.self.transform.position, null);
        }

        for (int i = 0; i < this.speed; i++)
        {
            target.self.transform.position = this.path[i + this.currentProgress];
        }
        this.currentProgress = this.speed;
        this.speed = this.path.Count - this.speed;


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

    // Update is called once per frame  
    override public void RemoveEffect(Unit target)  
    {
        bool isPositionFound = false;
        Unit unit = target.gameManager.grid.GetGridObject((int)target.self.transform.position.x, (int)target.self.transform.position.y);
        if (unit != null)
        {
            Vector3 movementDirection = (this.path[this.path.Count - 1] - this.path[this.path.Count - 2]);
            MeleeAttack.Attack(unit, target.toHitBonus, target.armorPenetration, target.damage);
            //ForcedMovement.FirstCallIsClearToMoveToPosition(movementDirection, unit, false);
            ForcedMovement.MoveUnit(unit);
            /*
            if (!IsClearToMoveToPosition(movementDirection, unit, false))
            {
                for (float i = -1; i <= 1; i++)
                {
                    for (float j = -1; j <= 1; j++)
                    {
                        Debug.Log("What Is Going ON");
                        Vector3 newDirection = new Vector3(j, i, 0f);
                        if (movementDirection == newDirection || movementDirection == -newDirection)
                        {
                            continue;
                        }

                        if (IsClearToMoveToPosition(newDirection, unit, false))
                        {
                            isPositionFound = true;
                            break;
                        }
                    }
                    if (isPositionFound)
                    {
                        break;
                    }
                }
            }
            */
        }
        else
        {
            target.gameManager.grid.SetGridObject(target.self.transform.position, target);
        }

        int index = target.statuses.IndexOf(this);
        target.statuses.RemoveAt(index);
        target.statusDuration.RemoveAt(index);
        target.gameManager.isLocationChangeStatus -= 1;
        target.hasLocationChangeStatus -= 1;
        int statusindex = target.gameManager.allStatuses.IndexOf(this);
        target.gameManager.statusPriority.RemoveAt(statusindex);
        target.gameManager.allStatuses.RemoveAt(statusindex);
        target.gameManager.statusDuration.RemoveAt(statusindex);
        foreach (ActionTypes actionType in actionTypesNotPermitted)
        { 
            target.unusableActionTypes[actionType] = target.unusableActionTypes[actionType] - 1;
            if(target.unusableActionTypes[actionType] <= 0) { }
            target.unusableActionTypes.Remove(actionType);
        }
    }

    public bool IsClearToMoveToPosition(Vector3 direction, Unit movingUnit, bool isDisplaceOriginalMovingUnit)
    {
        Vector3 position = movingUnit.self.transform.position + direction;
        Unit unit = movingUnit.gameManager.grid.GetGridObject((int)position.x, (int)position.y);
        if (unit != null)
        {
            if(IsClearToMoveToPosition(direction, unit, false))
            {
                if(!isDisplaceOriginalMovingUnit)
                {
                    movingUnit.gameManager.grid.SetGridObject(movingUnit.self.transform.position, null);
                }
                movingUnit.self.transform.position = position;
                movingUnit.gameManager.grid.SetGridObject(movingUnit.self.transform.position, movingUnit);
                return true;
            }
            else
            {
                for (float i = -1; i <= 1; i++)
                {
                    for (float j = -1; j <= 1; j++)
                    {
                        Vector3 newDirection = new Vector3(j, i, 0f);
                        if (direction == newDirection || direction == -newDirection)
                        {
                            continue;
                        }

                        if(IsClearToMoveToPosition(newDirection, unit, false))
                        {
                            movingUnit.gameManager.grid.SetGridObject(movingUnit.self.transform.position, null);
                            movingUnit.self.transform.position = position;
                            movingUnit.gameManager.grid.SetGridObject(movingUnit.self.transform.position, movingUnit);
                            return true;
                        }
                    }
                }
            }
        }

        Vector3Int gridPosition = movingUnit.gameManager.groundTilemap.WorldToCell(position);
        if (!movingUnit.gameManager.groundTilemap.HasTile(gridPosition) || movingUnit.gameManager.collisionTilemap.HasTile(gridPosition))
        {
            return false;
        }
        movingUnit.gameManager.grid.SetGridObject(movingUnit.self.transform.position, null);
        movingUnit.self.transform.position = position;
        movingUnit.gameManager.grid.SetGridObject(movingUnit.self.transform.position, movingUnit);

        return true;
    }
}
