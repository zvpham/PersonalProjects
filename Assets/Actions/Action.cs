using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Action : ScriptableObject
{
    public int blastRadius;
    public ActionName actionName;
    public int weight;
    public int maxCooldown;
    public int currentCooldown;
    public int duration;
    public int range;
    public bool isUsable;
    public ActionTypes[] actionType;
    public bool isActiveAction;
    public bool isTurnActivated;

    public Unit affectedUnit;

    public Status[] status;

    public GameObject targetingPrefab;
    public GameObject targetingSystem;

    public CreatedField createdField;

    abstract public void Activate(Unit self);

    abstract public void PlayerActivate(Unit self);


    public bool startActionPresets()
    {
        if (this.currentCooldown == 0)
        {
            this.currentCooldown = maxCooldown;
            this.isTurnActivated = true;
            return true;
        }
        return false;
    }

    public bool startStatusPresets(Unit self)
    {
        foreach (Status statuseffect in status)
        {
            Status temp = Instantiate(statuseffect);
            temp.ApplyEffect(self);
        }
        return true;
    }

    public void CalculateWeight()
    {
        // reorder ability list according to its weight.
        Debug.Log("hi");
    }
    public void AddAction()
    {

    }
}
