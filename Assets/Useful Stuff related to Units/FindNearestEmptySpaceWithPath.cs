using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FindNearestEmptySpaceWithPath
{ 
    public static List<Vector3> openList;
    public static List<Vector3> closedList;

    public static bool isPositionFound = false;
    public static int[,] grid;
    public static int maxRange;
    public static int currentRange;
    public static Unit nextUnit;
    public static Unit storedUnit;
    public static int x;
    public static int y;
    public static int debugTries;
    public static int maxDebugTries;

    public static string debugWord;

    public static NodeState nodeState;

    public static GameManager gameManager;

    public static List<Vector3> temp;

    public enum NodeState
    {
        wall,
        unit,
        emptySpace
    }
    public static List<Vector3> FindEmptySpace(Unit targetUnit, Unit originUnit, bool CareAboutWalls)
    {
        debugWord = "";
        openList = new List<Vector3>();
        closedList = new List<Vector3>();
        maxDebugTries = 100;

        gameManager = targetUnit.gameManager;
        return FindPath(targetUnit, originUnit, !CareAboutWalls);
    }

    private static List<Vector3> FindPath(Unit targetUnit, Unit originUnit, bool ignoreWalls)
    {
        debugTries = 0;
        openList.Add(targetUnit.gameObject.transform.position);
        while (openList.Count > 0)
        {
            debugTries += 1;
            if (debugTries > maxDebugTries)
            {
                break;
            }
            Vector3 currentNode = GetHighestPriorityNode(openList);
            openList.Remove(currentNode);
            closedList.Add(currentNode);
            foreach (Vector3 neighborNode in GetNeighborList(currentNode))
            {
                nodeState = NodeState.wall;
                if (!ignoreWalls)
                {
                    nodeState = IsClearToMoveToPositionRadial(neighborNode);
                }
                else
                {
                    nodeState = IsClearToMoveToPositionIgnoreWalls(neighborNode);
                }

                if (nodeState == NodeState.unit)
                {
                    openList.Add(neighborNode);
                }
                else if (nodeState == NodeState.emptySpace)
                {
                    temp = isPath(new Vector3(neighborNode.x, neighborNode.y, 0), originUnit);
                    if (temp != null)
                    {
                        return temp;
                    }
                }
            }
        }
        return null;
    }

    private static List<Vector3> isPath(Vector3 endNode, Unit originUnit)
    {
        AStarPathfinding path = originUnit.gameManager.mainGameManger.path;
        List<AStarPathNode> movementPath = path.FindPath(originUnit.gameObject.transform.position, endNode, true);
        if(movementPath != null )
        {
            List<Vector3> movement = new List<Vector3>();
            foreach(AStarPathNode node in movementPath)
            {
                movement.Add(node.grid.GetWorldPosition(node.x, node.y));
            }
            return movement;
        }
        else
        {
            return null;
        }

    }

    private static Vector3 GetHighestPriorityNode(List<Vector3> pathNodeList)
    {
        Vector3 highestPriorityNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].z < highestPriorityNode.z)
            {
                highestPriorityNode = pathNodeList[i];
            }
        }
        return highestPriorityNode;
    }

    private static List<Vector3> GetNeighborList(Vector3 currentNode)
    {
        List<Vector3> neighborList = new List<Vector3>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (!(i == 0 && j == 0))
                {
                    Vector3 newNode = new Vector3(currentNode.x + j, currentNode.y + i, currentNode.z + 1);

                    if (!closedList.Contains(newNode) || !openList.Contains(newNode))
                    {
                        neighborList.Add(newNode);
                    }
                }
            }
        }

        return neighborList;
    }

    public static NodeState IsClearToMoveToPositionRadial(Vector3 position)
    {
        Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(position);
        if (!gameManager.groundTilemap.HasTile(gridPosition) || (gameManager.obstacleGrid.GetGridObject(position) != null && gameManager.obstacleGrid.GetGridObject(position).blockMovement == true))
        {
            return NodeState.wall;
        }

        Unit unit = gameManager.grid.GetGridObject((int)position.x, (int)position.y);
        if (unit != null)
        {
            return NodeState.unit;
        }

        return NodeState.emptySpace;
    }

    public static NodeState IsClearToMoveToPositionIgnoreWalls(Vector3 position)
    {
        Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(position);
        if (!gameManager.groundTilemap.HasTile(gridPosition))
        {
            return NodeState.wall;
        }

        Unit unit = gameManager.grid.GetGridObject((int)position.x, (int)position.y);
        if (unit != null)
        {
            return NodeState.unit;
        }

        return NodeState.emptySpace;
    }
}
