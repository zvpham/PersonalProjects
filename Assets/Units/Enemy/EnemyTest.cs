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
    public bool ableToWander = true;
    public bool possibleToTransferGameMangers = false;

    public int highestPriorityActionIndex;

    public int currentActionWeight;
    public int highestActionWeight = 0;

    public int currentInterestLevel;
    public int maxInterestLevel = 10;

    // Start is called before the first frame update
    void Start()
    {
        ChangeStr(0);
        ChangeAgi(0);
        ChangeEnd(0);
        ChangeWis(0);
        ChangeInt(0);
        ChangeCha(0);

        baseActionTemplate = Instantiate(baseActionTemplate);
        foreach (Action templateAction in baseActionTemplate.Actions)
        {
            baseActions.Add(Instantiate(templateAction));
        }

        UpdateActions();

        originalSprite = GetComponent<SpriteRenderer>().sprite;
        //gameManager = GameManager.instance;
        gameManager.ChangeUnits(gameObject.transform.position, this);
        if (gameManager.isNewSlate)
        {
            gameManager.speeds.Add(this.quickness);
            gameManager.priority.Add((int)(this.quickness * gameManager.baseTurnTime));
            gameManager.units.Add(this);
        }
        //gameManger will be set  to instance on the onload function in GameManagerScript
        else
        {
            for(int i = 0; i < actions.Count; i++)
            {
                int index = actionNamesForCoolDownOnLoad.IndexOf(actions[i].actionName);
                if (index == -1)
                {
                    actions.RemoveAt(i);
                    i--;
                    continue;
                }
                actions[i].currentCooldown = currentCooldownOnLoad[index];
            }
        }
        enabled = false;
        
    }

    private void OnEnable()
    {
        OnTurnStart();
        chasing = false;
        possibleToTransferGameMangers = false;
        if (gameManager != null)
        {
            highestActionWeight = 0;
            highestPriorityActionIndex = -1;
            UseSenses(false);
            if (inPeripheralGameManager)
            {
                if (enemyList.Count == 0)
                {
                    UseSenses(true);
                    if (enemyList.Count == 0)
                    {
                        if (lastKnownEnemyLocation != null)
                        {
                            chasing = true;
                            locationUnitIsChasing = lastKnownEnemyLocation;
                        }
                        else
                        {
                            Wander();
                            TurnEnd();
                        }
                    }
                    // enemy was found in a different Tile/Map
                    else
                    {
                        chasing = true;
                        FindClosestEnemy();
                        locationUnitIsChasing = enemyList[closestEnemyIndex].transform.position;
                        possibleToTransferGameMangers = true;
                    }
                }
                else
                {
                    IsInMelee();
                    FindClosestEnemy();
                }
            }
            else
            {
                if (enemyList.Count == 0)
                {
                    if (lastKnownEnemyLocation != null)
                    {
                        chasing = true;
                        locationUnitIsChasing = lastKnownEnemyLocation;
                    }
                    else if (ableToWander && !unusableActionTypes.Keys.Contains(ActionTypes.movement))
                    {
                        Wander();
                        TurnEnd();
                    }
                }
                else
                {
                    IsInMelee();
                    FindClosestEnemy();
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (notOnHold)
        {
            if (chasing)
            {
                for (int i = 0; i < chaseActions.Count; i++)
                {
                    if (chaseActions[i].currentCooldown == 0 && !ContainsMatchingUnusableActionType(i, false))
                    {
                        currentActionWeight = chaseActions[i].CalculateWeight(this, locationUnitIsChasing);
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
                    chaseActions[highestPriorityActionIndex].StartActionPresetAI(this);
                    chaseActions[highestPriorityActionIndex].Activate(this, locationUnitIsChasing);
                    highestActionWeight = 0;
                    highestPriorityActionIndex = -1;

                }
                else
                {
                    TurnEnd();
                }
                if (possibleToTransferGameMangers)
                {
                    gameManager.mainGameManger.TransferGameManagers(transform.position, this); 
                }
            }
            else
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

                    if (currentActionWeight > highestActionWeight)
                    {
                        highestPriorityActionIndex = i;
                        highestActionWeight = currentActionWeight;
                    }
                }

                if (highestPriorityActionIndex != -1)
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
                Vector3 position = gameObject.transform.position + new Vector3(j, i, 0);
                gridPosition = gameManager.groundTilemap.WorldToCell(gameObject.transform.position + new Vector3(j, i, 0));
                unit = gameManager.grid.GetGridObject((int)gameObject.transform.position.x + j, (int)gameObject.transform.position.y + i);

                if (!(!gameManager.groundTilemap.HasTile(gridPosition) || gameManager.obstacleGrid.GetGridObject(position) != null || unit != null))
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