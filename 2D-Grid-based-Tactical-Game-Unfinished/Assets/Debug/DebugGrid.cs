using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugGrid 
{
    private GridHex<DebugPosition> grid;
    bool stopGridOnUpdate = false;
    // Start is called before the first frame update
    public DebugGrid(int width, int height, float cellSize, Vector3 orginPosition, bool debug)
    {
        grid = new GridHex<DebugPosition>(width, height, cellSize, orginPosition, (GridHex<DebugPosition> g, int x, int y) => new DebugPosition(g, x, y, 0), debug);
        stopGridOnUpdate = debug;
    }

    public void UpdateGrid(int[,] valueGrid)
    {
        for(int i = 0; i < grid.GetHeight(); i++)
        {
            for(int j = 0; j < grid.GetWidth(); j++)
            {
                DebugPosition currentNode =  grid.GetGridObject(i, j);
                currentNode.value = valueGrid[i, j];
                grid.SetGridObject(i, j, currentNode);
            }
        }

        if(stopGridOnUpdate)
        {
            Debug.LogError("STop Game To Check VAlues");
            Debug.Break();
        }
    }
}
