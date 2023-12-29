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
    public Grid<Wall>[,] wallGrid;
    public Grid<Unit>[,] unitGrid;
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

    public List<Unit> units;
    public List<Status> allStatuses;
    public List<CreatedField> createdFields = new List<CreatedField>();
    public List<AnimatedField> animatedFields = new List<AnimatedField>();

    public int least;
    public int index = 0;
    public bool aUnitActed = false;
    // during turn 0 = no; 1 = yes
    public int duringTurn = 0;

    public int numberOfStatusRemoved = 0;

    public int baseTurnTime = 500;
    public int worldPriority = 0;

    public static MainGameManger Instance;

    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        wallGrid = new Grid<Wall>[3, 3];
        unitGrid = new Grid<Unit>[3, 3];
        activeGameManagers = new List<GameManager>();
        for (int i = 0; i < gameMangers.Count; i++)
        {
            Vector3 worldPosition = gameMangers[i].defaultGridPosition;
            int gridXIndex = (int)(worldPosition.x / mapWidth);
            int gridYIndex = (int)(worldPosition.y / mapHeight);
            if (gameMangers[i].activeGameManager)
            {
                activeGameManagers.Add(gameMangers[i]);
                unitGrid[gridXIndex, gridYIndex] = gameMangers[i].grid;
                wallGrid[gridXIndex, gridYIndex] = gameMangers[i].obstacleGrid;
                wallsForInspector.Add(gameMangers[i].obstacleGrid);
            }
            else
            {
                unitGrid[gridXIndex, gridYIndex] = null;
                wallGrid[gridXIndex, gridYIndex] = null;
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

            if (activeGameManagers[i].defaultGridPosition.y + mapHeight > topMostPoint)
            {
                topMostPoint = (int)activeGameManagers[i].defaultGridPosition.y + mapHeight;
            }
        }
        CreateAStarPathing(rightMostPoint - leftMostPoint, topMostPoint - bottomMostPoint, wallGrid, unitGrid,
            new Vector3(leftMostPoint, bottomMostPoint, 0));
    }

    // Update is called once per frame 
    void Update()
    {
        if (CanContinue(units[index]))
        {
            // finds the lowest priority amongst all the units, statuses, worldtimer
            // if we are at the top of a turn
            if (duringTurn == 0)
            {
                least = units[0].priority;
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i].priority < least)
                    {
                        least = units[i].priority;
                    }
                }
                if (allStatuses.Count > 0)
                {
                    for (int i = 0; i < allStatuses.Count; i++)
                    {
                        if (allStatuses[i].statusPriority < least)
                        {
                            least = allStatuses[i].statusPriority;
                        }
                    }
                }

                if (createdFields.Count > 0)
                {
                    for (int i = 0; i < createdFields.Count; i++)
                    {
                        if (createdFields[i].createdFieldPriority < least && !createdFields[i].fromAnimatedField)
                        {
                            least = createdFields[i].createdFieldPriority;
                        }

                    }
                }

                if (animatedFields.Count > 0)
                {
                    for (int i = 0; i < animatedFields.Count; i++)
                    {
                        if (animatedFields[i].animatedFieldPriority < least)
                        {
                            least = animatedFields[i].animatedFieldPriority;
                        }

                    }
                }
            }

            //lowers priority of a unit by the least amount of priority 
            // if priority is 0 activate unit and end loop
            // if mid turn will resume list one more than unit that just went
            aUnitActed = false;
            for (int i = index + duringTurn; i < units.Count;)
            {
                units[i].priority = units[i].priority - least;

                if (units[i].priority == 0)
                {
                    index = i;
                    duringTurn = 1;
                    units[i].enabled = true;
                    aUnitActed = true;
                    break;
                }
                else if (i == 0)
                {
                    duringTurn = 1;
                    i++;
                }
                else
                {
                    index = i;
                    i++;
                }
            }


            //end turn reset all turn variables and reset priority of units who acted
            if (index + duringTurn == units.Count && !aUnitActed)
            {
                index = 0;
                duringTurn = 0;
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i].priority <= 0)
                    {
                        units[i].priority = (int)(baseTurnTime * units[i].quickness);
                    }
                }

                if (allStatuses.Count > 0)
                {
                    numberOfStatusRemoved = 0;
                    for (int i = 0; i < allStatuses.Count; i++)
                    {
                        if (i < 0)
                        {
                            break;
                        }
                        if (allStatuses[i].ApplyEveryTurn && allStatuses[i].isFirstWorldTurn)
                        {
                            allStatuses[i].isFirstWorldTurn = false;
                        }
                        else
                        {
                            allStatuses[i].statusPriority -= least;
                            if (allStatuses[i].statusPriority <= 0)
                            {
                                allStatuses[i].statusPriority = (int)(allStatuses[i].statusQuickness * baseTurnTime);
                                Unit tempUnit = allStatuses[i].targetUnit;
                                int tempIndex = tempUnit.statuses.Count;

                                //reduces status duration of a status if it is supposed to go down at the end of a turn
                                if (!allStatuses[i].nonStandardDuration)
                                {
                                    allStatuses[i].currentStatusDuration -= 1;
                                }

                                // if an affect applyies everyturn apply the affect if it isn't the  turn it was activated

                                if (allStatuses[i].ApplyEveryTurn)
                                {
                                    allStatuses[i].ApplyEffect(tempUnit, -1);
                                }

                                //if a status was removed in previous step reset sprite of unit otherwise if a status duration is 0 remove the status from the unit and reset the units sprite
                                if (tempUnit.statuses.Count != tempIndex || allStatuses[i].currentStatusDuration <= 0)
                                {
                                    if (tempUnit.statuses.Count == tempIndex)
                                    {
                                        allStatuses[i].RemoveEffect(tempUnit);
                                    }
                                    tempUnit.ChangeSprite(tempUnit.originalSprite);
                                    tempUnit.spriteIndex = -1;

                                }
                                i -= numberOfStatusRemoved;
                                numberOfStatusRemoved = 0;
                            }
                        }
                    }
                }

                // change Status Priority at top of turn
                if (createdFields.Count > 0)
                {
                    for (int i = 0; i < createdFields.Count; i++)
                    {
                        if (createdFields[i].fromAnimatedField)
                        {
                            continue;
                        }
                        createdFields[i].createdFieldPriority -= least;
                        if (createdFields[i].createdFieldPriority <= 0)
                        {
                            createdFields[i].createdFieldPriority = (int)(createdFields[i].createdFieldQuickness * baseTurnTime);

                            //reduces status duration of a status if it is supposed to go down at the end of a turn
                            if (!createdFields[i].nonStandardDuration)
                            {
                                createdFields[i].currentCreatedFieldDuration -= 1;
                            }

                            //if a status was removed in previous step reset sprite of unit otherwise if a status duration is 0 remove the status from the unit and reset the units sprite
                            if (createdFields[i].currentCreatedFieldDuration <= 0)
                            {
                                createdFields[i].RemoveStatusOnDeletion();
                            }
                        }
                    }
                }

                if (animatedFields.Count > 0)
                {
                    for (int i = 0; i < animatedFields.Count; i++)
                    {
                        animatedFields[i].animatedFieldPriority -= least;
                        if (animatedFields[i].animatedFieldPriority <= 0)
                        {
                            animatedFields[i].animatedFieldPriority = (int)(animatedFields[i].createdFieldQuickness * baseTurnTime);

                            animatedFields[i].Activate();
                            //Animated Fields Handle thier Own Deletion
                        }
                    }
                }
            }
        }
    }


    private bool CanContinue(MonoBehaviour script)
    {
        return !script.isActiveAndEnabled;
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
                unitGrid[gridXIndex, gridYIndex] = gameMangers[i].grid;
                wallGrid[gridXIndex, gridYIndex] = gameMangers[i].obstacleGrid;
            }
            else
            {
                unitGrid[gridXIndex, gridYIndex] = null;
                wallGrid[gridXIndex, gridYIndex] = null;
            }
        }
    }

    public void CreateAStarPathing(int width, int height, Grid<Wall>[,] initialWalls, Grid<Unit>[,] initialUnits, Vector3 initalPosition)
    {
        wallGrid = initialWalls;
        unitGrid = initialUnits;
        path = new AStarPathfinding(width, height, mapWidth, mapHeight, wallGrid, unitGrid, initalPosition + new Vector3(-0.5f, -0.5f, 0));
    }

    public void UpdatePathFindingGrid(Vector3 worldPosition, Grid<Wall> wall, Grid<Unit> unit)
    {
        Grid<AStarPathNode> AStarGrid = path.GetGrid();
        int gridXIndex = (int)(worldPosition.x / mapWidth);
        int gridYIndex = (int)(worldPosition.y / mapHeight);
        wallGrid[gridXIndex, gridYIndex] = wall;
        unitGrid[gridXIndex, gridYIndex] = unit;

        int x;
        int y;
        AStarGrid.GetXY(worldPosition, out x, out y);
        AStarPathNode node = new AStarPathNode(AStarGrid, x, y, mapWidth, mapHeight, wallGrid, unitGrid);
        AStarGrid.SetGridObject(worldPosition, node);
    }

    public void FreezeGameManager(Vector2Int gameManagerDirection)
    {
        int gridXIndex = gameManagerDirection.x + 1;
        int gridYIndex = gameManagerDirection.y + 1;

        wallGrid[gridXIndex, gridYIndex] = null;
        unitGrid[gridXIndex, gridYIndex] = null;
        GameManager frozenGameManager = gameMangers[gridXIndex + gridYIndex * 3];
        activeGameManagers.Remove(frozenGameManager);
        for(int i = frozenGameManager.units.Count - 1; i >= 0; i--)
        {
            if (frozenGameManager.units.Count == 0)
            {
                break;
            }
            frozenGameManager.units[i].Death();
        }

        for (int i = frozenGameManager.walls.Count - 1; i >= 0; i--)
        {
            if (frozenGameManager.walls.Count == 0)
            {
                break;
            }
            frozenGameManager.walls[i].Death();
        }

        for(int i = frozenGameManager.items.Count - 1; i >= 0; i--)
        {
            if (frozenGameManager.items.Count == 0)
            {
                break;
            }
            Destroy(frozenGameManager.items[i].gameObject);
        }
        frozenGameManager.items = new List<Item>();

        Vector3 worldPosition;
        AStarPathNode node;
        int x, y;
        Grid<AStarPathNode> AStarGrid = path.GetGrid();

        // make AStarGird Not Walkable on bottom of Frozen GameManager
        if (frozenGameManager.gameManagerDirection.y != -1)
        {
            for (int i = 0; i < mapWidth; i++)
            {
                worldPosition = frozenGameManager.defaultGridPosition + new Vector3(i, 0, 0);
                AStarGrid.GetXY(worldPosition, out x, out y);
                node = new AStarPathNode(AStarGrid, x, y, false);
                AStarGrid.SetGridObject(worldPosition, node);
            }
        }
        // make AStarGird Not Walkable on Top of Frozen GameManager
        if (frozenGameManager.gameManagerDirection.y != 1)
        {
            for (int i = 0; i < mapWidth; i++)
            {
                worldPosition = frozenGameManager.defaultGridPosition + new Vector3(i, mapHeight - 1, 0);
                AStarGrid.GetXY(worldPosition, out x, out y);
                node = new AStarPathNode(AStarGrid, x, y, false);
                AStarGrid.SetGridObject(worldPosition, node);
            }
        }

        // make AStarGird Not Walkable on Left Side of Frozen GameManager
        if (frozenGameManager.gameManagerDirection.x != -1)
        {
            for (int i = 0; i < mapHeight; i++)
            {
                worldPosition = frozenGameManager.defaultGridPosition + new Vector3(0, i, 0);
                AStarGrid.GetXY(worldPosition, out x, out y);
                node = new AStarPathNode(AStarGrid, x, y, false);
                AStarGrid.SetGridObject(worldPosition, node);
            }
        }

        // make AStarGird Not Walkable on Right Side of Frozen GameManager
        if (frozenGameManager.gameManagerDirection.x != 1)
        {
            for (int i = 0; i < mapHeight; i++)
            {
                worldPosition = frozenGameManager.defaultGridPosition + new Vector3(mapWidth - 1, i, 0);
                AStarGrid.GetXY(worldPosition, out x, out y);
                node = new AStarPathNode(AStarGrid, x, y, false);
                AStarGrid.SetGridObject(worldPosition, node);
            }
        }
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
            int index = unit.gameManager.units.IndexOf(unit);

            for(int i = 0; i < unit.statuses.Count; i++)
            {
                unit.gameManager.numberOfStatusRemoved += 1;
                int statusindex = unit.gameManager.allStatuses.IndexOf(unit.statuses[i]);

                newGameManager.allStatuses.Add(unit.gameManager.allStatuses[statusindex]);

                unit.gameManager.allStatuses.RemoveAt(statusindex);
            }

            newGameManager.units.Add(unit.gameManager.units[index]);
            newGameManager.isLocationChangeStatus += unit.hasLocationChangeStatus;

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
