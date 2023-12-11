using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FindNearestOpenSpaceMapGeneration
{
    public static List<Vector3> openList;
    public static List<Vector3> closedList;

    public static bool isPositionFound = false;
    // 1 = Room WFCState, 2 = Open WFCState
    public static int[,] grid;
    public static int maxRange;
    public static int currentRange;
    public static int x;
    public static int y;
    public static int debugTries;
    public static int maxDebugTries;

    public static string debugWord;

    public static NodeState nodeState;

    public enum NodeState
    {
        wall,
        unit,
        emptySpace,
        room
    }
    // Allready Spawned First Unit in a room
    public static List<Vector2Int> FindEmptySpace(int numUnits, Vector2 firstSpawnLocation, int[,]availableSpace, bool ignoreWalls)
    {
        debugWord = "";
        openList = new List<Vector3>();
        closedList = new List<Vector3>();
        maxDebugTries = 100;
        List<Vector2Int> unitSpawnLocations = new List<Vector2Int>();
        grid = availableSpace;
        unitSpawnLocations = FindPath(numUnits, firstSpawnLocation, ignoreWalls);
        unitSpawnLocations.Add(new Vector2Int((int)firstSpawnLocation.x, (int)firstSpawnLocation.y));
        return unitSpawnLocations;
    }

    private static List<Vector2Int> FindPath(int numUnits, Vector2 firstSpawnLocation, bool ignoreWalls)
    {
        List<Vector2Int> unitPlacements = new List<Vector2Int>();
        debugTries = 0;
        openList.Add(firstSpawnLocation);
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
            List<Vector3> neighborNodes = GetNeighborList(currentNode);
            for(int i = 0; i < 2 ; i++)
            {
                foreach (Vector3 neighborNode in neighborNodes)
                {
                    nodeState = NodeState.wall;
                    if (ignoreWalls)
                    {
                        nodeState = IsClearToMoveToPositionIgnoreWalls(neighborNode);
                    }
                    else
                    {
                        nodeState = IsClearToMoveToPositionRadial(neighborNode);
                    }

                    switch (nodeState)
                    {
                        case NodeState.wall:
                            closedList.Add(neighborNode);
                            break;
                        case NodeState.unit:
                            openList.Add(neighborNode);
                            break;
                        case NodeState.emptySpace:
                            openList.Add(neighborNode);
                            unitPlacements.Add(new Vector2Int((int)neighborNode.x, (int)neighborNode.y));
                            grid[(int)neighborNode.x, (int)neighborNode.y] = 3;
                            if (unitPlacements.Count == numUnits - 1)
                            {
                                return unitPlacements;
                            }
                            break;
                        case NodeState.room:
                            Vector3 newNode = neighborNode;
                            newNode.z -= 10;
                            openList.Add(newNode);
                            unitPlacements.Add(new Vector2Int((int) neighborNode.x, (int)neighborNode.y));
                            grid[(int)neighborNode.x, (int)neighborNode.y] = 3;
                            if(unitPlacements.Count == numUnits - 1)
                            {
                                return unitPlacements;
                            }
                            break;
                    }
                }
            }
        }
        Debug.LogError("Help Plze Code Should not go here");
        return null;
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
                    if(newNode.x < 0 || newNode.x >= grid.GetLength(1) || newNode.y < 0 || newNode.y >= grid.GetLength(0))
                    {
                        continue;
                    }
                    if (!closedList.Contains(newNode) && !openList.Contains(newNode))
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
        switch(grid[(int)position.x,(int)position.y])
        {
            case 0:
                return NodeState.wall;
            case 1:
                return NodeState.room;
            case 2:
                return NodeState.emptySpace;
            case 3:
                return NodeState.unit;
        }
        Debug.LogError("Messed Up Try again. fix This. code should not go here ");
        return NodeState.emptySpace;
    }

    public static NodeState IsClearToMoveToPositionIgnoreWalls(Vector3 position)
    {
        switch (grid[(int)position.x, (int)position.y])
        {
            case 0:
                return NodeState.unit;
            case 1:
                return NodeState.room;
            case 2:
                return NodeState.emptySpace;
            case 3:
                return NodeState.unit;
        }
        Debug.LogError("Messed Up Try again. fix This. code should not go here ");
        return NodeState.emptySpace;
    }
}
