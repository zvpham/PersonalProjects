using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarPathfinding 
{
    private const int Move_Straight_Cost = 10;
    private const int Move_Diagonal_Cost = 10;

    private Grid<AStarPathNode> grid;
    private List<AStarPathNode> openList;
    private List<AStarPathNode> closedList;

    public AStarPathfinding(int width, int height, List<Vector3> walkableSpaces, Vector3 orginPosition) 
    {
        List<Vector2> walkableSpacesV2 = new List<Vector2>();
        foreach (Vector3 space in walkableSpaces)
        {
            walkableSpacesV2.Add(new Vector2(space.x, space.y));
        }
        grid = new Grid<AStarPathNode> (width, height, 1f, orginPosition, (Grid<AStarPathNode> g, int x, int y ) => new AStarPathNode(g, x, y, walkableSpacesV2));
    }

    public AStarPathfinding(int width, int height, Grid<Wall> walls, Grid<Unit> units, Vector3 orginPosition)
    {
        grid = new Grid<AStarPathNode>(width, height, 1f, orginPosition, (Grid<AStarPathNode> g, int x, int y) => new AStarPathNode(g, x, y, walls, units));
    }

    public AStarPathfinding(int width, int height, int mapWidth, int mapHeight, Grid<Wall>[,] walls, Grid<Unit>[,] units, Vector3 orginPosition)
    {
        grid = new Grid<AStarPathNode>(width, height, 1f, orginPosition, (Grid<AStarPathNode> g, int x, int y) => new AStarPathNode(g, x, y, mapWidth, mapHeight, walls, units));
    }

    public Grid<AStarPathNode> GetGrid() 
    { 
        return grid;
    }

    public List<AStarPathNode> FindPath(Vector3 startPosition, Vector3 endPosition, bool endNodeisWalkable = false)
    {
        int startX, startY, endX, endY;
        grid.GetXY(startPosition, out startX, out startY);
        grid.GetXY(endPosition, out endX, out endY);
        if (grid.GetGridObject(endX, endY) == null)
        {
            Debug.Log("Testing " + endX + " " + endY);
            return null;
        }
        if (endNodeisWalkable)
        {
            grid.GetGridObject(endX, endY).IsWalkable = true;
        }
        Debug.Log(startX + ", " + startY);
        AStarPathNode startNode = grid.GetGridObject(startX, startY);
        AStarPathNode endNode = grid.GetGridObject(endX, endY);

        openList = new List<AStarPathNode> { startNode };
        closedList = new List<AStarPathNode>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for(int y = 0;  y < grid.GetHeight(); y++)
            {
                AStarPathNode pathNode = grid.GetGridObject (x, y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while(openList.Count > 0)
        {
           
            AStarPathNode currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode) 
            {
                return CalculatePath(endNode);
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);
            foreach (AStarPathNode neighborNode in GetNeighborList(currentNode))
            {
                if (neighborNode == null || !neighborNode.IsWalkable || closedList.Contains(neighborNode)) continue;

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighborNode);

                if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.cameFromNode = currentNode;
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = CalculateDistanceCost(neighborNode, endNode);
                    neighborNode.CalculateFCost();

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }
        return null;
    }

    private List<AStarPathNode> GetNeighborList(AStarPathNode currentNode)
    {
        List<AStarPathNode> neighborList  = new List<AStarPathNode>();
        for (int i = -1;  i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (currentNode.x + j >= 0 && currentNode.x <grid.GetWidth())
                {
                    if (currentNode.y + i >= 0 && currentNode.y < grid.GetHeight())
                    {
                        if (i == 0 && j == 0)
                        {

                        }
                        else
                        {
                            neighborList.Add(GetNode(currentNode.x + j, currentNode.y + i));
                        }
                    }
                }
            }
        }

        return neighborList;
    }
    private AStarPathNode GetNode (int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    private List<AStarPathNode> CalculatePath(AStarPathNode endNode)
    {
        List<AStarPathNode> path = new List<AStarPathNode>();
        path.Add(endNode);
        AStarPathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(AStarPathNode a, AStarPathNode b)
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return Move_Diagonal_Cost * Mathf.Min(xDistance, yDistance) + Move_Straight_Cost * remaining;
    }

    private AStarPathNode GetLowestFCostNode(List<AStarPathNode> pathNodeList)
    {
        AStarPathNode lowestFCostNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
            {
                lowestFCostNode = pathNodeList[i];
            }
        }
        return lowestFCostNode;
    }
}
