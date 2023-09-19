using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameBreathCreatedObject : CreatedObjectDamage
{
    Unit unit;
    public override void ApplyObject(float applyPercentage, GameManager gameManager)
    {
        if (gameManager.grid.GetGridObject(this.transform.position) != null)
        {
            unit = gameManager.grid.GetGridObject(this.transform.position);
        }
        else
        {
            unit = gameManager.flyingGrid.GetGridObject(this.transform.position);
        }

        unit.TakeDamage(damageCalculation, applyPercentage);
    }

    public override CreatedObject CreateObject(Grid<CreatedObject> grid, int x, int y, List<Vector3> validLocations)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        if( validLocations.Contains(grid.GetWorldPosition(this.x, this.y)))
        {
            Instantiate(this, grid.GetWorldPosition(this.x, this.y), new Quaternion(0, 0, 0, 1f));
        }
        return this;
    }
}
