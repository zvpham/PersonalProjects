using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class MovementTargeting : TargetingSystem
{
    public DijkstraMap map;
    public CombatGameManager gameManager;
    public Unit movingUnit;
    public Vector2 startingPosition;
    public Vector3 endPosition;
    public List<Vector2Int> path =  new List<Vector2Int>();
    public int oneActionMoveAmount;
    public int twoActionMoveAmount;

    public bool targetFriendly = false;
    public int actionPointsLeft;

    public UnityAction<List<Vector2Int>, Unit, bool> OnFoundTarget;

    public void SetParameters(Vector2 startPosition, Unit movingUnit, bool targetFriendly, int actionPointsLeft)
    {
        this.targetFriendly = targetFriendly;
        this.movingUnit = movingUnit;
        this.gameManager = movingUnit.gameManager;
        this.actionPointsLeft = actionPointsLeft;
        map = movingUnit.gameManager.map;
        startingPosition = startPosition;
        path = new List<Vector2Int>();
        this.enabled = true;
    }

    public override void SelectNewPosition(Vector3 newPosition)
    {
        endPosition = newPosition;
        map.ResetMap();
        List<Vector2Int> endHex = new List<Vector2Int>();
        map.getGrid().GetXY(newPosition, out int endX, out int endY);
        if (map.getGrid().GetGridObject(endX, endY) != null)
        {
            endHex.Add(new Vector2Int(endX, endY));
            map.SetGoals(endHex);
            map.getGrid().GetXY(movingUnit.transform.position, out int x, out int y);
            path.Clear();

            if (actionPointsLeft == 2)
            {
                oneActionMoveAmount = 0;

                bool foundEndPosition = false;
                bool endPositionInFirstRange = false;
                if (movingUnit.firstRangeBracket.Contains(new Vector2Int(endX, endY)))
                {
                    endPositionInFirstRange = true;
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

                if (endPositionInFirstRange && !foundEndPosition)
                {
                    map.ResetMap();
                    map.SetGoals(endHex);
                    map.getGrid().GetXY(movingUnit.transform.position, out x, out y);
                    path.Clear();
                    oneActionMoveAmount = 0;

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
                }

                twoActionMoveAmount = 0;
                if (!foundEndPosition)
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

                    if (!foundEndPosition && movingUnit.secondRangeBracket.Contains(new Vector2Int(endX, endY)))
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
            else if (actionPointsLeft == 1)
            {
                bool foundEndPosition = false;
                oneActionMoveAmount = 0;
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
                        foundEndPosition = true;
                        break;
                    }
                }

                if (!foundEndPosition && movingUnit.firstRangeBracket.Contains(new Vector2Int(endX, endY)))
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
            DrawLine();
        }
    }

    public override void EndMovementTargeting()
    {
        gameManager.grid.GetXY(endPosition, out int x, out int y);
        Vector2Int endHexPosition = new Vector2Int(x, y);
        if (!targetFriendly && gameManager.playerTurn.CheckSpaceForFriendlyUnit(endHexPosition))
        {
            OnFoundTarget?.Invoke(null, movingUnit, false);
            gameManager.playerTurn.SelectUnit(endHexPosition);
        }
        else
        {
            OnFoundTarget?.Invoke(path, movingUnit, true);
        }
    }

    public override void DeactivateTargetingSystem()
    {
        gameManager.spriteManager.ClearLines();
        path = new List<Vector2Int>();
        OnFoundTarget = null;
    }

    public void DrawLine()
    {
        if(actionPointsLeft <= 0)
        {
            return;
        }
        List<Vector3> oneActionLine = new List<Vector3>() { movingUnit.transform.position };
        for (int i = 0; i < oneActionMoveAmount; i++)
        {
            oneActionLine.Add(map.getGrid().GetWorldPosition(path[i].x, path[i].y));
        }

        List<Vector3> twoActionLine = new List<Vector3>() { oneActionLine[oneActionLine.Count - 1] };
        for (int i = 0; i < twoActionMoveAmount; i++)
        {
            twoActionLine.Add(map.getGrid().GetWorldPosition(path[i + oneActionMoveAmount].x, path[i + oneActionMoveAmount].y));
        }
        gameManager.spriteManager.DrawLine(oneActionLine, twoActionLine);
    }
}
