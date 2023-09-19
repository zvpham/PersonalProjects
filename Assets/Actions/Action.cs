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
    public int duration;
    public bool isUsable;
    public ActionTypes[] actionType;
    public bool isActiveAction;
    public bool isTurnActivated;

    public Unit affectedUnit;

    public Status[] status;

    public GameObject targetingPrefab;
    public GameObject targetingSystem;

    public CreatedField createdField;
    public GameObject createObject;
    
    public int range;
    public int blastRadius;
    public float blastAngle;
    abstract public void Activate(Unit self);

    abstract public void PlayerActivate(Unit self);

    abstract public int CalculateWeight(Unit self);

    public void StartActionPresetAI(Unit self)
    {
        this.currentCooldown = maxCooldown;
        this.isTurnActivated = true;
        this.affectedUnit = self;
    }

    public bool startActionPresets(Unit self)
    {
        if (this.currentCooldown == 0)
        {
            this.currentCooldown = maxCooldown;
            this.isTurnActivated = true;
            this.affectedUnit = self;
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

}
