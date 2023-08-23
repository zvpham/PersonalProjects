using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowTimeFieldObject : CreatedObjectStatus
{

    public SlowTimeFieldObject(Grid<CreatedObjectStatus> grid, int x, int y, GameObject createdObjectPrefab, Vector3 originPosition, Status[] statuses, int duration, float blastRadiusGet)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        blastRadius = blastRadiusGet;
        if (Vector3.Distance(originPosition + new Vector3(grid.GetWidth() / 2, grid.GetHeight() / 2, 0), originPosition + new Vector3(x, y, 0)) <= blastRadius)
        {
            this.statuses = statuses;
            this.duration = duration;
            spriteObject = Instantiate(createdObjectPrefab, originPosition + new Vector3(x, y, 0), new Quaternion(0, 0, 0, 1f));
        }
    }
 
}
    