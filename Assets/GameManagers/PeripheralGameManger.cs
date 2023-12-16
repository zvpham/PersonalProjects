using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;



public class PeripheralGameManager : GameManager
{
    public Vector3 gridPosition;
    private bool aUnitActed = false;
    // during turn 0 = no; 1 = yes
    private int duringTurn = 0;

    public GameManager mainGameManger;

    public void Start()
    {
        mainGameManger = GameManager.instance;
        grid = new Grid<Unit>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, gridPosition, (Grid<Unit> g, int x, int y) => null);
        flyingGrid = new Grid<Unit>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, gridPosition, (Grid<Unit> g, int x, int y) => null);
        itemgrid = new Grid<List<Item>>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, gridPosition, (Grid<List<Item>> g, int x, int y) => null);

        for(int i = 0; i < units.Count; i++)
        {
            units[i].inPeripheralGameManager = true;
        }
    }

    // Update is called once per frame 
    void Update()
    {
        if (CanContinue(units[index]))
        {
            // finds the lowest priority amongst all the units, statuses, worldtimer
            // if we are at the top of a turn
            if (duringTurn == 0)
            {
                least = priority[0];
                for (int i = 0; i < priority.Count; i++)
                {
                    if (priority[i] < least)
                    {
                        least = priority[i];
                    }
                }
                if (allStatuses.Count > 0)
                {
                    for (int i = 0; i < statusPriority.Count; i++)
                    {
                        if (statusPriority[i] < least)
                        {
                            least = statusPriority[i];
                        }
                    }
                }

                if (createdFieldPriority.Count > 0)
                {
                    for (int i = 0; i < createdFieldPriority.Count; i++)
                    {
                        if (createdFieldPriority[i] < least)
                        {
                            least = createdFieldPriority[i];
                        }

                    }
                }

                if (animatedFieldPriority.Count > 0)
                {
                    for (int i = 0; i < animatedFieldPriority.Count; i++)
                    {
                        if (animatedFieldPriority[i] < least)
                        {
                            least = animatedFieldPriority[i];
                        }

                    }
                }
            }

            //lowers priority of a unit by the least amount of priority 
            // if priority is 0 activate unit and end loop
            // if mid turn will resume list one more than unit that just went
            aUnitActed = false;
            for (int i = index + duringTurn; i < priority.Count;)
            {
                priority[i] = priority[i] - least;

                if ((int)priority[i] == 0)
                {
                    index = i;
                    duringTurn = 1;
                    units[i].enabled = true;
                    aUnitActed = true;
                    break;
                }
                else if (i == 0)
                {
                    duringTurn = 1;
                    i++;
                }
                else
                {
                    index = i;
                    i++;
                }
            }


            //end turn reset all turn variables and reset priority of units who acted
            if (index + duringTurn == priority.Count && !aUnitActed)
            {
                index = 0;
                duringTurn = 0;
                for (int i = 0; i < priority.Count; i++)
                {
                    if (priority[i] <= 0)
                    {
                        priority[i] = (int)(baseTurnTime * speeds[i]);
                    }
                }

                if (allStatuses.Count > 0)
                {
                    numberOfStatusRemoved = 0;
                    for (int i = 0; i < statusPriority.Count; i++)
                    {
                        if (i < 0)
                        {
                            break;
                        }
                        if (allStatuses[i].ApplyEveryTurn && allStatuses[i].isFirstWorldTurn)
                        {
                            allStatuses[i].isFirstWorldTurn = false;
                        }
                        else
                        {
                            statusPriority[i] -= least;
                            if (statusPriority[i] <= 0)
                            {
                                statusPriority[i] = (int)(allStatuses[i].statusQuickness * baseTurnTime);
                                Unit tempUnit = allStatuses[i].targetUnit;
                                int tempIndex = tempUnit.statusDuration.Count;

                                //reduces status duration of a status if it is supposed to go down at the end of a turn
                                if (!allStatuses[i].nonStandardDuration)
                                {
                                    statusDuration[i] -= 1;
                                }

                                // if an affect applyies everyturn apply the affect if it isn't the  turn it was activated

                                if (allStatuses[i].ApplyEveryTurn)
                                {
                                    allStatuses[i].ApplyEffect(tempUnit);
                                }

                                //if a status was removed in previous step reset sprite of unit otherwise if a status duration is 0 remove the status from the unit and reset the units sprite
                                if (tempUnit.statusDuration.Count != tempIndex || statusDuration[i] <= 0)
                                {
                                    if (tempUnit.statusDuration.Count == tempIndex)
                                    {
                                        allStatuses[i].RemoveEffect(tempUnit);
                                    }
                                    tempUnit.ChangeSprite(tempUnit.originalSprite);
                                    tempUnit.spriteIndex = -1;

                                }
                                i -= numberOfStatusRemoved;
                                numberOfStatusRemoved = 0;
                            }
                        }
                    }
                }

                // change Status Priority at top of turn
                if (createdFieldPriority.Count > 0)
                {
                    for (int i = 0; i < createdFieldPriority.Count; i++)
                    {
                        createdFieldPriority[i] -= least;
                        if (createdFieldPriority[i] <= 0)
                        {
                            createdFieldPriority[i] = (int)(createdFields[i].createdFieldQuickness * baseTurnTime);

                            //reduces status duration of a status if it is supposed to go down at the end of a turn
                            if (!createdFields[i].nonStandardDuration)
                            {
                                createdFieldDuration[i] -= 1;
                            }

                            //if a status was removed in previous step reset sprite of unit otherwise if a status duration is 0 remove the status from the unit and reset the units sprite
                            if (createdFieldDuration[i] <= 0)
                            {
                                createdFields[i].RemoveStatusOnDeletion();
                            }
                        }
                    }
                }

                if (animatedFieldPriority.Count > 0)
                {
                    for (int i = 0; i < animatedFieldPriority.Count; i++)
                    {
                        animatedFieldPriority[i] -= least;
                        if (animatedFieldPriority[i] <= 0)
                        {
                            animatedFieldPriority[i] = (int)(animatedFields[i].createdFieldQuickness * baseTurnTime);

                            animatedFields[i].Activate();
                            //Animated Fields Handle thier Own Deletion
                        }
                    }
                }
            }
        }       
    }


    private bool CanContinue(MonoBehaviour script)
    {
        return !script.isActiveAndEnabled;
    }
}