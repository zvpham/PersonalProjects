using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class MovementTargeting : TargetingSystem
{
    public DijkstraMap map;
    public CombatGameManager gameManager;
    public Unit movingUnit;
    public Vector3 startingPosition;
    public Vector3 endPosition;
    public Vector2Int prevEndHexPosition;
    public List<Vector2Int> path = new List<Vector2Int>();
    public List<Vector2Int> setPath = new List<Vector2Int>();
    public GameObject tempMovingUnit;
    public int oneActionMoveAmount;
    public int twoActionMoveAmount;
    public int amountMoved = 0;

    public List<Vector2Int> firstRangeBracket = new List<Vector2Int>();
    public List<Vector2Int> secondRangeBracket = new List<Vector2Int>();
    public List<Vector2Int> highlightedPositions = new List<Vector2Int>();

    public bool targetFriendly = false;

    public int actionPointsLeft;

    public UnityAction<List<Vector2Int>, Unit, bool> OnFoundTarget;

    public void SetParameters(Unit movingUnit, bool targetFriendly, int actionPointsLeft)
    {
        this.targetFriendly = targetFriendly;
        this.movingUnit = movingUnit;
        this.gameManager = movingUnit.gameManager;
        this.actionPointsLeft = actionPointsLeft;
        map = movingUnit.gameManager.map;
        startingPosition = movingUnit.transform.position;
        prevEndHexPosition = new Vector2Int(-10, 0);
        path = new List<Vector2Int>();
        this.enabled = true;
        amountMoved = movingUnit.amountMoveUsedDuringRound;

        SetUp(startingPosition, actionPointsLeft, movingUnit.moveSpeed);
        for (int i = 0; i < gameManager.units.Count; i++)
        {
            if (gameManager.units[i].team != movingUnit.team)
            {
                map.getGrid().GetXY(gameManager.units[i].transform.position, out int x, out int y);
                map.SetUnwalkable(new Vector2Int(x, y));
            }
        }
    }

    public void SetUp(Vector3 targetPosition, int numActionPoints, int moveSpeed)
    {
        ResetSetUp();

        DijkstraMap map = gameManager.map;
        map.getGrid().GetXY(targetPosition, out int x, out int y);
        map.ResetMap();
        map.SetGoals(new List<Vector2Int>() { new Vector2Int(x, y) });
        startingPosition = targetPosition;
        firstRangeBracket.Clear();
        secondRangeBracket.Clear();

        actionPointsLeft = numActionPoints;

        int initialActionPoints = numActionPoints;
        int moveAmounts = 0;
        while(initialActionPoints > 0)
        {
            if(initialActionPoints >= movingUnit.amountMoveUsedDuringRound + moveAmounts +  1)
            {
                moveAmounts += 1;
                initialActionPoints -= movingUnit.amountMoveUsedDuringRound + moveAmounts;
            }
            else
            {
                break;
            }
        }

        if (moveAmounts == 2)
        {
            for (int i = 1; i <= moveSpeed * 2; i++)
            {
                List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(x, y, i);
                for (int j = 0; j < mapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                    if (map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= moveSpeed)
                    {
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 2);
                        firstRangeBracket.Add(currentNodePosition);
                        highlightedPositions.Add(currentNodePosition);
                    }
                    else if (map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= moveSpeed * 2)
                    {
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 4);
                        secondRangeBracket.Add(currentNodePosition);
                        highlightedPositions.Add(currentNodePosition);
                    }
                }
            }
        }
        else if (moveAmounts == 1)
        {
            for (int i = 1; i <= moveSpeed; i++)
            {
                List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(x, y, i);
                for (int j = 0; j < mapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                    if (map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= moveSpeed)
                    {
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 4);
                        firstRangeBracket.Add(currentNodePosition);
                        highlightedPositions.Add(currentNodePosition);
                    }
                }
            }
        }
    }

    // Clear Highlighted Hexes
    public void ResetSetUp()
    {
        for (int i = 0; i < highlightedPositions.Count; i++)
        {
            gameManager.spriteManager.ChangeTile(highlightedPositions[i], 1, 0);
        }
        highlightedPositions = new List<Vector2Int>();
    }

    public override void SelectNewPosition(Vector3 newPosition)
    {
        endPosition = newPosition;
        map.ResetMap();
        List<Vector2Int> endHex = new List<Vector2Int>();
        map.getGrid().GetXY(newPosition, out int endX, out int endY);
        if (map.getGrid().GetGridObject(endX, endY) != null)
        {
            oneActionMoveAmount = 0;
            twoActionMoveAmount = 0;
            if (actionPointsLeft >= amountMoved + 1 + amountMoved + 2)
            {
                endHex.Add(new Vector2Int(endX, endY));
                map.SetGoals(endHex);
                map.getGrid().GetXY(startingPosition, out int x, out int y);
                path.Clear();
                bool foundEndPosition = false;
                bool inFirstRangeBracket = firstRangeBracket.Contains(new Vector2Int(endX, endY));

                for (int i = 0; i < movingUnit.moveSpeed; i++)
                {
                    DijkstraMapNode nextLowestNode = map.GetLowestNearbyNode(x, y);
                    x = nextLowestNode.x;
                    y = nextLowestNode.y;
                    path.Add(new Vector2Int(x, y));
                    oneActionMoveAmount++;
                    if (endHex[0] == path[i])
                    {
                        foundEndPosition = true;
                        break;
                    }
                }

                // SecondAction Movement
                if (!foundEndPosition && !inFirstRangeBracket)
                {
                    for (int i = 0; i < movingUnit.moveSpeed; i++)
                    {
                        DijkstraMapNode nextLowestNode = map.GetLowestNearbyNode(x, y);
                        x = nextLowestNode.x;
                        y = nextLowestNode.y;
                        path.Add(new Vector2Int(x, y));
                        twoActionMoveAmount++;
                        if (endHex[0] == path[i + oneActionMoveAmount])
                        {
                            foundEndPosition = true;
                            break;
                        }
                    }
                }


                // Redraw Path Through Unoptimal Terrain to Reach Target (Not Implemented)
                if (!foundEndPosition)
                {
                    endHex.Add(new Vector2Int(endX, endY));
                    map.SetGoals(endHex);
                    map.getGrid().GetXY(movingUnit.transform.position, out x, out y);
                    path.Clear();
                    oneActionMoveAmount = 0;
                    foundEndPosition = false;

                    for (int i = 0; i < movingUnit.moveSpeed; i++)
                    {
                        DijkstraMapNode nextLowestNode = map.GetLowestNearbyNode(x, y);
                        x = nextLowestNode.x;
                        y = nextLowestNode.y;
                        path.Add(new Vector2Int(x, y));
                        oneActionMoveAmount++;
                        if (endHex[0] == path[i])
                        {
                            foundEndPosition = true;
                            break;
                        }
                    }

                    // SecondAction Movement
                    if (!foundEndPosition && !inFirstRangeBracket)
                    {
                        twoActionMoveAmount = 0;
                        for (int i = 0; i < movingUnit.moveSpeed; i++)
                        {
                            DijkstraMapNode nextLowestNode = map.GetLowestNearbyNode(x, y);
                            x = nextLowestNode.x;
                            y = nextLowestNode.y;
                            path.Add(new Vector2Int(x, y));
                            twoActionMoveAmount++;
                            if (endHex[0] == path[i + oneActionMoveAmount])
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else if (actionPointsLeft >= amountMoved + 1)
            {
                endHex.Add(new Vector2Int(endX, endY));
                map.SetGoals(endHex);
                map.getGrid().GetXY(startingPosition, out int x, out int y);
                path.Clear();
                twoActionMoveAmount = 0;

                for (int i = 0; i < movingUnit.moveSpeed; i++)
                {
                    DijkstraMapNode nextLowestNode = map.GetLowestNearbyNode(x, y);
                    x = nextLowestNode.x;
                    y = nextLowestNode.y;
                    path.Add(new Vector2Int(x, y));
                    twoActionMoveAmount++;
                    if (endHex[0] == path[i])
                    {
                        break;
                    }
                }
            }
            DrawLine();
        }  
    }
    public override void EndTargeting()
    {
        gameManager.grid.GetXY(endPosition, out int x, out int y);

        if (x >= gameManager.grid.GetWidth() || x < 0 || y >= gameManager.grid.GetHeight() || y < 0)
        {
            return;
        }

        Vector2Int endHexPosition = new Vector2Int(x, y);
         
        // Case -  Player Clicks on previously selected Hex
        // Confirms action
        if (prevEndHexPosition.x >= 0 && endHexPosition == prevEndHexPosition)
        {
                gameManager.spriteManager.ConfirmAction();
        }
        //Case - Player clicks on hex that has not been previously selected
        else
        {
            // Select Player Unit when you aren't using a friendly ability like heal
            if (!targetFriendly && gameManager.playerTurn.CheckSpaceForFriendlyUnit(endHexPosition))
            {
                OnFoundTarget?.Invoke(null, movingUnit, false);
                gameManager.playerTurn.SelectUnit(endHexPosition);
            }
            else
            {
                //if there is still action Points left Add temp Path to Set Path
                if (actionPointsLeft > 0)
                {
                    prevEndHexPosition = endHexPosition;
                    for (int i = 0; i < path.Count; i++)
                    {
                        setPath.Add(path[i]);
                    }
                }

                if (firstRangeBracket.Contains(endHexPosition))
                {
                    actionPointsLeft -= amountMoved + 1;
                    amountMoved += 1;
                }
                else if (secondRangeBracket.Contains(endHexPosition))
                {
                    actionPointsLeft -= amountMoved + 1;
                    actionPointsLeft -= amountMoved + 2;
                    amountMoved += 2;
                }


                Vector2Int endPathHex = setPath[setPath.Count - 1];
                Destroy(tempMovingUnit);
                tempMovingUnit = null;
                tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, movingUnit.unitProfile);
                SetUp(map.getGrid().GetWorldPosition(setPath[setPath.Count - 1]), actionPointsLeft, movingUnit.moveSpeed);
                gameManager.spriteManager.ActivateActionConfirmationMenu(
                    () => // Confirm Action
                    {
                        OnFoundTarget?.Invoke(setPath, movingUnit, true);
                        Destroy(tempMovingUnit);
                    },
                    () => // Cancel Action
                    {
                        SetUp(movingUnit.transform.position, movingUnit.currentActionsPoints, movingUnit.moveSpeed);
                        ResetTargeting();
                    });
                
            }
        }
    }

    public void ResetTargeting()
    {
        Destroy(tempMovingUnit);
        tempMovingUnit = null;
        setPath.Clear();
        prevEndHexPosition = new Vector2Int(-1, -1);
    }

    public override void DeactivateTargetingSystem()
    {
        for (int i = 0; i < highlightedPositions.Count; i++)
        {
            gameManager.spriteManager.ChangeTile(highlightedPositions[i], 2, 0);
        }

        ResetSetUp();
        ResetTargeting();
        map.ResetMap(true);
        gameManager.spriteManager.ClearLines();
        setPath = new List<Vector2Int>();
        path = new List<Vector2Int>();
        OnFoundTarget = null;
    }

    public void DrawLine()
    {
        if (actionPointsLeft <= 0)
        {
            return;
        }
        List<Vector3> oneActionLine = new List<Vector3>();
        List<Vector3> twoActionLine = new List<Vector3>();

        if (setPath.Count > 0)
        {
            oneActionLine = new List<Vector3>() { movingUnit.transform.position };
            for (int i = 0; i < setPath.Count; i++)
            {
                oneActionLine.Add(map.getGrid().GetWorldPosition(setPath[i].x, setPath[i].y));
            }
        }
        else
        {
            oneActionLine = new List<Vector3>() { movingUnit.transform.position };
            for (int i = 0; i < oneActionMoveAmount; i++)
            {
                oneActionLine.Add(map.getGrid().GetWorldPosition(path[i].x, path[i].y));
            }
        }

        if (setPath.Count > 0)
        {
            twoActionLine.Add(oneActionLine[oneActionLine.Count - 1]);
            for (int i = 0; i < twoActionMoveAmount; i++)
            {
                twoActionLine.Add(map.getGrid().GetWorldPosition(path[i].x, path[i].y));
            }
        }
        else
        {
            twoActionLine.Add(oneActionLine[oneActionLine.Count - 1]);
            for (int i = 0; i < twoActionMoveAmount; i++)
            {
                twoActionLine.Add(map.getGrid().GetWorldPosition(path[i + oneActionMoveAmount].x, path[i + oneActionMoveAmount].y));
            }
        }
        gameManager.spriteManager.DrawLine(oneActionLine, twoActionLine);
    }
}
