using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveGridObject
{
    public GridHex<PassiveGridObject> passiveGrid;
    public int x;
    public int y;
    public List<PassiveObjects> passiveObjects;    

    public PassiveGridObject(GridHex<PassiveGridObject> grid, int x, int y)
    {
        this.passiveGrid = grid;
        this.x = x;
        this.y = y;
        passiveObjects = new List<PassiveObjects>();
    }

    public override string ToString()
    {
        string passiveDebug = "";
        for(int i = 0; i < passiveObjects.Count; i++)
        {
            passiveDebug += passiveObjects[i].ToString() + "\n";
        }
        return passiveDebug;
    }
}
