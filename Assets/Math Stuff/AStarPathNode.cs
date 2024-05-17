using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarPathNode
{
    public Grid<AStarPathNode> grid;
    public GridHex<AStarPathNode> hexGrid;
    public int x;
    public int y;
    public bool IsWalkable;
    public bool IsVoid;
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
                IsWalkable = true;
            }
            else
            {
                IsWalkable = false;
            }
        }
        else
        {
            IsWalkable = true;
        }
    }

    public AStarPathNode(Grid<AStarPathNode> grid, int x, int y, bool walkable)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        IsWalkable = walkable;
    }

    public AStarPathNode(GridHex<AStarPathNode> grid, int x, int y, List<Vector2> walkableSpaces)
    {
        this.hexGrid = grid;
        this.x = x;
        this.y = y;
        if (walkableSpaces != null)
        {
            if (walkableSpaces.Contains(new Vector2(x, y)))
            {
                IsWalkable = true;
            }
            else
            {
                IsWalkable = false;
            }
        }
        else
        {
            IsWalkable = true;
        }
    }

    public AStarPathNode(GridHex<AStarPathNode> grid, int x, int y, bool walkable)
    {
        this.hexGrid = grid;
        this.x = x;
        this.y = y;
        IsWalkable = walkable;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(x, y, 0);
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public override string ToString()
    {
        return "A* " + x + "," + y;
    }
}
