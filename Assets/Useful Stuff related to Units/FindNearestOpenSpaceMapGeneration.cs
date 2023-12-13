using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public static class FindNearestOpenSpaceMapGeneration
{
    public static List<Vector3> openList;
    public static List<Vector3> closedList;

    public static List<Vector2> openListLocation;
    public static List<Vector2> closedListLocation;

    public static int[,] grid;
    public static int x;
    public static int y;
    public static int debugTries;
    public static int maxDebugTries;

    public static string debugWord;
    public static bool showDebug = false;

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
        if (showDebug)
        {
            Debug.Log("Num Units: " + numUnits);
            List<string> debugWords =  new List<string>();
            string debugWordAvailableSpace = "";
            Debug.Log(firstSpawnLocation);
            for(int  i = 0; i < availableSpace.GetLength(1); i++)
            {
                debugWordAvailableSpace = "";
                for(int j = 0; j < availableSpace.GetLength(0); j++)
                {
                    debugWordAvailableSpace += availableSpace[j, i] + " ";
                }
                debugWords.Add(debugWordAvailableSpace);
            }

            for(int i = debugWords.Count - 1; i > -1; i--)
            {
                Debug.Log(debugWords[i]);   
            }
        }

        debugWord = "";
        openList = new List<Vector3>();
        closedList = new List<Vector3>();
        openListLocation = new List<Vector2>();
        closedListLocation = new List<Vector2>();
        maxDebugTries = 100;
        List<Vector2Int> unitSpawnLocations = new List<Vector2Int>();
        grid = availableSpace;
        if(numUnits == 1)
        {
            unitSpawnLocations.Add(new Vector2Int((int)firstSpawnLocation.x, (int)firstSpawnLocation.y));
            return unitSpawnLocations;
        }
        unitSpawnLocations = FindPath(numUnits, firstSpawnLocation, ignoreWalls);
        if (unitSpawnLocations == null )
        {
            return null;
        }
        unitSpawnLocations.Add(new Vector2Int((int)firstSpawnLocation.x, (int)firstSpawnLocation.y));
        return unitSpawnLocations;
    }

    private static List<Vector2Int> FindPath(int numUnits, Vector2 firstSpawnLocation, bool ignoreWalls)
    {
        List<Vector2Int> unitPlacements = new List<Vector2Int>();
        debugTries = 0;
        openList.Add(firstSpawnLocation);
        openListLocation.Add(firstSpawnLocation);
        while (openList.Count > 0)
        {
            debugTries += 1;
            if (debugTries > maxDebugTries)
            {
                Debug.Log("Too Many Tries"); 
                break;
            }
            Vector3 currentNode = GetHighestPriorityNode(openList);
            int nodeIndex = openList.IndexOf(currentNode);
            closedList.Add(currentNode);
            closedListLocation.Add(openListLocation[nodeIndex]);
            openList.RemoveAt(nodeIndex);
            openListLocation.RemoveAt(nodeIndex);

            List<Vector3> neighborNodes = GetNeighborList(currentNode);
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
                        closedListLocation.Add(new Vector2(neighborNode.x, neighborNode.y));
                        break;
                    case NodeState.unit:
                        openList.Add(neighborNode);
                        openListLocation.Add(new Vector2(neighborNode.x, neighborNode.y));
                        break;
                    case NodeState.emptySpace:
                        openList.Add(neighborNode);
                        openListLocation.Add(new Vector2(neighborNode.x, neighborNode.y));
                        unitPlacements.Add(new Vector2Int((int)neighborNode.x, (int)neighborNode.y));
                        grid[(int)neighborNode.x, (int)neighborNode.y] = 3;
                        if (unitPlacements.Count == numUnits - 1)
                        {
                            return unitPlacements;
                        }
                        break;
                    case NodeState.room:
                        Vector3 newNode = neighborNode;
                        openList.Add(newNode);
                        openListLocation.Add(new Vector2(neighborNode.x, neighborNode.y));
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
        if (showDebug)
        {
            debugWord = "";
            for (int i = 0; i < openList.Count; i++)
            {
                debugWord += openList[i] + " ";
            }
            Debug.Log("OpenList: " + debugWord);

            debugWord = "";
            for (int i = 0; i < closedList.Count; i++)
            {
                debugWord += closedList[i] + " ";
            }
            Debug.Log("ClosedList: " + debugWord);
        }

        if(ignoreWalls)
        {
            Debug.LogError("Help Plze Code Should not go here Already Placed this amount of Units: " + unitPlacements.Count);
        } 
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
                    Vector2 newNodeLocation =  new Vector2(currentNode.x + j, currentNode.y + i);
                    Vector3 newNode = new Vector3(currentNode.x + j, currentNode.y + i, currentNode.z + 1);
                    if(newNode.x < 0 || newNode.x >= grid.GetLength(0) || newNode.y < 0 || newNode.y >= grid.GetLength(1))
                    {
                        continue;
                    }
                    if (!closedListLocation.Contains(newNodeLocation) && !openListLocation.Contains(newNodeLocation))
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
