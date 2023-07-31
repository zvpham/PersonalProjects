using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  abstract class Action : ScriptableObject
{
    public string actionName;
    public int weight;
    public int maxCooldown;
    public int currentCooldown;
    public int duration;
    public bool isUsable;
    public bool isMovement;
    public bool isRanged;
    public bool isFreeStateChange;
    public bool isActiveAction;
    public bool isTurnActivated;
    public Status status;



    abstract public void Activate(Unit self);
 

    public void CalculateWeight()
    {
        // reorder ability list according to its weight.
        Debug.Log("hi");
    }
    public void AddAction()
    {

    }
}
