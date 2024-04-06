using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SlowTimeFieldObject : CreatedObjectStatus
{

    public SlowTimeFieldObject(Grid<CreatedObject> grid, Unit originunit, int x, int y, GameManager gameManager, GameObject createdObjectPrefab, Vector3 originPosition, Status[] statuses, int duration, float blastRadius)
    {
        this.grid = grid;
        this.originUnit = originunit;
        this.x = x;
        this.y = y;
        Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(originPosition + new Vector3(x, y, 0));
        if (gameManager.groundTilemap.HasTile(gridPosition) && Vector3.Distance(originPosition + new Vector3(grid.GetWidth() / 2, grid.GetHeight() / 2, 0), originPosition + new Vector3(x, y, 0)) <= blastRadius)
        {
            this.timeflow = 2;
            this.statuses = statuses;
            this.duration = duration;
            spriteObject = GameObject.Instantiate(createdObjectPrefab, originPosition + new Vector3(x, y, 0), new Quaternion(0, 0, 0, 1f));
        }
    }

    public override void ApplyObject(float applyPercentage, GameManager gameManager)
    {
        if(statuses != null)
        {
            Vector3 location = grid.GetWorldPosition(x, y);
            Unit ground = gameManager.grid.GetGridObject(location);  
            if (ground != null)
            {
                foreach (Status status in statuses)
                {
                    status.ApplyEffect(ground, 1);
                }

            }

            Unit flying = gameManager.flyingGrid.GetGridObject(location);
            if (flying != null)
            {
                foreach (Status status in statuses)
                {
                    status.ApplyEffect(flying, 1);
                }
            }
        }
    }
}
    