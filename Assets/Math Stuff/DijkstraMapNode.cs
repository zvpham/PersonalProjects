using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DijkstraMapNode
{
    public GridHex<DijkstraMapNode> grid;
    public int x;
    public int y;
    public int value;
    
    public DijkstraMapNode(GridHex<DijkstraMapNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        value = int.MaxValue;
    }

    public void ChangeValue(int newValue)
    {
        value = newValue;
    }

    public override string ToString()
    {
        return value.ToString();
    }
}
