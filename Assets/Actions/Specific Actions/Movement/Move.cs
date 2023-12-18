using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class Move
{
    public static ActionTypes[] momvementActions = { ActionTypes.movement};
    public static ActionTypes[] meleeActions = {ActionTypes.attack, ActionTypes.meleeAttack};
    public static void Movement(Unit target, Vector2 direction, GameManager gameManager, bool isPlayer = true)
    {
        Vector3 originalPosition = target.gameObject.transform.position;
        target.gameObject.transform.position += (Vector3)direction;
        Vector3 newPosition = target.gameObject.transform.position;

        if(target.inMiddleMap && isPlayer)
        {
            int x = (int) (newPosition.x / gameManager.mapWidth);
            int y = (int)(newPosition.y / gameManager.mapWidth);
            if (x != 1 || y != 1)
            {
                
            }
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
                target.CheckForStatusFields(target.gameObject.transform.position);
                gameManager.ChangeUnits(target.gameObject.transform.position, target);
                target.HandlePerformActions(momvementActions, ActionName.MoveNorth);
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

    public static bool IsEnemy(Unit target, Vector3 newPosition, GameManager gameManager)
    {
        Unit unit = gameManager.grid.GetGridObject((int)newPosition.x, (int)newPosition.y);
        if (unit != null)
        {
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
