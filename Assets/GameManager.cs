using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;



public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public List<double> speeds;
    public List<int> priority;
    public int baseTurnTime = 500;
    public int worldPriority = 0;

    public Tilemap groundTilemap;
    public Tilemap collisionTilemap;
    public List<Vector3> locations = new List<Vector3>();
    public List<Vector3> itemLocations = new List<Vector3>();


    public List<Unit> scripts;
    public List<EnemyTest> enemies;
    public List<Item> items;

    public List<Sprite> sprites;
    private InputManager inputManager;

    private int least;
    private int index = 0;

    public float secSpriteChangeSpeed;
    private float currentTime = 0;

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
    }

    void Start()
    {
        inputManager = InputManager.instance;
        collisionTilemap = Obstacles.instance.collisionTilemap;
        groundTilemap = Ground.instance.groundTilemap;

    }

    // Update is called once per frame
    void Update()
    {
        // Changes Sprites of all units based on statuses they have independent of Turns
        currentTime += Time.deltaTime;
        if (currentTime >= secSpriteChangeSpeed)
        {
            //Debug.Log("THIS is Sprite Change");
            foreach (Unit unit in scripts)
            {
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
                        unit.ChangeSprite(unit.statuses[unit.spriteIndex].statusImage);
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
            // finds the lowest priority amongst all the units
            // if we are at the top of a turn
            if (duringTurn == 0)
            {
                least = (int)priority[0];
                for (int i = 0; i < priority.Count; i++)
                {
                    if (priority[i] < least)
                    {
                        least = priority[i];
                    }
                }
            }
            //lowers priority of a unit by the least amount of priority 
            // if priority is 0 activate unit and end loop
            // if mid turn will resume list one more than unit that just went
            for (int i = index + duringTurn; i < priority.Count;)
            {
                priority[i] = priority[i] - least;

                if ((int)priority[i] == 0)
                {
                    index = i;
                    duringTurn = 1;
                    scripts[i].enabled = true;
                    break;
                }
                else if(i == 0)
                {
                    duringTurn = 1;
                    break;
                }
                else
                {
                    index = i;
                    break;
                }
            }

            
            //end turn reset all turn variables and reset priority of units who acted
            if (index + duringTurn == priority.Count)
            {
                index = 0;
                duringTurn = 0;
                for (int i = 0; i < priority.Count; i++)
                {
                    if (priority[i] <= 0)
                    {
                        priority[i] = (int) (baseTurnTime * speeds[i]);
                    }
                }
                // changes the worlds turn if 0 reduce status effects by 1 and resets unit sprite and remove statuses if a duraction is 0
                worldPriority -= least;
                if (worldPriority <= 0)
                {

                    foreach (Unit unit in scripts)
                    {
                        for (int i = 0; i < unit.statusDuration.Count; i++)
                        {
                            if (!unit.statuses[i].nonStandardDuration)
                            {
                                unit.statusDuration[i] -= 1;
                            }
                            if (unit.statuses[i].ApplyEveryTurn)    
                            {
                                if (!unit.statuses[i].isWorldTurnActivated)
                                {
                                    unit.statuses[i].isWorldTurnActivated = false;
                                }
                                else
                                {
                                    unit.statuses[i].ApplyEffect(unit);
                                }
                            }
                            if (unit.statusDuration[i] <= 0)
                            {
                                unit.statuses[i].RemoveEffect(unit);
                                unit.ChangeSprite(unit.originalSprite);
                                unit.spriteIndex = -1;
                            }
                        }
                    }
                    worldPriority = 1 * baseTurnTime;
                }
            }
        }
    }


    private bool CanContinue(MonoBehaviour script)
    {
        //Debug.Log(!script.isActiveAndEnabled);
        return !script.isActiveAndEnabled;
    }
}   
