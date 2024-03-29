using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class Move
{
    public static ActionTypes[] movementActions = { ActionTypes.movement};
    public static ActionTypes[] meleeActions = {ActionTypes.attack, ActionTypes.meleeAttack};
    public static void Movement(Unit selfUnit, Vector2 direction, GameManager gameManager, bool isPlayer = true,
        bool isMoveGameManagers = false)
    {
        Vector3 originalPosition = selfUnit.gameObject.transform.position;
        Vector3 newPosition = selfUnit.gameObject.transform.position + (Vector3)direction;
        MainGameManger mainGameManger = gameManager.mainGameManger;
        //Checks to see if player is attempting to leave main play area
        if (isPlayer && (newPosition.x >= mainGameManger.mapWidth * 2 || newPosition.x < mainGameManger.mapWidth
            || newPosition.y >= mainGameManger.mapHeight * 2 || newPosition.y < mainGameManger.mapHeight))
        {
            int x = (int) (newPosition.x / gameManager.mainGameManger.mapWidth);
            int y = (int)(newPosition.y / gameManager.mainGameManger.mapHeight);
            if ((x != 1 || y != 1))
            {
                if(mainGameManger.wallGrid[x, y] != null)
                {
                    // Check if space is open in a tile that already exists
                    if (CanMove(selfUnit, newPosition, direction, mainGameManger.GetGameManger(newPosition)))
                    {
                        mainGameManger.mapManager.AttemptToMoveMapPosition(new Vector2Int(x - 1, y - 1), 
                            new Vector2Int((int) newPosition.x, (int) newPosition.y));
                        return;
                    }
                }
                else
                {
                    mainGameManger.mapManager.AttemptToMoveMapPosition(new Vector2Int(x - 1, y - 1), 
                        new Vector2Int((int)newPosition.x, (int)newPosition.y));
                    return;
                }
            }
        }

        if (isMoveGameManagers && CanMoveMapManagers(selfUnit, newPosition, direction, mainGameManger.GetGameManger(newPosition)))
        {
            if(!IsEnemy(selfUnit, newPosition, mainGameManger.GetGameManger(newPosition)))
            {
                selfUnit.UnitMovement(originalPosition, newPosition, false, false);
                selfUnit.HandlePerformActions(movementActions, ActionName.MoveNorth);
            }
            return;
        }



        if (CanMove(selfUnit, newPosition, direction, gameManager))
        {
            if (!IsEnemy(selfUnit, newPosition, gameManager))
            {
                if (selfUnit.index == 0 && isPlayer)
                {
                    PickupIfItem(selfUnit, newPosition, gameManager);
                }

                selfUnit.UnitMovement(originalPosition, newPosition, false, false);
                selfUnit.HandlePerformActions(movementActions, ActionName.MoveNorth);
            }

        }
    }

    private static void PickupIfItem(Unit selfUnit, Vector3 newPosition, GameManager gameManager)
    {
        List<Item> tempItemList = gameManager.itemgrid.GetGridObject(newPosition);
        if(tempItemList != null)
        {
            for(int i = 0; i < tempItemList.Count; i++)
            {
                selfUnit.gameObject.GetComponent<PickupSystem>().Pickup(tempItemList[i], i);
            }
        }
    }

    public static bool CanMove(Unit selfUnit, Vector3 newPosition, Vector2 direction, GameManager gameManager)
    {
        Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(newPosition);
        if (!gameManager.groundTilemap.HasTile(gridPosition) || (gameManager.obstacleGrid.GetGridObject(newPosition) != null && 
            gameManager.obstacleGrid.GetGridObject(newPosition).blockMovement == true))
        {
            return false;
        }
        return true;
    }

    public static bool CanMoveMapManagers(Unit selfUnit, Vector3 newPosition, Vector2 direction, GameManager gameManager)
    {
        if ((gameManager.obstacleGrid.GetGridObject(newPosition) != null && 
            gameManager.obstacleGrid.GetGridObject(newPosition).blockMovement == true))
        {
            return false;
        }
        return true;
    }

    public static bool IsEnemy(Unit selfUnit, Vector3 newPosition, GameManager gameManager)
    {
        Unit target = gameManager.grid.GetGridObject(newPosition);
        if (target != null && selfUnit.faction != target.faction)
        {
            MeleeAttack.Attack(target, selfUnit);
            selfUnit.HandlePerformActions(meleeActions, ActionName.MeleeAttack);
            return true;
        }
        return false;
    }
}
