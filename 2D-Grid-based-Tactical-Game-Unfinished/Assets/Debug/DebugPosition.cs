using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPosition
{
    public GridHex<DebugPosition> debugGrid;
    public int x;
    public int y;
    public int value;

    public DebugPosition(GridHex<DebugPosition> grid, int x, int y, int value)
    {
        this.debugGrid = grid;
        this.value = value;
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return value.ToString();

        // return x.ToString() + ", " + y.ToString() + ", " + unit;
    }
}

