using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class MeleeTargeting : TargetingSystem
{
    public DijkstraMap map;
    public CombatGameManager gameManager;
    public Unit movingUnit;
    public Vector2 startingPosition;
    public Vector3 endPosition;
    public Vector2Int prevEndHexPosition;       
    public List<Vector2Int> path = new List<Vector2Int>();
    public List<Vector2Int> highlightedHexes = new List<Vector2Int>();
    public GameObject tempMovingUnit;
    public int oneActionMoveAmount;
    public int meleeRange;

    public bool targetFriendly = false;
    public bool selectedEndPositionNoUnit = false;

    public int actionPointsLeft;

    public UnityAction<List<Vector2Int>, Unit, bool> OnFoundTarget;

    public void SetParameters(Vector2 startPosition, Unit movingUnit, bool targetFriendly, int actionPointsLeft, int meleeRange)
    {
        this.targetFriendly = targetFriendly;
        this.movingUnit = movingUnit;
        this.gameManager = movingUnit.gameManager;
        this.actionPointsLeft = actionPointsLeft;
        this.meleeRange = meleeRange;
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
                oneActionMoveAmount = 0;
                map.getGrid().GetXY(movingUnit.transform.position, out x, out y);
                path.Add(new Vector2Int(x, y));
            }
            DrawLine();
        }
    }
    public override void EndMovementTargeting()
    {
        gameManager.grid.GetXY(endPosition, out int x, out int y);
        Vector2Int endHexPosition = new Vector2Int(x, y);

        if (prevEndHexPosition != null)
        {
            if(endHexPosition == prevEndHexPosition)
            {
                gameManager.spriteManager.ConfirmAction();
            }
            else
            {
                gameManager.spriteManager.CancelAction();
            }
        }
        else
        {

            if (!targetFriendly && gameManager.playerTurn.CheckSpaceForFriendlyUnit(endHexPosition))
            {
                OnFoundTarget?.Invoke(null, movingUnit, false);
                gameManager.playerTurn.SelectUnit(endHexPosition);
            }
            else
            {
                Vector2Int endPathHex = path[path.Count - 1];
                Unit targetUnit = gameManager.grid.GetGridObject(endPathHex.x, endPathHex.y).unit;

                if (targetUnit != null && targetUnit.team != Team.Player)
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
                            Destroy(tempMovingUnit);
                        });
                }
                else if(targetUnit == null)
                {
                    selectedEndPositionNoUnit = true;

                }
            }
        }
    }

    public override void DeactivateTargetingSystem()
    {
        for(int i = 0; i < highlightedHexes.Count; i++)
        {
            gameManager.spriteManager.ChangeTile(highlightedHexes[i], 2, 0);
        }

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

        gameManager.spriteManager.DrawLine(oneActionLine, null);
    }
}
