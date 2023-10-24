using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "CreatedObject/FlameBreathObject")]
public class FlameBreathCreatedObject : CreatedObjectDamage
{
    Unit unit;
    Unit flyingUnit;

    public override void ApplyObject(float applyPercentage, GameManager gameManager, Vector3 Location)
    {
        unit = gameManager.grid.GetGridObject(Location);
        flyingUnit = gameManager.flyingGrid.GetGridObject(Location);

        if (unit != null)
        {
            unit.TakeDamage(damageCalculation, applyPercentage);
        }
        if (flyingUnit != null)
        {
            flyingUnit.TakeDamage(damageCalculation, applyPercentage);
        }
    }

    public override void ApplyObject(float applyPercentage, GameManager gameManager)
    {
        unit = gameManager.grid.GetGridObject(grid.GetWorldPosition(x,y));
        flyingUnit = gameManager.flyingGrid.GetGridObject(grid.GetWorldPosition(x, y));

        if(unit != null)
        {
            unit.TakeDamage(damageCalculation, applyPercentage);
        }
        if(flyingUnit != null)
        {
            flyingUnit.TakeDamage(damageCalculation, applyPercentage);
        }
    }

    public override void ApplyObject(Unit unit)
    {
        if(!this.isActive)
        {
            return;
        }
        unit.TakeDamage(damageCalculation);
    }

    public override void RemoveObject(GameManager gameManager, bool affectFlying)
    {
        Destroy(createdObjectSprite);
    }

    public override CreatedObject CreateObject(Grid<CreatedObject> grid, int x, int y, List<Vector3> validLocations)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.isActive = false;
        if ( validLocations.Contains(grid.GetWorldPosition(this.x, this.y)))
        {
            this.createdObjectSprite = Instantiate(createdObjectSpritePrefab, grid.GetWorldPosition(this.x, this.y), new Quaternion(0, 0, 0, 1f));
            this.isActive = true;
        }
        return this;
    }

    public override string ToString()
    {
        return (isActive).ToString();
    }
}
