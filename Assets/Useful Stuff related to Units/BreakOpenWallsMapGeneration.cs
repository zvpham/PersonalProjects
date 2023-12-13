using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public static class BreakOpenWallsMapGeneration
{
    public static List<Vector3> openList;
    public static List<Vector3> closedList;

    public static List<Vector2> openListLocation;
    public static List<Vector2> closedListLocation;
    public static List<Vector2> unknownLocations;

    public static int[,] grid;
    public static int x;
    public static int y;
    public static int debugTries;
    public static int maxDebugTries;

    public static Vector2 structureStartLocation;
    public static int height;
    public static int width;

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
    public static List<Vector2Int> FindEmptySpace(Vector2 initialStructureStartLocation, int structureWidth, int structureHeight,
        int[,] availableSpace)
    {
        if (showDebug)
        {
            List<string> debugWords = new List<string>();
            string debugWordAvailableSpace;
            Debug.Log(structureStartLocation);
            for (int i = 0; i < availableSpace.GetLength(1); i++)
            {
                debugWordAvailableSpace = "";
                for (int j = 0; j < availableSpace.GetLength(0); j++)
                {
                    debugWordAvailableSpace += availableSpace[j, i] + " ";
                }
                debugWords.Add(debugWordAvailableSpace);
            }

            for (int i = debugWords.Count - 1; i > -1; i--)
            {
                Debug.Log(debugWords[i]);
            }
        }
        structureStartLocation = initialStructureStartLocation;
        for(int i = 0; i < structureHeight; i++)
        {
            for(int j = 0; j < structureHeight; j++)
            {
                unknownLocations.Add(structureStartLocation + new Vector2(j, i));
            }
        }

        debugWord = "";
        openList = new List<Vector3>();
        closedList = new List<Vector3>();
        openListLocation = new List<Vector2>();
        closedListLocation = new List<Vector2>();
        maxDebugTries = 1000;
        List<Vector2Int> wallBreakLocations = new List<Vector2Int>();
        grid = availableSpace;
        height = structureHeight;
        width = structureWidth;

        wallBreakLocations = FindPath();
        if (wallBreakLocations == null)
        {
            return null;
        }
        return wallBreakLocations;
    }

    private static List<Vector2Int> FindPath()
    {
        List<Vector2Int> wallLocations = new List<Vector2Int>();
        List<Vector2Int> wallBreakLocations = new List<Vector2Int>();
        debugTries = 0;
        Vector2 startLocation = structureStartLocation;
        NodeState startLocationState = IsClearToMoveToPositionRadial(startLocation);
        int unknownLocationIndex = 0;
        while (startLocationState == NodeState.wall)
        {
            unknownLocationIndex++;
            startLocation = unknownLocations[unknownLocationIndex];
            startLocationState =  IsClearToMoveToPositionRadial(startLocation);

        }
        openList.Add(startLocation);
        openListLocation.Add(startLocation);
        while(unknownLocations.Count > 0)
        {
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
                unknownLocations.Remove(new Vector2(currentNode.x, currentNode.y));
                openList.RemoveAt(nodeIndex);
                openListLocation.RemoveAt(nodeIndex);

                List<Vector3> neighborNodes = GetNeighborList(currentNode);
                foreach (Vector3 neighborNode in neighborNodes)
                {
                    nodeState = IsClearToMoveToPositionRadial(neighborNode);

                    switch (nodeState)
                    {
                        case NodeState.wall:
                            closedList.Add(neighborNode);
                            closedListLocation.Add(new Vector2(neighborNode.x, neighborNode.y));
                            unknownLocations.Remove(new Vector2(neighborNode.x, neighborNode.y));
                            wallLocations.Add(new Vector2Int((int)neighborNode.x, (int)neighborNode.y));
                            break;
                        case NodeState.emptySpace:
                            openList.Add(neighborNode);
                            openListLocation.Add(new Vector2(neighborNode.x, neighborNode.y));
                            unknownLocations.Remove(new Vector2(neighborNode.x, neighborNode.y));
                            break;
                        case NodeState.room:
                            openList.Add(neighborNode);
                            openListLocation.Add(new Vector2(neighborNode.x, neighborNode.y));
                            unknownLocations.Remove(new Vector2(neighborNode.x, neighborNode.y));
                            break;
                        case NodeState.unit:
                            Debug.LogError("There Shouldn't be Units in structures Yet");
                            break;
                    }
                }
            }
            if (unknownLocations.Count == 0) 
            {
                return null;
            }
            bool wallIsBroken = false;
            for(int i = 0;i < wallLocations.Count; i++)
            {
                Vector3 wallPosition = new Vector3(wallLocations[i].x, wallLocations[i].y, 0);
                if (ShouldWallBeBroken(wallPosition))
                {
                    openList.Add(wallPosition);
                    openListLocation.Add(wallPosition);
                    wallBreakLocations.Add(new Vector2Int((int) wallPosition.x , (int) wallPosition.y));
                    wallIsBroken = true;
                }
            }
            if(!wallIsBroken)
            {
                Debug.LogError("You couldn't find a wall to break");
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

        Debug.LogError("Help Plze Code Should not go here Already Placed this amount of Units: ");
        return wallBreakLocations;
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
                    Vector2 newNodeLocation = new Vector2(currentNode.x + j, currentNode.y + i);
                    Vector3 newNode = new Vector3(currentNode.x + j, currentNode.y + i, currentNode.z + 1);
                    if ( newNode.x < structureStartLocation.x || newNode.x >= structureStartLocation.x + width || 
                        newNode.y < structureStartLocation.y || newNode.y >= structureStartLocation.y + height)
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

    private static bool ShouldWallBeBroken(Vector3 currentNode)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (!(i == 0 && j == 0))
                {
                    Vector2 newNodeLocation = new Vector2(currentNode.x + j, currentNode.y + i);
                    Vector3 newNode = new Vector3(currentNode.x + j, currentNode.y + i, currentNode.z + 1);
                    if (newNode.x < structureStartLocation.x || newNode.x >= structureStartLocation.x + width ||
                        newNode.y < structureStartLocation.y || newNode.y >= structureStartLocation.y + height)
                    {
                        continue;
                    }
                    if (!closedListLocation.Contains(newNodeLocation) && !openListLocation.Contains(newNodeLocation))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public static NodeState IsClearToMoveToPositionRadial(Vector3 position)
    {
        switch (grid[(int)position.x, (int)position.y])
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
}
