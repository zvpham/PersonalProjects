using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MainGameManger : MonoBehaviour
{
    public AStarPathfinding path;
    public List<Grid<Wall>> wallsForInspector = new List<Grid<Wall>>();
    public Grid<Wall>[,] walls;
    public Grid<Unit>[,] units;
    public int mapWidth;
    public int mapHeight;
    public MapManager mapManager;
    public List<GameManager> gameMangers;
    public List<GameManager> activeGameManagers;
    public GameManager centralGameManager;
    public GameManager BLGameManager;
    public GameManager BGameManager;
    public GameManager BRGameManager;
    public GameManager MLGameManager;
    public GameManager MRGameManager;
    public GameManager TLGameManager;
    public GameManager TGameManager;
    public GameManager TRGameManager;

    public static MainGameManger Instance;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        walls = new Grid<Wall>[3, 3];
        units = new Grid<Unit>[3, 3];
        activeGameManagers = new List<GameManager>();
        for (int i = 0; i < gameMangers.Count; i++)
        {
            Vector3 worldPosition = gameMangers[i].defaultGridPosition;
            int gridXIndex = (int)(worldPosition.x / mapWidth);
            int gridYIndex = (int)(worldPosition.y / mapHeight);
            if (gameMangers[i].activeGameManager)
            {
                activeGameManagers.Add(gameMangers[i]);
                units[gridXIndex, gridYIndex] = gameMangers[i].grid;
                walls[gridXIndex, gridYIndex] = gameMangers[i].obstacleGrid;
                wallsForInspector.Add(gameMangers[i].obstacleGrid);
            }
            else
            {
                units[gridXIndex, gridYIndex] = null;
                walls[gridXIndex, gridYIndex] = null;
                gameMangers[i].enabled = false;
                gameMangers[i].gameObject.SetActive(false);
            }
        }

        int leftMostPoint = (int)activeGameManagers[0].defaultGridPosition.x;
        int rightMostPoint = (int)activeGameManagers[0].defaultGridPosition.x;
        int topMostPoint = (int)activeGameManagers[0].defaultGridPosition.y;
        int bottomMostPoint = (int)activeGameManagers[0].defaultGridPosition.y;

        for (int i = 0; i < activeGameManagers.Count; i++)
        {
            if (activeGameManagers[i].defaultGridPosition.x < leftMostPoint)
            {
                leftMostPoint = (int)activeGameManagers[i].defaultGridPosition.x;
            }

            if (activeGameManagers[i].defaultGridPosition.x + mapWidth > rightMostPoint)
            {
                rightMostPoint = (int)activeGameManagers[i].defaultGridPosition.x + mapWidth;
            }

            if (activeGameManagers[i].defaultGridPosition.y < bottomMostPoint)
            {
                bottomMostPoint = (int)activeGameManagers[i].defaultGridPosition.y;
            }

            if (activeGameManagers[i].defaultGridPosition.y + mapHeight < leftMostPoint)
            {
                topMostPoint = (int)activeGameManagers[i].defaultGridPosition.y + mapHeight;
            }
        }
        CreateAStarPathing(rightMostPoint - leftMostPoint, topMostPoint - bottomMostPoint, walls, units,
            new Vector3(leftMostPoint, bottomMostPoint, 0));
    }

    public void CreateGrid()
    {
        activeGameManagers = new List<GameManager>();
        for (int i = 0; i < gameMangers.Count; i++)
        {
            Vector3 worldPosition = gameMangers[i].defaultGridPosition;
            int gridXIndex = (int)(worldPosition.x / mapWidth);
            int gridYIndex = (int)(worldPosition.y / mapHeight);
            if (gameMangers[i].activeGameManager)
            {
                units[gridXIndex, gridYIndex] = gameMangers[i].grid;
                walls[gridXIndex, gridYIndex] = gameMangers[i].obstacleGrid;
            }
            else
            {
                units[gridXIndex, gridYIndex] = null;
                walls[gridXIndex, gridYIndex] = null;
            }
        }
    }

    public void CreateAStarPathing(int width, int height, Grid<Wall>[,] initialWalls, Grid<Unit>[,] initialUnits, Vector3 initalPosition)
    {
        walls = initialWalls;
        units = initialUnits;
        path = new AStarPathfinding(width, height, mapWidth, mapHeight, walls, units, initalPosition + new Vector3(-0.5f, -0.5f, 0));
    }

    public void UpdatePathFindingGrid(Vector3 worldPosition, Grid<Wall> wall, Grid<Unit> unit)
    {
        Grid<AStarPathNode> AStarGrid = path.GetGrid();
        int gridXIndex = (int)(worldPosition.x / mapWidth);
        int gridYIndex = (int)(worldPosition.y / mapHeight);
        walls[gridXIndex, gridYIndex] = wall;
        units[gridXIndex, gridYIndex] = unit;

        int x;
        int y;
        AStarGrid.GetXY(worldPosition, out x, out y);
        AStarPathNode node = new AStarPathNode(AStarGrid, x, y, mapWidth, mapHeight, walls, units);
        AStarGrid.SetGridObject(worldPosition, node);
    }

    // Assumption is that space unit is going to is empty, Should be resolved by chase action not by this
    public void TransferGameManagers(Vector3 worldPosition, Unit unit)
    {
        int gridXIndex = (int)(worldPosition.x / mapWidth);
        int gridYIndex = (int)(worldPosition.y / mapHeight);
        GameManager newGameManager = gameMangers[gridXIndex + gridYIndex * 3];
        if (newGameManager ==  unit.gameManager)
        {
            return;
        }

        if(gridXIndex + gridYIndex * 3 == 4)
        {
            unit.inPeripheralGameManager = false;
            int index = centralGameManager.units.IndexOf(unit);

            for(int i = 0; i < unit.statuses.Count; i++)
            {
                unit.gameManager.numberOfStatusRemoved += 1;
                int statusindex = unit.gameManager.allStatuses.IndexOf(unit.statuses[i]);

                newGameManager.statusPriority.Add(unit.gameManager.statusPriority[statusindex]);
                newGameManager.allStatuses.Add(unit.gameManager.allStatuses[statusindex]);
                newGameManager.statusDuration.Add(unit.gameManager.statusDuration[statusindex]);

                unit.gameManager.statusPriority.RemoveAt(statusindex);
                unit.gameManager.allStatuses.RemoveAt(statusindex);
                unit.gameManager.statusDuration.RemoveAt(statusindex);
            }

            newGameManager.speeds.Add(unit.gameManager.speeds[index]);
            newGameManager.priority.Add(unit.gameManager.priority[index]);
            newGameManager.units.Add(unit.gameManager.units[index]);
            newGameManager.isLocationChangeStatus += unit.hasLocationChangeStatus;

            unit.gameManager.speeds.RemoveAt(index);
            unit.gameManager.priority.RemoveAt(index);
            unit.gameManager.units.RemoveAt(index);
            unit.gameManager.isLocationChangeStatus -= unit.hasLocationChangeStatus;

            unit.gameManager = newGameManager;        
        }
        else
        {

            Debug.LogError("This Really shouldn't be happening. This unit left Central GameManager: " + unit.name);
        }
    }

    public GameManager GetGameManger(Vector3 worldPosition)
    {
        int gridXIndex = (int)(worldPosition.x / mapWidth);
        int gridYIndex = (int)(worldPosition.y / mapHeight);
        GameManager requestedGameManager = gameMangers[gridXIndex + gridYIndex * 3];
        return requestedGameManager;
    }
}
