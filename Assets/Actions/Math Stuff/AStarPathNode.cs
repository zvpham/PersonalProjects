using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarPathNode
{
    public Grid<AStarPathNode> grid;
    public int x;
    public int y;
    public bool IsWalkable;
    public int gCost;
    public int hCost;
    public int fCost;

    public AStarPathNode cameFromNode;

    public AStarPathNode(Grid<AStarPathNode> grid, int x, int y, List<Vector2> walkableSpaces)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        if(walkableSpaces != null)
        {
            if(walkableSpaces.Contains(new Vector2(x, y)))
            {
                Debug.Log("THIS IS WALKABLE " + new Vector2(x, y));
                IsWalkable = true;
            }
            else
            {
                Debug.Log("THIS IS  NOT WALKABLE" + new Vector2(x, y));
                IsWalkable = false;
            }
        }
        else
        {
            IsWalkable = true;
        }
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public override string ToString()
    {
        return x + "," + y;
    }
}
