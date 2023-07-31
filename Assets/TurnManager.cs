            using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;
    public List<int> speeds;
    public List<int> priority;
    public int baseTurnTime = 500;

    public List<(double, double)> locations = new List<(double, double)>();

    public List<MonoBehaviour> scripts;

    private InputManager inputManager;

    private int least;
    private int index = 0;
    // during turn 0 = no; 1 = yes
    private int duringTurn = 0;


    // Start is called before the first frame update


    void Awake()
            {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(this);

    }

    void Start()
    {
        inputManager = InputManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        
        for(int i =  0; i < priority.Count; i++)
        {
            Debug.Log("This is Speeds" +  i + " " + priority[i]);
        }
        
        if (CanContinue(scripts[index]))
        {
            Debug.Log("Test 1: " + index);
            // finds the lowest priority amongst all the units
            // if we are at the top of a turn
            if (duringTurn == 0)
            {
                Debug.Log("Test 2");
                least = (int)priority[0];
                for (int i = 0; i < priority.Count; i++)
                {
                    Debug.Log("Test 3: " + least);
                    if (priority[i] < least)
                    {
                        Debug.Log("Test 4");
                        least = priority[i];
                    }
                }
            }
            //lowers priority of a unit by the least amount of priority 
            // if priority is 0 activate unit and end loop
            // if mid turn will resume list one more than unit that just went
            for (int i = index + duringTurn; i < priority.Count;)
            {
                Debug.Log("Test 5");
                priority[i] = priority[i] - least;
                if(i == 0)
                {
                    Debug.Log("This is Priority 0 " + priority[i]);
                }

                if ((int)priority[i] == 0)
                {
                    index = i;
                    duringTurn = 1;
                    scripts[i].enabled = true;
                    Debug.Log("Test 6");
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
            if(index + duringTurn == priority.Count)
            {
                Debug.Log("Test 7");
                index = 0;
                duringTurn = 0;
                for (int i = 0; i < priority.Count; i++)
                {
                    Debug.Log("Test 8");
                    if (priority[i] <= 0)
                    {
                        Debug.Log("Test 9");
                        priority[i] = baseTurnTime * speeds[i];
                        if (i == 1)
                        {
                            Debug.Log("This is Priority 1 Reset " + priority[i]);
                        }
                    }
                }
            }
        }
    }


    private bool CanContinue(MonoBehaviour script)
    {
        Debug.Log("Can Continue");
        Debug.Log(!script.isActiveAndEnabled);
        return !script.isActiveAndEnabled;
    }

}   
