using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;



public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public List<double> speeds;
    public List<int> priority;
    public List<int> statusPriority;
    public List<int> statusDuration;

    public List<int> statusObjectPriority;
    public List<int> StatusObjectDuration;

    public int baseTurnTime = 500;
    public int worldPriority = 0;

    public Tilemap groundTilemap;
    public Tilemap collisionTilemap;
    //public List<Vector3> locations = new List<Vector3>();
    public List<Vector3> itemLocations = new List<Vector3>();

    public Grid<Unit> grid;
    public Grid<Unit> flyingGrid;

    public List<CreatedField> StatusFields = new List<CreatedField>();

    public List<Unit> scripts;
    public List<EnemyTest> enemies;
    public List<Item> items;
    public List<Status> allStatuses;

    public List<Sprite> sprites;
    private InputManager inputManager;

    public int least;
    public int index = 0;
    private bool aUnitActed = false;


    public int numberOfStatusRemoved = 0;

    public float secSpriteChangeSpeed;
    private float currentTime = 0;

    public float expectedLocationChangeSpeed;
    public float currentExpectedLoactionChangeSpeed;
    public int isLocationChangeStatus = 0;
    public ExpectedLocationMarker expectedLocationMarker;
    public List<int> expectedLocationChangeList = new List<int>();
    public List<Unit> unitWhoHaveLocationChangeStatus = new List<Unit>();

    public List<List<List<AnimatedField.Node>>> expectedBlastPaths = new List<List<List<AnimatedField.Node>>>();
    public List<int> expectedBlastRowNumber = new List<int>();
    public List<List<GameObject>> expectedBlastMarkers = new List<List<GameObject>>();

    // during turn 0 = no; 1 = yes
    private int duringTurn = 0;


    // Start is called before the first frame update


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        /*
        else if (instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);
        */
        grid = new Grid<Unit>(20, 10, 1f, new Vector3(-0.5f, -0.5f, 0f), (Grid<Unit> g, int x, int y) => null);
        flyingGrid = new Grid<Unit>(20, 10, 1f, new Vector3(-0.5f, -0.5f, 0f), (Grid<Unit> g, int x, int y) => null);
    }

    void Start()
    {
        inputManager = InputManager.instance;
        collisionTilemap = Obstacles.instance.collisionTilemap;
        groundTilemap = Ground.instance.groundTilemap;
        currentExpectedLoactionChangeSpeed = expectedLocationChangeSpeed;
        expectedLocationMarker.selfDestructionTimer = expectedLocationChangeSpeed;
    }
     
    // Update is called once per frame 
    void Update()
    {
        // Adds a phantome image of things that have a set or predicted path
        currentTime += Time.deltaTime;

        if(currentTime >= currentExpectedLoactionChangeSpeed)
        {
            if (isLocationChangeStatus >= 1)
            {
                foreach (Unit unit in scripts)
                {
                    if(unit.hasLocationChangeStatus >= 1)
                    {
                        if (!unitWhoHaveLocationChangeStatus.Contains<Unit>(unit))
                        {
                            unitWhoHaveLocationChangeStatus.Add(unit);
                            expectedLocationChangeList.Add(0);
                        }
                        foreach(Status unitstatus in unit.statuses)
                        {
                            if(unitstatus.path != null && unitstatus.path.Count >= 1)       
                            {
                                int i = unitWhoHaveLocationChangeStatus.IndexOf(unit);
                                if(!(expectedLocationChangeList[i] + unitstatus.currentProgress > unitstatus.path.Count - 1))
                                {
                                    GameObject temp = Instantiate(expectedLocationMarker.gameObject);
                                    temp.GetComponent<SpriteRenderer>().sprite = unit.originalSprite;
                                    temp.transform.position = unitstatus.path[unitstatus.currentProgress + expectedLocationChangeList[i]];
                                }
                                expectedLocationChangeList[i] += 1;
                                if (expectedLocationChangeList[i] + unitstatus.currentProgress > unitstatus.path.Count)
                                {
                                    expectedLocationChangeList[i] = 0;
                                }
                            }
                        }
                    }
                }
            }

            if(expectedBlastPaths.Count > 0)
            {
                for (int individualBlastIndex = 0; individualBlastIndex < expectedBlastPaths.Count; individualBlastIndex++)
                {
                    int rowIndex = expectedBlastRowNumber[individualBlastIndex];
                    List<GameObject> blastmarkerList = new List<GameObject>();  
                    for (int markerIndex = 0; markerIndex < expectedBlastPaths[individualBlastIndex][rowIndex].Count; markerIndex++)
                    {
                        AnimatedField.Node node = expectedBlastPaths[individualBlastIndex][rowIndex][markerIndex];
                        blastmarkerList.Add(Instantiate(node.createdObject, node.position, new Quaternion(0, 0, 0, 1f)));
                    }
                    if(expectedBlastMarkers.Count != expectedBlastPaths.Count && individualBlastIndex == expectedBlastPaths.Count - 1)
                    {
                        expectedBlastMarkers.Add(blastmarkerList);
                    }
                    else
                    {
                        foreach (GameObject marker in expectedBlastMarkers[individualBlastIndex])
                        {
                            Destroy(marker);
                        }
                        expectedBlastMarkers[individualBlastIndex] = blastmarkerList;
                    }
                    expectedBlastRowNumber[individualBlastIndex] += 1;
                    if(expectedBlastRowNumber[individualBlastIndex] >= expectedBlastPaths[individualBlastIndex].Count())
                    {
                        expectedBlastRowNumber[individualBlastIndex] = 0;
                    }
                }
            }

            currentExpectedLoactionChangeSpeed += expectedLocationChangeSpeed;
        }
        // Changes Sprites of all units based on statuses they have independent of Turns
        if (currentTime >= secSpriteChangeSpeed)
        {
            foreach (Unit unit in scripts)
            {
                currentExpectedLoactionChangeSpeed = expectedLocationChangeSpeed;
                if (unit.statuses.Count != 0)
                {
                    unit.spriteIndex += 1;
                    if (unit.spriteIndex == unit.statuses.Count)
                    {
                        unit.spriteIndex = -1;
                        unit.ChangeSprite(unit.originalSprite);
                    }
                    else
                    {
                        if((unit.spriteIndex < unit.statuses.Count))
                        {
                            unit.ChangeSprite(unit.statuses[unit.spriteIndex].statusImage);
                        }
                    }
                }
                else
                {
                    unit.ChangeSprite(unit.originalSprite);
                }
            }
            currentTime = 0;
        }
        if (CanContinue(scripts[index]))
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

                if(statusObjectPriority.Count > 0)
                {
                    for(int i = 0;i < statusObjectPriority.Count;i++) 
                    { 
                        if(statusObjectPriority[i] < least)
                        {
                            least = statusObjectPriority[i];
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
                    scripts[i].enabled = true;
                    aUnitActed = true;
                    break;
                }
                else if(i == 0)
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
                        if(i < 0)
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
                if (statusObjectPriority.Count > 0)
                {
                    for (int i = 0; i < statusObjectPriority.Count; i++)
                    {
                        statusObjectPriority[i] -= least;
                        if (statusObjectPriority[i] <= 0)
                        {
                            statusObjectPriority[i] = (int)(StatusFields[i].createdFieldQuickness * baseTurnTime);

                            //reduces status duration of a status if it is supposed to go down at the end of a turn
                            if (!StatusFields[i].nonStandardDuration)
                            {
                                StatusObjectDuration[i] -= 1;
                            }

                            //if a status was removed in previous step reset sprite of unit otherwise if a status duration is 0 remove the status from the unit and reset the units sprite
                            if (StatusObjectDuration[i] <= 0)
                            {
                                StatusFields[i].RemoveStatusOnDeletion();
                            }
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
