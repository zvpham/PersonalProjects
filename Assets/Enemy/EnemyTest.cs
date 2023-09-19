using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class EnemyTest : Unit
{
    private Vector2 newPosition = new Vector2(0.0f, 0.0f);
    public bool ableToWander = true;

    public int highestPriorityActionIndex;

    public int currentActionWeight;
    public int highestActionWeight = 0;
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
        gameManager.grid.SetGridObject(self.transform.position, this);
        enabled = false;
        
    }

    private void OnEnable()
    {
        if(gameManager != null)
        {
            highestActionWeight = 0;
            highestPriorityActionIndex = -1;
            DetermineAllyOrEnemy();

            if (enemyList.Count == 0)
            {
                if (ableToWander && !unusableActionTypes.Keys.Contains(ActionTypes.movement))
                {
                    Wander();
                }
                TurnEnd();
            }
            else
            {
                IsInMelee();
                FindClosestEnemy();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i].currentCooldown == 0 && !ContainsMatchingUnusableActionType(i, false))
            {
                currentActionWeight = actions[i].CalculateWeight(this);
            }
            else
            {
                currentActionWeight = 0;
            }

            if(currentActionWeight > highestActionWeight)
            {
                highestPriorityActionIndex = i;
                highestActionWeight = currentActionWeight;
            }
        }
        
        if(highestPriorityActionIndex != -1)
        {
            actions[highestPriorityActionIndex].StartActionPresetAI(this);
            actions[highestPriorityActionIndex].Activate(this);
            highestActionWeight = 0;
            highestPriorityActionIndex = -1;
        }
        else
        {
            for (int i = 0; i < baseActions.Count; i++)
            {
                if (baseActions[i].currentCooldown == 0 && !ContainsMatchingUnusableActionType(i, false))
                {
                    currentActionWeight = baseActions[i].CalculateWeight(this);
                }
                else
                {
                    currentActionWeight = 0;
                }

                if (currentActionWeight > highestActionWeight)
                {
                    highestPriorityActionIndex = i;
                    highestActionWeight = currentActionWeight;
                }
            }

            if (highestPriorityActionIndex != -1)
            {
                baseActions[highestPriorityActionIndex].StartActionPresetAI(this);
                baseActions[highestPriorityActionIndex].Activate(this);
                highestActionWeight = 0;
                highestPriorityActionIndex = -1;
            }
            TurnEnd();
        }
    }

    public void Wander()
    {
        List<Vector3> possiblePositions = new List<Vector3>();
        Vector3Int gridPosition;
        Unit unit;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                gridPosition = gameManager.groundTilemap.WorldToCell(gameObject.transform.position + new Vector3(j, i, 0));
                unit = gameManager.grid.GetGridObject((int)gameObject.transform.position.x + j, (int)gameObject.transform.position.y + i);

                if (!(!gameManager.groundTilemap.HasTile(gridPosition) || gameManager.collisionTilemap.HasTile(gridPosition) || unit != null))
                {
                    possiblePositions.Add(new Vector3(j, i, 0));
                }
            }
        }

        if(possiblePositions.Count > 0)
        {
            Vector2 newPosition = (Vector2) possiblePositions[Random.Range(0, possiblePositions.Count)];
            Move.Movement(this, newPosition, gameManager, false);
            TurnEnd();
        }
        TurnEnd();
    }
}