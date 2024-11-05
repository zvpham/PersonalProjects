using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Status : ScriptableObject
{
    public int statusPrefabIndex;

    public int currentStatusDuration;
    public bool nonStandardDuration = false;

    public string statusName;
    public Sprite statusImage;

    public bool isActiveStatus;
    public bool ApplyEveryTurn;
    public bool isFirstApply;
    public bool isFirstWorldTurn = true;

    // Start is called before the first frame update
    abstract public void AddStatus(Unit target, int newDuration);

    // Update is called once per frame
    abstract public void RemoveStatus(Unit target);

    abstract public bool ContinueEvent(Action occuringAction, Passive occuringPassive);
}
