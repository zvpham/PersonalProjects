using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.GraphicsBuffer;

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
        //gameManager.locations.Add(transform.position);
        gameManager.grid.SetGridObject(self.transform.position, this);

        index = gameManager.speeds.Count;
        Debug.Log("Player Start");

        enabled = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (turn1)
        {
            if(actions.Count > 0)
            {
                if (actions[0].startActionPresets() )
                {
                    actions[0].Activate(this);
                    turn1 = false;
                    TurnEnd();
                }
            }
        }
        else
        {
            newPosition.Set(-1f, 0f);
            Move(newPosition);
            TurnEnd();
        }
        */
        TurnEnd();
    }

    public void Move(Vector2 direction)
    {
        if (CanMove(direction))
        {
            Vector3 originalPosition = transform.position;
            transform.position += (Vector3)direction;
            gameManager.grid.SetGridObject(originalPosition, null);
            gameManager.grid.SetGridObject(transform.position, this);
            //gameManager.locations[index] = transform.position;
        }
    }

    public bool CanMove(Vector2 direction)
    {
        Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(transform.position + (Vector3)direction);
        if (!gameManager.groundTilemap.HasTile(gridPosition) || gameManager.collisionTilemap.HasTile(gridPosition))
        {
            return false;
        }
        return true;
    }
}