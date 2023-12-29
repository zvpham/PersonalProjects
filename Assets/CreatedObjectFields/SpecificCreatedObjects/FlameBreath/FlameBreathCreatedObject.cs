using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlameBreathCreatedObject : CreatedObjectDamage
{
    Unit unit;
    Unit flyingUnit;

    public FlameBreathCreatedObject(Grid<CreatedObject> grid, int x, int y, GameObject createdObjectSpritePrefab, List<Vector3> validLocations)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.isActive = false;
        if (validLocations.Contains(grid.GetWorldPosition(this.x, this.y)))
        {
            this.createdObjectSprite = GameObject.Instantiate(createdObjectSpritePrefab, grid.GetWorldPosition(this.x, this.y), new Quaternion(0, 0, 0, 1f));
            this.isActive = true;
        }
    }

    public static void ApplyObject(float applyPercentage, GameManager gameManager, Vector3 Location, FullDamage damageCalculation)
    {
        Unit unit = gameManager.grid.GetGridObject(Location);
        Unit flyingUnit = gameManager.flyingGrid.GetGridObject(Location);
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
        GameObject.Destroy(createdObjectSprite);
    }

    public override string ToString()
    {
        return (isActive).ToString();
    }
}
