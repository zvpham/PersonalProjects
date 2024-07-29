using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPosition
{
    public GridHex<GridPosition> hexGrid;
    public int x;
    public int y;
    public Unit unit;
    public Unit tempUnit;

    public GridPosition(GridHex<GridPosition> grid, int x, int y)
    {
        this.hexGrid = grid;
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        /*
        if(unit == null)
        {
            return " ";
        }
        return unit.ToString();
        */
        return x.ToString() + ", " + y.ToString();
    }
}
