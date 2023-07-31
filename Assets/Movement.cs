/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    // Update is called once per frame
    public void Move(Vector2 direction)
    {
        if (CanMove(direction))
        {
            if (IsEnemy(direction))
            {

            }
            else
            {
                transform.position += (Vector3)direction;
                gameManager.locations[0] = transform.position;
            }
        }
    }

    public bool CanMove(Vector2 direction)
    {
        Vector3Int gridPosition = groundTilemap.WorldToCell(transform.position + (Vector3)direction);
        if (!groundTilemap.HasTile(gridPosition) || collisionTilemap.HasTile(gridPosition))
        {
            return false;
        }
        return true;
    }

    public bool IsEnemy(Vector2 direction)
    {

        for (int i = 0; i < gameManager.locations.Count; i++)
        {
            if (i == 0)
            {
                continue;
            }
            if (gameManager.locations[i] == transform.position + (Vector3)direction)
            {
                //MeleeAttack.Attack(gameManager.scripts[i], toHitBonus);
                return true;
            }
        }

        return false;
    }
}
*/