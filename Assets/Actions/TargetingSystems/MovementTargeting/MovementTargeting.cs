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

    public List<int> actionMoveAmounts;
    public int amountMoved = 0;
    public int amountOfPossibleMoves;

    public int amountActionLineIncreased = 0;
    public int IndexOfStartingActionLine = 0;
    List<List<Vector3>> actionLines = new List<List<Vector3>>();

    public List<List<Vector2Int>> rangeBrackets =  new List<List<Vector2Int>>();
    public List<Vector2Int> highlightedPositions = new List<Vector2Int>();

    public bool targetFriendly = false;

    public int actionPointsLeft;

    public UnityAction<List<Vector2Int>, Unit, int,  bool> OnFoundTarget;

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
    }

    public void SetUp(Vector3 targetPosition, int numActionPoints, int moveSpeed)
    {
        ResetSetUp();

        DijkstraMap map = gameManager.map;
        map.getGrid().GetXY(targetPosition, out int x, out int y);
        map.ResetMap(true);

        for (int i = 0; i < gameManager.units.Count; i++)
        {
            if (gameManager.units[i].team != movingUnit.team)
            {
                map.getGrid().GetXY(gameManager.units[i].transform.position, out int unitX, out int unitY);
                map.SetUnwalkable(new Vector2Int(unitX, unitY));
            }
        }

        map.SetGoals(new List<Vector2Int>() { new Vector2Int(x, y) });
        startingPosition = targetPosition;

        actionPointsLeft = numActionPoints;

        int initialActionPoints = numActionPoints;
        int moveAmounts = 0;
        while(initialActionPoints > 0)
        {
            if(initialActionPoints >= amountMoved + moveAmounts +  1)
            {
                moveAmounts += 1;
                initialActionPoints -= amountMoved + moveAmounts;
            }
            else
            {
                break;
            }
        }

        List<DijkstraMapNode> mapNodes;
        List<DijkstraMapNode> unresolvedMapNodes = new List<DijkstraMapNode>();
        rangeBrackets = new List<List<Vector2Int>>();

        amountOfPossibleMoves = moveAmounts;
        int currentRadius = 1;
        if (moveAmounts > 0)
        {
            for (int i = 1; i <= moveAmounts; i++)
            {

                rangeBrackets.Add(new List<Vector2Int>());
                for (int j = 0; j < unresolvedMapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(unresolvedMapNodes[j].x, unresolvedMapNodes[j].y);
                    if (map.getGrid().GetGridObject(unresolvedMapNodes[j].x, unresolvedMapNodes[j].y).value <= moveSpeed * i)
                    {
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, amountMoved +  i + 1);
                        rangeBrackets[i - 1].Add(currentNodePosition);
                        highlightedPositions.Add(currentNodePosition);
                        unresolvedMapNodes.RemoveAt(j);
                        j--;
                    }
                }

                for (int j = currentRadius; j <= moveSpeed * i; j++)
                {
                    mapNodes = map.getGrid().GetGridObjectsInRing(x, y, j);
                    for (int k = 0; k < mapNodes.Count; k++)
                    {
                        Vector2Int currentNodePosition = new Vector2Int(mapNodes[k].x, mapNodes[k].y);
                        if (map.getGrid().GetGridObject(mapNodes[k].x, mapNodes[k].y).value <= moveSpeed * i)
                        {
                            gameManager.spriteManager.ChangeTile(currentNodePosition, 1, amountMoved + i + 1);
                            rangeBrackets[i - 1].Add(currentNodePosition);
                            highlightedPositions.Add(currentNodePosition);
                        }
                        else
                        {
                            unresolvedMapNodes.Add(mapNodes[k]);
                        }
                    }
                    currentRadius += 1;
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

    // Select New Position when Mouse Hovers over a new Hex
    public override void SelectNewPosition(Vector3 newPosition)
    {
        endPosition = newPosition;
        map.ResetMap();
        List<Vector2Int> endHex = new List<Vector2Int>();
        map.getGrid().GetXY(newPosition, out int endX, out int endY);
        amountActionLineIncreased = 0;
        if (map.getGrid().GetGridObject(endX, endY) != null)
        {
            endHex.Add(new Vector2Int(endX, endY));
            map.SetGoals(endHex);
            map.getGrid().GetXY(startingPosition, out int x, out int y);
            path.Clear();
            bool foundEndPosition = false;
            bool findMostDirectPath = false;
            int initialAmountPathMoveIncreased = amountActionLineIncreased;

            // loop through possible move amounts to find a path to end hex
            for (int i = 0; i < amountOfPossibleMoves; i++)
            {
                actionMoveAmounts.Add(0);
                amountActionLineIncreased += 1;
                bool inRangeBracket = rangeBrackets[i].Contains(new Vector2Int(endX, endY));
                int startx = x;
                int starty = y;

                // Attempt to find path Avoiding harmful Terrain
                for (int j = 0; j < movingUnit.moveSpeed; j++)
                {
                    DijkstraMapNode nextLowestNode = map.GetLowestNearbyNode(x, y);
                    x = nextLowestNode.x;
                    y = nextLowestNode.y;
                    path.Add(new Vector2Int(x, y));
                    actionMoveAmounts[i] = actionMoveAmounts[i] + 1;
                    if (endHex[0] == path[path.Count - 1])
                    {
                        foundEndPosition = true;
                        break;
                    }
                    else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
                    {
                        path.RemoveAt(path.Count - 1);
                        foundEndPosition = false;
                        actionMoveAmounts[i] = actionMoveAmounts[i] - 1;
                        break;
                    }
                }

                if (foundEndPosition)
                {
                    break;
                }

                // If it is possible to reach end position in range bracket Reset data and try again
                if (inRangeBracket && !foundEndPosition)
                {
                    // Reset Data From Initial attempt to find path
                    actionMoveAmounts[i] = 0;
                    for (int j = 0; j < movingUnit.moveSpeed; j++)
                    {
                        path.RemoveAt(path.Count - 1);
                    }
                    x = startx; 
                    y = starty;

                    //Try to find Path
                    for (int j = 0; j < movingUnit.moveSpeed; j++)
                    {
                        DijkstraMapNode nextLowestNode = map.GetLowestNearbyNode(x, y);
                        x = nextLowestNode.x;
                        y = nextLowestNode.y;
                        path.Add(new Vector2Int(x, y));
                        actionMoveAmounts[i] = actionMoveAmounts[i] + 1;
                        if (endHex[0] == path[path.Count - 1])
                        {
                            foundEndPosition = true;
                            break;
                        }
                        else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
                        {
                            path.RemoveAt(path.Count - 1);
                            foundEndPosition = false;
                            actionMoveAmounts[i] = actionMoveAmounts[i] - 1;
                            break;
                        }
                    }

                    findMostDirectPath = !foundEndPosition;
                    break;
                }
            }
            // Unable to find path in its rangebracket, redo entire path taking most direct route
            if (findMostDirectPath)
            {
                for (int i = 0; i < amountActionLineIncreased; i++)
                {
                    actionMoveAmounts.RemoveAt(actionMoveAmounts.Count - 1);
                }
                amountActionLineIncreased = initialAmountPathMoveIncreased;
                endHex.Add(new Vector2Int(endX, endY));
                map.SetGoals(endHex);
                map.getGrid().GetXY(startingPosition, out x, out y);
                path.Clear();
                foundEndPosition = false;
                for (int i = 0; i < amountOfPossibleMoves; i++)
                {
                    amountActionLineIncreased += 1;
                    actionMoveAmounts.Add(0);
                    bool inRangeBracket = rangeBrackets[i].Contains(new Vector2Int(endX, endY));
                    int startx = x;
                    int starty = y;

                    // Attempt to find path Avoiding harmful Terrain
                    for (int j = 0; j < movingUnit.moveSpeed; j++)
                    {
                        DijkstraMapNode nextLowestNode = map.GetLowestNearbyNode(x, y);
                        x = nextLowestNode.x;
                        y = nextLowestNode.y;
                        path.Add(new Vector2Int(nextLowestNode.x, nextLowestNode.y));
                        actionMoveAmounts[i] = actionMoveAmounts[i] + 1;
                        if (endHex[0] == path[path.Count - 1])
                        {
                            foundEndPosition = true;
                            break;
                        }
                        else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
                        {
                            path.RemoveAt(path.Count - 1);
                            foundEndPosition = false;
                            actionMoveAmounts[i] = actionMoveAmounts[i] - 1;
                            break;
                        }
                    }

                    if (foundEndPosition)
                    {
                        break;
                    }
                }
            }
            /*
            oneActionMoveAmount = 0;
            twoActionMoveAmount = 0;
            if (actionPointsLeft >= amountMoved + 1 + amountMoved + 2)
            {
                endHex.Add(new Vector2Int(endX, endY));
                map.SetGoals(endHex);
                map.getGrid().GetXY(startingPosition, out int x, out int y);
                path.Clear();
                bool foundEndPosition = false;
                bool inFirstRangeBracket = false;
                if (rangeBrackets.Count >= 1)
                {
                    inFirstRangeBracket = rangeBrackets[0].Contains(new Vector2Int(endX, endY));
                }

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
            */
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
        // Determine Latest Action Line
        for(int i = actionLines.Count - 1; i >= 0; i--)
        {
            if (actionLines[i].Count != 0)
            {
                IndexOfStartingActionLine = i + 1;
                break;
            } 
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
                OnFoundTarget?.Invoke(null, movingUnit, -1, false);
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
                
                Vector2Int endPathHex = setPath[setPath.Count - 1];

                for(int i = 0; i < rangeBrackets.Count; i++)
                {
                    if (rangeBrackets[i].Contains(endPathHex))
                    {
                        for(int j = 0; j <= i; j++)
                        {
                            actionPointsLeft -= amountMoved + (j + 1);
                        }
                        amountMoved += i + 1;
                    }
                }

                Destroy(tempMovingUnit);
                tempMovingUnit = null;
                tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, 1, movingUnit.unitProfile);
                SetUp(map.getGrid().GetWorldPosition(setPath[setPath.Count - 1]), actionPointsLeft, movingUnit.moveSpeed);
                gameManager.spriteManager.ActivateActionConfirmationMenu(
                    () => // Confirm Action
                    {
                        if(setPath.Count == 1 && movingUnit.gameManager.map.getGrid().GetWorldPosition(setPath[0].x, setPath[0].y) == movingUnit.transform.position)
                        {
                            OnFoundTarget?.Invoke(null, movingUnit, -1, false);
                            gameManager.playerTurn.SelectUnit(endHexPosition);
                        }
                        else
                        {
                            OnFoundTarget?.Invoke(setPath, movingUnit, IndexOfStartingActionLine, true);
                            Destroy(tempMovingUnit);
                        }
                    },
                    () => // Cancel Action
                    {
                        ResetTargeting();
                        SetUp(movingUnit.transform.position, movingUnit.currentActionsPoints, movingUnit.moveSpeed);
                    });
                
            }
        }
    }

    public void ResetTargeting()
    {
        Destroy(tempMovingUnit);
        tempMovingUnit = null;
        setPath = new List<Vector2Int>();
        path = new List<Vector2Int>();
        prevEndHexPosition = new Vector2Int(-1, -1);
        actionMoveAmounts = new List<int>();
        actionLines = new List<List<Vector3>>();
        amountActionLineIncreased = 0;
        IndexOfStartingActionLine = 0;
        amountMoved = 0;
        gameManager.spriteManager.ClearLines();
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
        OnFoundTarget = null;
    }

    public void DrawLine()
    {
        if (actionPointsLeft <= 0)
        {
            return;
        }

        if(IndexOfStartingActionLine > 0)
        {
            for (int i = 0; i < amountActionLineIncreased; i++)
            {
                if(i > IndexOfStartingActionLine)
                {
                    actionLines.RemoveAt(actionLines.Count - 1);
                }
            }
        }

        for (int i = IndexOfStartingActionLine; i < actionLines.Count; i++)
        {
            actionLines[i] = new List<Vector3>();
            gameManager.spriteManager.DrawLine(actionLines[i], i);
        }

            int currentPathindex = 0;
        for(int i = IndexOfStartingActionLine; i < IndexOfStartingActionLine + amountActionLineIncreased; i++)
        { 
            if(actionLines.Count <= i)
            {
                actionLines.Add(new List<Vector3>());
            }

            if (i == 0)
            {
                actionLines[i].Add(movingUnit.transform.position);
            }
            else
            {
                actionLines[i].Add(actionLines[i - 1][actionLines[i - 1].Count - 1]);
            }

            for(int j = 0; j < actionMoveAmounts[i - IndexOfStartingActionLine]; j++)
            {
                actionLines[i].Add(map.getGrid().GetWorldPosition(path[currentPathindex].x, path[currentPathindex].y));
                currentPathindex += 1;
            }
            gameManager.spriteManager.DrawLine(actionLines[i], i);
        }

        for(int i = 0; i < amountActionLineIncreased; i++)
        {
            actionMoveAmounts.RemoveAt(actionMoveAmounts.Count - 1);
        }
        /*
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
        */
    }
}
