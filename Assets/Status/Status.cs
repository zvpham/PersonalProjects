using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Status : ScriptableObject
{
    public Unit targetUnit;
    public float statusQuickness = 1;

    public int statusDuration;
    public bool nonStandardDuration = false;

    public string statusName;
    public Sprite statusImage;
    public bool ApplyEveryTurn;
    public bool isFirstTurn = true;
    public List<ActionTypes> actionTypesNotPermitted;
    public List<ActionTypes> actionTypesThatCancelStatus;
    public List<Vector3> path;
    public int currentProgress;
    /*  
    public static void make()S
    {
        Instantiate(statusPrefab);
       // Instance = statusPrefab.GetComponent<Sprinting>();
    }
    */
    /*
    public void Create(Unit target)
    {
        GameObject temp = Instantiate(statusPrefab);
        Status status = temp.GetComponent<Status>();
        target.activeStatuses.Add(status);
    }
    */
    // Start is called before the first frame update
    abstract public void ApplyEffect(Unit target);


    // Update is called once per frame
    abstract public void RemoveEffect(Unit target);

    public void AddUnusableStatuses(Unit target)
    {
        foreach (ActionTypes actionType in this.actionTypesNotPermitted)
        {
            if (target.unusableActionTypes.ContainsKey(actionType))
            {
                target.unusableActionTypes[actionType] = target.unusableActionTypes[actionType] + 1;
            }
            else
            {
                target.unusableActionTypes.Add(actionType, 1);
            }
        }
    }

}
