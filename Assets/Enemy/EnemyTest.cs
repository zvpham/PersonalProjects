using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyTest : Unit
{
    private Vector2 newPosition = new Vector2(0.0f, 0.0f);
    public bool turn1 = true;
    //private GameManager gameManager;

   // public int index;

    // Start is called before the first frame update
    void Start()
    {

        ChangeStr(0);
        ChangeAgi(0);
        ChangeEnd(0);
        ChangeWis(0);
        ChangeInt(0);
        ChangeCha(0);

        foreach (SoulItemSO physicalSoul in physicalSouls)
        {
            if (physicalSoul != null)
            {
                physicalSoul.AddPhysicalSoul(this);
            }
        }

        foreach (SoulItemSO mentalSoul in physicalSouls)
        {
            if (mentalSoul != null)
            {
                mentalSoul.AddMentalSoul(this);
            }
        }

        originalSprite = GetComponent<SpriteRenderer>().sprite;

        gameManager = GameManager.instance;
        gameManager.speeds.Add(this.quickness);
        gameManager.priority.Add((int)(this.quickness * gameManager.baseTurnTime));
        gameManager.scripts.Add(this);
        gameManager.enemies.Add(this);
        gameManager.locations.Add(transform.position);

        collisionTilemap = Obstacles.instance.collisionTilemap;
        groundTilemap = Ground.instance.groundTilemap;

        index = gameManager.speeds.Count;
        Debug.Log("Player Start");

        enabled = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (turn1)
        {
            if(actions.Count > 0)
                actions[0].Activate(this);
            turn1 = false;  
            TurnEnd();
        }
        else
        {
            newPosition.Set(-1f, 0f);
            Move(newPosition);
            TurnEnd();
        }
    }

    public void Move(Vector2 direction)
    {
        if (CanMove(direction))
        {
            transform.position += (Vector3)direction;
            gameManager.locations[index] = transform.position;
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
}