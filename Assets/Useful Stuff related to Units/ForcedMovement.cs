using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using static ForcedMovement;
using static UnityEngine.UI.CanvasScaler;

public static class ForcedMovement
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

    public enum NodeState
    {
        wall,
        unit,
        emptySpace
    }
    public static void MoveUnit(Unit movingUnit)
    {
        debugWord = "";
        openList = new List<Vector3>();
        closedList = new List<Vector3>();
        maxDebugTries = 100;

        gameManager = movingUnit.gameManager;
        if (!FindPath(movingUnit, false))
        {
            /*
            debugWord = "Open List ";
            foreach (Vector3 node in openList)
            {
                debugWord += node.ToString() +  " ";
            }
            Debug.Log(debugWord);

            debugWord = "Closed List ";
            foreach (Vector3 node in closedList)
            {
                debugWord += node.ToString() + " ";
            }
            Debug.Log(debugWord);
            */
            openList = new List<Vector3>();
            closedList = new List<Vector3>();

            FindPath(movingUnit, true);
            /*
            debugWord = "Open List ";
            foreach (Vector3 node in openList)
            {
                debugWord += node.ToString() + " ";
            }

            Debug.Log(debugWord);
            debugWord = "Closed List ";
            foreach (Vector3 node in closedList)
            {
                debugWord += node.ToString() + " ";
            }
            Debug.Log(debugWord);
            */
        }
    }

    private static bool FindPath(Unit movingUnit, bool ignoreWalls)
    {
        debugTries = 0;
        openList.Add(movingUnit.gameObject.transform.position);
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
                    return CalculatePath(neighborNode, movingUnit);
                }
            }
        }
        return false;
    }
    private static bool CalculatePath(Vector3 endNode, Unit movingUnit)
    {
        closedList.Add(endNode);
        AStarPathfinding path = new AStarPathfinding(gameManager.grid.GetWidth(), gameManager.grid.GetHeight(), closedList, Vector3.zero);
        List<AStarPathNode> movementPath = path.FindPath(movingUnit.gameObject.transform.position, endNode);
        if (movementPath != null)
        {
            storedUnit = null;
            nextUnit = null;
            Vector3 prevNodePosition;
            Vector3 newNodePosition;
            for (int i = 1; i < movementPath.Count; i++)
            {
                if (i == 1)
                {
                    storedUnit = movingUnit.gameManager.grid.GetGridObject(movementPath[i].grid.GetWorldPosition(movementPath[i].x, movementPath[i].y));
                    nextUnit = movingUnit.gameManager.grid.GetGridObject(movementPath[i - 1].grid.GetWorldPosition(movementPath[i - 1].x, movementPath[i - 1].y));
                    prevNodePosition = movementPath[i - 1].grid.GetWorldPosition(movementPath[i - 1].x, movementPath[i - 1].y);
                    movingUnit.gameManager.ChangeUnits(prevNodePosition, null);
                    newNodePosition = movementPath[i].grid.GetWorldPosition(movementPath[i].x, movementPath[i].y);
                    movingUnit.gameManager.ChangeUnits(newNodePosition, nextUnit);
                    movingUnit.gameObject.transform.position = newNodePosition;
                }
                else
                {
                    nextUnit = storedUnit;
                    storedUnit = movingUnit.gameManager.grid.GetGridObject(movementPath[i].grid.GetWorldPosition(movementPath[i].x, movementPath[i].y));
                    newNodePosition = movementPath[i].grid.GetWorldPosition(movementPath[i].x, movementPath[i].y);
                    movingUnit.gameManager.ChangeUnits(newNodePosition, nextUnit);
                    nextUnit.gameObject.transform.position = newNodePosition;
                    nextUnit = storedUnit;
                }
            }
            return true;
        }
        return false;
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
                if(!(i == 0 && j == 0))
                {
                    Vector3 newNode = new Vector3(currentNode.x + j, currentNode.y + i, currentNode.z + 1);

                    if(!closedList.Contains(newNode) || !openList.Contains(newNode))
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
