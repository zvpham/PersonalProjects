using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AStarPathfindingHex
{
    private const int DistanceCost = 10;

    private GridHex<AStarPathNode> grid;
    private List<AStarPathNode> openList;
    private List<AStarPathNode> closedList;

    public AStarPathfindingHex(int width, int height, List<Vector3> walkableSpaces, Vector3 orginPosition)
    {
        List<Vector2> walkableSpacesV2 = new List<Vector2>();
        foreach (Vector3 space in walkableSpaces)
        {
            walkableSpacesV2.Add(new Vector2(space.x, space.y));
        }
        grid = new GridHex<AStarPathNode>(width, height, 1f, orginPosition, (GridHex<AStarPathNode> g, int x, int y) => new AStarPathNode(g, x, y, walkableSpacesV2));
    }

    public AStarPathfindingHex(int width, int height, Vector3 orginPosition)
    {
        grid = new GridHex<AStarPathNode>(width, height, 1f, orginPosition, (GridHex<AStarPathNode> g, int x, int y) => new AStarPathNode(g, x, y, true));
    }
    public GridHex<AStarPathNode> GetGrid()
    {
        return grid;
    }

    public List<AStarPathNode> FindPath(int startX, int startY, int endX, int endY, bool ForceEndNodeToWalkable = false)
    {
        if (grid.GetGridObject(endX, endY) == null)
        {
            Debug.Log("End Position Does Not exist:  " + endX + " " + endY);
            return null;
        }
        if (ForceEndNodeToWalkable)
        {
            grid.GetGridObject(endX, endY).IsWalkable = true;
        }
        // Debug.Log(startPosition + ", " + startX + ", " + startY);
        AStarPathNode startNode = grid.GetGridObject(startX, startY);
        AStarPathNode endNode = grid.GetGridObject(endX, endY);

        openList = new List<AStarPathNode> { startNode };
        closedList = new List<AStarPathNode>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                AStarPathNode pathNode = grid.GetGridObject(x, y);
                pathNode.gCost = int.MaxValue;
                pathNode.CalculateFCost();
                pathNode.cameFromNode = null;
            }
        }

        startNode.gCost = 0;
        startNode.hCost = DistanceCost;
        startNode.CalculateFCost();

        while (openList.Count > 0)
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

                int tentativeGCost = currentNode.gCost + DistanceCost;

                if (tentativeGCost < neighborNode.gCost)
                {
                    neighborNode.cameFromNode = currentNode;
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = DistanceCost;
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
        List<AStarPathNode> neighborList = new List<AStarPathNode>();


        List<Vector2Int> neighborNodes = new List<Vector2Int>();
        bool oddRow = currentNode.x % 2 == 1;
        Vector2Int currentPosition = new Vector2Int(currentNode.x, currentNode.y);
        neighborNodes.Add(currentPosition + new Vector2Int(0, 1));
        neighborNodes.Add(currentPosition + new Vector2Int(0, -1));

        neighborNodes.Add(currentPosition + new Vector2Int(-1, oddRow ? 1 : -1));
        neighborNodes.Add(currentPosition + new Vector2Int(-1, 0));
        neighborNodes.Add(currentPosition + new Vector2Int(1, oddRow ? 1 : -1));
        neighborNodes.Add(currentPosition + new Vector2Int(1, 0));

        for(int i = 0; i < neighborNodes.Count; i++)
        {
            AStarPathNode neighborNode = GetNode(neighborNodes[i].x, neighborNodes[i].y);
            if(neighborNode != null)
            {
                neighborList.Add(neighborNode);
            }
        }
        return neighborList;
    }
    private AStarPathNode GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    private List<AStarPathNode> CalculatePath(AStarPathNode endNode)
    {
        List<AStarPathNode> path = new List<AStarPathNode>() { endNode };
        AStarPathNode currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
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
