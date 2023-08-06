using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public static void Movement(Unit target, Vector2 direction, GameManager gameManager)
    {
        target.self.transform.position += (Vector3)direction;
        if (CanMove(target, target.self.transform.position, direction, gameManager))
        {
            if (IsEnemy(target, target.self.transform.position, gameManager))
            {
                target.self.transform.position -= (Vector3)direction;
            }
            else
            {
                PickupIfItem(target, target.self.transform.position, gameManager);
                gameManager.locations[0] = target.self.transform.position;
            }
        }
    }

    private static void PickupIfItem(Unit target, Vector3 newPosition, GameManager gameManager)
    {
        for (int i = 0; i < gameManager.itemLocations.Count; i++)
        {
            if (gameManager.itemLocations[i] == newPosition)
            {
                target.self.GetComponent<PickupSystem>().Pickup(gameManager.items[i]);
            }
        }
    }

    public static bool CanMove(Unit target, Vector3 newPosition, Vector2 direction, GameManager gameManager)
    {
        Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(newPosition);
        if (!gameManager.groundTilemap.HasTile(gridPosition) || gameManager.collisionTilemap.HasTile(gridPosition))
        {
            target.self.transform.position -= (Vector3)direction;
            return false;
        }
        return true;
    }

    public static bool IsEnemy(Unit target, Vector3 newPosition, GameManager gameManager)
    {

        for (int i = 0; i < gameManager.locations.Count; i++)
        {
            if (i == 0)
            {
                continue;
            }
            if (gameManager.locations[i] == newPosition)
            {
                Debug.Log(gameManager.scripts[i]);
                MeleeAttack.Attack(gameManager.scripts[i], target.toHitBonus, target.armorPenetration, target.damage);
                return true;
            }
        }

        return false;
    }

}
