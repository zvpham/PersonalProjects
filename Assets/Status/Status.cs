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


    public void AddStatusPreset(Unit target, int newDuration, bool noDuration)
    {
        int statusIndex = GetIndexOfStatus(target);
        if (statusIndex == -1)
        {
            StatusData newStatus = new StatusData(this, newDuration, noDuration);
            target.statuses.Add(newStatus);
        }
        else
        {
            StatusData oldStatusData = target.statuses[statusIndex];
            if (oldStatusData.duration < currentStatusDuration)
            {
                oldStatusData.duration = currentStatusDuration;
            }
        }
    }
    abstract public void AddStatus(ActionStatusData actionStatusData);

    abstract public void RemoveStatus(Unit target);

    // Check to see if Status Consequence should continue
    abstract public bool ContinueEvent(Action occuringAction, Passive occuringPassive);

    // fnction to call to modify action
    abstract public void ModifiyAction(Action action, AttackData attackData);

    abstract public void ModifyAttackData(AttackData attackData);

    public void RemoveStatusFinal(Unit target)
    {
        for (int i = 0; i < target.statuses.Count; i++)
        {
            if (target.statuses[i] == null || target.statuses[i].status == this)
            {
                target.statuses.RemoveAt(i);
                i--;
            }
        }
    }

    // return -1 if status is not present
    public int GetIndexOfStatus(Unit target)
    {
        for (int i = 0; i < target.statuses.Count; i++)
        {
            if (target.statuses[i] != null && target.statuses[i].status == this)
            {
                return i;
            }
        }
        return -1;
    }
}
