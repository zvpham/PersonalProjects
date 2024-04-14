using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Action : ScriptableObject
{
    public ActionName actionName;
    public int weight;
    public int maxCooldown;
    public int currentCooldown;
    public bool isUsable;
    public ActionTypes[] actionType;
    public bool actionIsActive;
    public bool isActiveAction;
    public bool isChaseAction;
    public string description;
    //public bool isTurnActivated;

    public Unit affectedUnit;
    abstract public void Activate(Unit self);

    abstract public void PlayerActivate(Unit self);

    abstract public int CalculateWeight(Unit self);

    abstract public void AnimationEnd();

    virtual public void Activate(Unit self, Vector3 targetLocation)
    {

    }
    virtual public int CalculateWeight(Unit self, Vector3 targetLocation)
    {
        return -1;
    }

    public void StartActionPresetAI(Unit self)
    {
        this.currentCooldown = maxCooldown;
        //this.isTurnActivated = true;
        this.affectedUnit = self;
    }

    public bool startActionPresets(Unit self)
    {
        if (this.currentCooldown == 0)
        {
            this.currentCooldown = maxCooldown;
            //this.isTurnActivated = true;
            this.affectedUnit = self;
            return true;
        }
        return false;
    }
}
