using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class Status : ScriptableObject
{
    public Unit targetUnit;
    public float statusQuickness = 1;

    public int statusDuration;
    public bool nonStandardDuration = false;

    public string statusName;
    public Sprite statusImage;

    public bool isFieldStatus;
    public bool ApplyEveryTurn;
    public bool isFirstTurn = true;
    public bool isFirstWorldTurn = true;

    public List<ActionTypes> actionTypesNotPermitted;
    public List<ActionTypes> actionTypesThatCancelStatus;
    public List<ActionTypes> actionTypesThatActionMustContain;
    public List<Vector3> path;
    public int currentProgress;

    public event Action<bool> StatusRemovedOnActivation;

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

    public void AddStatusPreset(Unit target)
    {
        this.isFirstTurn = false;
        target.statuses.Add(this);
        target.statusDuration.Add(statusDuration);
        target.gameManager.statusPriority.Add((int)(target.gameManager.baseTurnTime * this.statusQuickness));
        target.gameManager.allStatuses.Add(this);
        target.gameManager.statusDuration.Add(this.statusDuration);
        this.targetUnit = target;
        ChangeQuickness(target.timeFlow);
    }

    public void RemoveStatusPreset(Unit target)
    {
        Debug.Log(this);
        target.gameManager.numberOfStatusRemoved += 1;
        int index = target.statuses.IndexOf(this);
        target.statuses.RemoveAt(index);
        target.statusDuration.RemoveAt(index);
        int statusindex = target.gameManager.allStatuses.IndexOf(this);
        target.gameManager.statusPriority.RemoveAt(statusindex);
        target.gameManager.allStatuses.RemoveAt(statusindex);
        target.gameManager.statusDuration.RemoveAt(statusindex);
    }

    public void CancelStatusIfActionContainsMatchingType(ActionTypes[] actionTypes, ActionName actionName)
    {
        foreach (ActionTypes actionType in actionTypes)
        {
            if (actionTypesThatCancelStatus.Contains(actionType))
            {
                RemoveEffect(targetUnit);
            }
        }
    }

    public void CancelStatusIfActionNotContainMatchingType(ActionTypes[] actionTypes, ActionName actionName)
    {
        if(actionName == ActionName.Wait)
        {
            return;
        }
        bool actionContainsMatchingActionType = false;
        foreach( ActionTypes actionType in actionTypes )
        {
            if (actionTypesThatActionMustContain.Contains(actionType))
            {
                actionContainsMatchingActionType = true;
            }
        }
        if (!actionContainsMatchingActionType)
        {
            RemoveEffect(targetUnit);
        }
    }

    public void ChangeQuickness(float value)
    {
        if(nonStandardDuration)
        {
            ChangeQuicknessNonstandard(value);
        }
        else
        {
            int index = targetUnit.gameManager.allStatuses.IndexOf(this);
            targetUnit.gameManager.allStatuses[index].statusQuickness *= value;
            targetUnit.gameManager.statusPriority[index] = (int)(targetUnit.gameManager.statusPriority[index] * value);
        }
    }

    abstract public void ChangeQuicknessNonstandard(float value);

}
