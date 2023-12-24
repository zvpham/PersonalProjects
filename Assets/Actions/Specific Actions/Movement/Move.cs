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
    public static void Movement(Unit target, Vector2 direction, GameManager gameManager, bool isPlayer = true,
        bool isMoveGameManagers = false)
    {
        Vector3 originalPosition = target.gameObject.transform.position;
        target.gameObject.transform.position += (Vector3)direction;
        Vector3 newPosition = target.gameObject.transform.position;
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
                    if (CanMove(target, newPosition, direction, mainGameManger.GetGameManger(newPosition)))
                    {
                        mainGameManger.mapManager.AttemptToMoveMapPosition(new Vector2Int(x - 1, y - 1), 
                            new Vector2Int((int) newPosition.x, (int) newPosition.y));
                        target.gameObject.transform.position -= (Vector3)direction;
                        return;
                    }
                }
                else
                {
                    mainGameManger.mapManager.AttemptToMoveMapPosition(new Vector2Int(x - 1, y - 1), 
                        new Vector2Int((int)newPosition.x, (int)newPosition.y));
                    target.gameObject.transform.position -= (Vector3)direction;
                    return;
                }
            }
        }

        if (isMoveGameManagers && CanMoveMapManagers(target, newPosition, direction, mainGameManger.GetGameManger(newPosition)))
        {
            if(IsEnemy(target, newPosition, mainGameManger.GetGameManger(newPosition)))
            {
                target.gameObject.transform.position -= (Vector3)direction;
            }
            else
            {
                gameManager.ChangeUnits(originalPosition, null);
                target.CheckForStatusFields(target.gameObject.transform.position, mainGameManger.GetGameManger(newPosition));
                mainGameManger.GetGameManger(newPosition).ChangeUnits(newPosition, target);
                target.HandlePerformActions(movementActions, ActionName.MoveNorth);
            }
            return;
        }



        if (CanMove(target, target.gameObject.transform.position, direction, gameManager))
        {
            if (IsEnemy(target, target.gameObject.transform.position, gameManager))
            {
                target.gameObject.transform.position -= (Vector3)direction;
            }
            else
            {
                if(target.index == 0 && isPlayer)
                {
                    PickupIfItem(target, target.gameObject.transform.position, gameManager);
                }
                //gameManager.locations[0] = target.gameObject.transform.position;
                gameManager.ChangeUnits(originalPosition, null);
                target.CheckForStatusFields(target.gameObject.transform.position, gameManager);
                gameManager.ChangeUnits(target.gameObject.transform.position, target);
                target.HandlePerformActions(movementActions, ActionName.MoveNorth);
            }
        }
    }

    private static void PickupIfItem(Unit target, Vector3 newPosition, GameManager gameManager)
    {
        List<Item> tempItemList = gameManager.itemgrid.GetGridObject(newPosition);
        if(tempItemList != null)
        {
            for(int i = 0; i < tempItemList.Count; i++)
            {
                target.gameObject.GetComponent<PickupSystem>().Pickup(tempItemList[i], i);
            }
        }
    }

    public static bool CanMove(Unit target, Vector3 newPosition, Vector2 direction, GameManager gameManager)
    {
        Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(newPosition);
        if (!gameManager.groundTilemap.HasTile(gridPosition) || (gameManager.obstacleGrid.GetGridObject(newPosition) != null && gameManager.obstacleGrid.GetGridObject(newPosition).blockMovement == true))
        {
            target.gameObject.transform.position -= (Vector3)direction;
            return false;
        }
        return true;
    }

    public static bool CanMoveMapManagers(Unit target, Vector3 newPosition, Vector2 direction, GameManager gameManager)
    {
        if ((gameManager.obstacleGrid.GetGridObject(newPosition) != null && 
            gameManager.obstacleGrid.GetGridObject(newPosition).blockMovement == true))
        {
            target.gameObject.transform.position -= (Vector3)direction;
            return false;
        }
        return true;
    }

    public static bool IsEnemy(Unit target, Vector3 newPosition, GameManager gameManager)
    {
        Unit unit = gameManager.grid.GetGridObject(newPosition);
        if (unit != null)
        {
            Debug.Log(unit);
            MeleeAttack.Attack(unit, target.toHitBonus, target.armorPenetration, target.strengthMod + 3);
            target.HandlePerformActions(meleeActions, ActionName.MeleeAttack);
            return true;
        }
        return false;
    }

    public static void ChangeScreens()
    {

    }
}
