using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MeleeTargeting : TargetingSystem
{
    public DijkstraMap map;
    public CombatGameManager gameManager;
    public Unit movingUnit;
    public Vector3 startingPosition;
    public Vector3 endPosition;
    public Vector2Int prevEndHexPosition;       
    public List<Vector2Int> path = new List<Vector2Int>();
    public List<Vector2Int> highlightedHexes = new List<Vector2Int>();
    public List<Vector2Int> highlightedPositions = new List<Vector2Int>();
    public GameObject tempMovingUnit;
    public int oneActionMoveAmount;
    public int meleeRange;

    public bool selectedTarget = false;

    public bool targetFriendly = false;
    public bool selectedEndPositionNoUnit = false;

    public int actionPointsLeft;

    public UnityAction<List<Vector2Int>, Unit, bool> OnFoundTarget;

    public void SetParameters(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int meleeRange)
    {
        this.targetFriendly = targetFriendly;
        this.movingUnit = movingUnit;
        this.gameManager = movingUnit.gameManager;  
        this.actionPointsLeft = actionPointsLeft;
        this.meleeRange = meleeRange;
        map = movingUnit.gameManager.map;
        startingPosition = movingUnit.transform.position;
        selectedTarget = false;
        selectedEndPositionNoUnit = false;
        prevEndHexPosition = new Vector2Int(-10, 0);
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

                    Unit targetUnit = gameManager.grid.GetGridObject(currentNodePosition.x, currentNodePosition.y).unit;
                    if (targetUnit != null && targetUnit.team != movingUnit.team && map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= moveSpeed + 1)
                    {
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 5);
                        highlightedPositions.Add(currentNodePosition);
                    }
                }
            }
        }
        else if (numActionPoints == 1)
        {
            List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(x, y, 1);
            for (int j = 0; j < mapNodes.Count; j++)
            {
                Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                Unit targetUnit = gameManager.grid.GetGridObject(currentNodePosition.x, currentNodePosition.y).unit;
                if (targetUnit != null && targetUnit.team != movingUnit.team && map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= moveSpeed + 1)
                {
                    gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 5);
                    highlightedPositions.Add(currentNodePosition);
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
        if (!selectedTarget)
        {
            endPosition = newPosition;
            map.ResetMap();
            List<Vector2Int> endHex = new List<Vector2Int>();
            map.getGrid().GetXY(newPosition, out int endX, out int endY);
            if (map.getGrid().GetGridObject(endX, endY) != null)
            {
                if (selectedEndPositionNoUnit)
                {
                        
                }
                else if (actionPointsLeft == 2 )
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
                else if (actionPointsLeft == 1)
                {
                    endHex.Add(new Vector2Int(endX, endY));
                    map.SetGoals(endHex);
                    map.getGrid().GetXY(movingUnit.transform.position, out int x, out int y);
                    path.Clear();
                    oneActionMoveAmount = 0;
                    map.getGrid().GetXY(movingUnit.transform.position, out x, out y);
                    path.Add(new Vector2Int(x, y));
                }
                DrawLine();
            }
        }
    }
    public override void EndMovementTargeting()
    {
        gameManager.grid.GetXY(endPosition, out int x, out int y);

        if (x >= gameManager.grid.GetWidth() || x < 0 || y >= gameManager.grid.GetHeight() || y < 0)
        {
            return;
        }

        Vector2Int endHexPosition = new Vector2Int(x, y);

        bool targetInRange =  false;
        for(int i = 0; i < path.Count; i++)
        {
            if(endHexPosition == path[i])
            {
                targetInRange = true;
                break;
            }
        }

        if (prevEndHexPosition.x >= 0)
        {
            if(endHexPosition == prevEndHexPosition)
            {
                gameManager.spriteManager.ConfirmAction();
            }
            else
            {
                prevEndHexPosition = new Vector2Int(-1, -1);
                Vector2Int endPathHex = path[path.Count - 1];
                Unit targetUnit = gameManager.grid.GetGridObject(endPathHex.x, endPathHex.y).unit;

                if (targetUnit != null && targetUnit.team != Team.Player && targetInRange)
                {
                    tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, movingUnit.GetComponent<SpriteRenderer>().sprite);
                    gameManager.spriteManager.ActivateActionConfirmationMenu(
                        () => // Confirm Action
                        {
                            OnFoundTarget?.Invoke(path, movingUnit, true);
                            Destroy(tempMovingUnit);
                        },
                        () => // Cancel Action
                        {
                            ResetTargeting();
                        });
                }
            }
        }
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
                prevEndHexPosition =  endHexPosition;
                Vector2Int endPathHex = path[path.Count - 1];
                Unit targetUnit = gameManager.grid.GetGridObject(endPathHex.x, endPathHex.y).unit;
                //Target Unit
                if (targetUnit != null && targetUnit.team != Team.Player && targetInRange)
                {
                    tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, movingUnit.GetComponent<SpriteRenderer>().sprite);
                    gameManager.spriteManager.ActivateActionConfirmationMenu(
                        () => // Confirm Action
                        {
                            selectedTarget = true;
                            OnFoundTarget?.Invoke(path, movingUnit, true);
                            Destroy(tempMovingUnit);
                        },
                        () => // Cancel Action
                        {
                            ResetTargeting();
                        });
                }
                else if(targetUnit == null)
                {
                    selectedEndPositionNoUnit = true;
                    tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, movingUnit.GetComponent<SpriteRenderer>().sprite);
                    gameManager.spriteManager.ActivateActionConfirmationMenu(
                        () => // Confirm Action
                        {
                            gameManager.move.AnotherActionMove(path, movingUnit);
                            Destroy(tempMovingUnit);
                        },
                        () => // Cancel Action
                        {
                            ResetTargeting();
                        });
                }
            }
        }
    }

    public void ResetTargeting()
    {
        Destroy(tempMovingUnit);
        tempMovingUnit = null;
        selectedEndPositionNoUnit = false;
        selectedTarget = false;
        prevEndHexPosition = new Vector2Int(-1, -1);
    }

    public override void DeactivateTargetingSystem()
    {
        for(int i = 0; i < highlightedHexes.Count; i++)
        {
            gameManager.spriteManager.ChangeTile(highlightedHexes[i], 2, 0);
        }

        ResetSetUp();
        ResetTargeting();
        map.ResetMap(true);
        gameManager.spriteManager.ClearLines();
        path = new List<Vector2Int>();
        OnFoundTarget = null;
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

        for (int i = 0; i < highlightedHexes.Count; i++)
        {
            gameManager.spriteManager.ChangeTile(highlightedHexes[i], 2, 0);
        }

        // Draw meleeRange
        Vector2Int endHex = path[path.Count - 1];
        highlightedHexes.Clear();
        for (int i = 1; i <= meleeRange; i++)
        {
            List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(endHex.x, endHex.y, i);
            for (int j = 0; j < mapNodes.Count; j++)
            {
                Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                gameManager.spriteManager.ChangeTile(currentNodePosition, 2, 3);
                highlightedHexes.Add(currentNodePosition);
            }
        }

        if (!selectedEndPositionNoUnit)
        {
            gameManager.spriteManager.DrawLine(oneActionLine, null);
        }
    }
}
