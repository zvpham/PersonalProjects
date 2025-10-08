using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DijkstraMapNode
{
    public GridHex<DijkstraMapNode> grid;
    public int x;
    public int y;
    public int value;
    public int amountTraveled;
    public int permissableMoves; // Hex can end on a permissable space/ (Units)
    public int amountOfFreeMoves; // Move a free hex in a place (expand hexes due ot range)
    public bool walkable;
    public bool endPositionOnly;
    
    public DijkstraMapNode(GridHex<DijkstraMapNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.walkable = true;
        this.endPositionOnly = false;
        value = int.MaxValue;
    }

    public void ChangeValue(int newValue)
    {
        if(walkable)
        {
            value = newValue;
        }
    }

    public override string ToString()
    {
        //return "( " + x + ", " + y + "): " + value.ToString();
        return walkable.ToString();
    }
}
