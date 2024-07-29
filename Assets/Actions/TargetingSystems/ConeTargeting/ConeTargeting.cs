using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConeTargeting : TargetingSystem
{
    public DijkstraMap map;
    public CombatGameManager gameManager;
    public Unit movingUnit;
    public Vector3 startingPosition;
    public Vector3 endPosition;
    public Vector2Int prevEndHexPosition;
    public List<Vector2Int> path = new List<Vector2Int>();
    public List<Vector2Int> coneHexes = new List<Vector2Int>();
    public List<Vector2Int> highlightedHexes = new List<Vector2Int>();
    public List<Vector2Int> highlightedPositions = new List<Vector2Int>();
    public GameObject tempMovingUnit;
    public int oneActionMoveAmount;
    public int meleeRange;
    public int coneRange;

    public bool foundEndHex;
    public bool targetFriendly = false;


    public int actionPointsLeft;

    //path, coneHexes, Moving Unit, ConfirmAction
    public UnityAction<List<Vector2Int>, List<Vector2Int>,  Unit, bool> OnFoundTarget;

    public void SetParameters(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int coneRange, int meleeRange)
    {
        this.targetFriendly = targetFriendly;
        this.movingUnit = movingUnit;
        this.gameManager = movingUnit.gameManager;
        this.actionPointsLeft = actionPointsLeft;
        this.meleeRange = meleeRange;   
        this.coneRange = coneRange;
        map = movingUnit.gameManager.map;
        startingPosition = movingUnit.transform.position;
        prevEndHexPosition = new Vector2Int(-10, 0);
        foundEndHex = false;
        path = new List<Vector2Int>();
        this.enabled = true;

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
        DijkstraMap map = gameManager.map;
        map.getGrid().GetXY(targetPosition, out int x, out int y);
        map.ResetMap();
        map.SetGoals(new List<Vector2Int>() { new Vector2Int(x, y) });
        startingPosition = targetPosition;

        if (numActionPoints == 2)
        {
            for (int i = 1; i <= moveSpeed + 1; i++)
            {
                List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(x, y, i);
                for (int j = 0; j < mapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                    if (map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= moveSpeed)
                    {
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 2);
                        highlightedPositions.Add(currentNodePosition);
                    }
                }
            }
        }
        else if (numActionPoints == 1)
        {
            DrawRange(new Vector2Int(x, y));
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
        if (!foundEndHex && actionPointsLeft == 2)
        {
            endPosition = newPosition;
            map.ResetMap();
            List<Vector2Int> endHex = new List<Vector2Int>();
            map.getGrid().GetXY(newPosition, out int endX, out int endY);
            if (map.getGrid().GetGridObject(endX, endY) != null)
            {
                if (actionPointsLeft == 2)
                {
                    endHex.Add(new Vector2Int(endX, endY));
                    map.SetGoals(endHex);
                    map.getGrid().GetXY(movingUnit.transform.position, out int x, out int y);
                    path.Clear();
                    oneActionMoveAmount = 0;
                    bool foundEndPosition = false;

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

                    if (!foundEndPosition)
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
                                break;
                            }
                        }
                    }
                }
                DrawLine();
            }
       
        }
        else
        {
            map.getGrid().GetXY(startingPosition, out int x, out int y);
            map.getGrid().GetXY(newPosition, out int endX, out int endY);

            Vector3Int startCube = map.getGrid().OffsetToCube(x, y);
            Vector3Int endCube = map.getGrid().OffsetToCube(endX, endY);
            List<Vector3Int> line =  map.getGrid().CubeLineDraw(startCube, endCube);

            if(line.Count > 1)
            {
                Vector3Int nextCube = line[1];
                Vector3Int chosenDirection = nextCube - startCube;
                nextCube += chosenDirection * (coneRange);

                Vector2Int chosenHex = map.getGrid().CubeToOffset(nextCube);
                coneHexes.Clear();
                coneHexes.Add(chosenHex);

                for(int i = 1; i <= coneRange; i++)
                {
                    List<DijkstraMapNode> ringHex = map.getGrid().GetGridObjectsInRing(nextCube, i);
                    for(int j = 0; j < ringHex.Count; j++)
                    {
                        if (ringHex[j] != null)
                        {
                            Vector2Int tempHex = new Vector2Int(ringHex[j].x, ringHex[j].y);
                            coneHexes.Add(tempHex);
                        }
                    }
                }
                DrawCone();
            }
            else
            {
                ResetCone();
            }
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


        if (prevEndHexPosition.x >= 0 && endHexPosition == prevEndHexPosition)
        {
            gameManager.spriteManager.ConfirmAction();
        }
        else if (foundEndHex ||  actionPointsLeft == 1)
        {
            prevEndHexPosition = new Vector2Int(-1, -1);
            Vector2Int endPathHex = path[path.Count - 1];
            tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, 1, movingUnit.GetComponent<SpriteRenderer>().sprite);
            gameManager.spriteManager.ActivateActionConfirmationMenu(
                () => // Confirm Action
                {
                    OnFoundTarget?.Invoke(path, coneHexes, movingUnit, true);
                    Destroy(tempMovingUnit);
                },
                () => // Cancel Action
                {
                    ResetTargeting();
                });
        }
        else
        {
            map.getGrid().GetXY(movingUnit.transform.position, out int tempx, out int tempy);
            Vector2Int movingUnitEndHex = new Vector2Int(tempx, tempy);

            // Select Player Unit when you aren't using a friendly ability like heal
            if (!targetFriendly && endHexPosition != movingUnitEndHex && gameManager.playerTurn.CheckSpaceForFriendlyUnit(endHexPosition))
            {
                OnFoundTarget?.Invoke(null, null, movingUnit, false);
                gameManager.playerTurn.SelectUnit(endHexPosition);
            }
            else
            {
                prevEndHexPosition = endHexPosition;
                foundEndHex = true;
                Vector2Int endPathHex = path[path.Count - 1];
                
                if(movingUnitEndHex != endPathHex)
                {
                    actionPointsLeft -= 1;
                }

                SetUp(map.getGrid().GetWorldPosition(endPathHex), actionPointsLeft, movingUnit.moveSpeed);

                tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, 1, movingUnit.GetComponent<SpriteRenderer>().sprite);
                gameManager.spriteManager.ActivateActionConfirmationMenu(
                    () => // Confirm Action
                    {
                        //gameManager.move.AnotherActionMove(path, movingUnit);
                        Destroy(tempMovingUnit);
                    },
                    () => // Cancel Action
                    {
                        ResetTargeting();
                    });
                
            }
        }
    }

    public void ResetTargeting()
    {
        Destroy(tempMovingUnit);
        tempMovingUnit = null;
        prevEndHexPosition = new Vector2Int(-1, -1);
        foundEndHex = false;
        SetUp(movingUnit.transform.position, movingUnit.currentActionsPoints, movingUnit.moveSpeed);
    }

    public override void DeactivateTargetingSystem()
    {
        for (int i = 0; i < highlightedHexes.Count; i++)
        {
            gameManager.spriteManager.ChangeTile(highlightedHexes[i], 2, 0);
        }

        ResetSetUp();
        ResetTargeting();
        map.ResetMap(true);
        gameManager.spriteManager.ClearLines();
        path = new List<Vector2Int>();
        OnFoundTarget = null;
        oneActionMoveAmount = 0;


        for (int i = 0; i < highlightedHexes.Count; i++)
        {
            gameManager.spriteManager.ChangeTile(highlightedHexes[i], 2, 0);
        }
        highlightedHexes.Clear();
    }

    public void DrawLine()
    {
        if (actionPointsLeft <= 0)
        {
            return;
        }

        List<Vector3> oneActionLine = new List<Vector3>() { movingUnit.transform.position };
        for (int i = 0; i < oneActionMoveAmount; i++)
        {
            oneActionLine.Add(map.getGrid().GetWorldPosition(path[i].x, path[i].y));
        }

        Vector2Int endPositionHex = path[path.Count - 1];
        Unit targetUnit = gameManager.grid.GetGridObject(endPositionHex.x, endPositionHex.y).unit;
        if (targetUnit != null)
        {
            oneActionLine.RemoveAt(oneActionLine.Count - 1);
        }

        // Draw meleeRange
        Vector2Int endHex = path[path.Count - 1];
        DrawRange(endHex);

        gameManager.spriteManager.DrawLine(oneActionLine, null);
    }

    public void DrawRange(Vector2Int targetHex)
    {
        for (int i = 0; i < highlightedHexes.Count; i++)
        {
            gameManager.spriteManager.ChangeTile(highlightedHexes[i], 2, 0);
        }

        highlightedHexes.Clear();
        for (int i = 1; i <= meleeRange; i++)
        {
            List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(targetHex.x, targetHex.y, i);
            for (int j = 0; j < mapNodes.Count; j++)
            {
                Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                gameManager.spriteManager.ChangeTile(currentNodePosition, 2, 3);
                highlightedHexes.Add(currentNodePosition);
            }
        }
    }

    public void DrawCone()
    {
        ResetCone();
        for(int i = 0; i < coneHexes.Count; i++)
        {
            gameManager.spriteManager.ChangeTile(coneHexes[i], 1, 5);
            highlightedPositions.Add(coneHexes[i]);
        }
    }

    public void ResetCone()
    {
        ResetSetUp();
    }
}
