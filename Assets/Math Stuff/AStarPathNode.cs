using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    public AStarPathNode(Grid<AStarPathNode> grid, int x, int y, Tilemap unWalkableSpaces, Grid<Unit> units)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        Vector3 worldPosition = grid.GetWorldPosition(x, y);
        Vector3Int gridPosition = unWalkableSpaces.WorldToCell(worldPosition);
        if (unWalkableSpaces.HasTile(gridPosition) || units.GetGridObject(worldPosition) != null)
        {
            IsWalkable = false;
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
