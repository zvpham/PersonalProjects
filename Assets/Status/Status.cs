using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.XR;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class Status : ScriptableObject
{
    public int statusPrefabIndex;
    public Unit targetUnit;

    public float statusQuickness = 1;
    public int statusPriority;
    
    public int currentStatusDuration;
    public bool nonStandardDuration = false;

    public string statusName;
    public Sprite statusImage;
    public Action activeAction;

    public bool isActiveStatus;
    public bool isFieldStatus;
    public bool ApplyEveryTurn;
    public bool isFirstApply;
    public bool isFirstWorldTurn = true;
    public bool isMovementStatus = false;

    public int statusIntData;
    public string statusStringData;
    public bool statusBoolData;

    public List<ActionTypes> actionTypesNotPermitted;
    public List<ActionTypes> actionTypesThatCancelStatus;
    public List<ActionTypes> actionTypesThatActionMustContain;

    // Start is called before the first frame update
    abstract public void ApplyEffect(Unit target, int newDuration);

    abstract public void onLoadApply(Unit target);

    virtual public void ApplyEffectEnemy(Unit target, int newDuration)
    {

    }

    virtual public void ApplyEffectPlayer(Unit target, int newDuration)
    {

    }


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

    public void RemoveUnusableStatuses(Unit target)
    {
        foreach (ActionTypes actionType in actionTypesNotPermitted)
        {
            target.unusableActionTypes[actionType] = target.unusableActionTypes[actionType] - 1;
            if (target.unusableActionTypes[actionType] <= 0) { }
            target.unusableActionTypes.Remove(actionType);
        }
    }

    // bool is for whether to continue and apply the effects of the status
    // true - don't apply affects, false - Aplly effects
    public bool AddStatusPreset(Unit target, int newDuration)
    {
        for (int i = 0; i < target.statuses.Count; i++)
        {
            if (target.statuses[i].statusName.Equals(this.statusName))
            {
                this.isFirstApply = false;
                if (currentStatusDuration < newDuration && newDuration > 0)
                {
                    currentStatusDuration = newDuration;
                }

                if (ApplyEveryTurn)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        this.isFirstApply = true;
        statusPriority = (int)(target.gameManager.baseTurnTime * this.statusQuickness);
        currentStatusDuration = newDuration;
        target.statuses.Add(this);
        target.gameManager.allStatuses.Add(this);
        target.gameManager.mainGameManger.allStatuses.Add(this);
        this.targetUnit = target;
        ChangeQuickness(target.timeFlow);
        AddUnusableStatuses(target);
        return false;
    }

    public void AddStatusOnLoadPreset(Unit target)
    {
        this.isFirstApply = false;
        this.targetUnit = target;
        target.statuses.Add(this);
        target.gameManager.allStatuses.Add(this);
        target.gameManager.mainGameManger.allStatuses.Add(this);
        ChangeQuickness(target.timeFlow);
        AddUnusableStatuses(target);
    }

    public void RemoveStatusPreset(Unit target)
    {
        if (isActiveStatus)
        {
            activeAction.actionIsActive = false;
        }
        target.gameManager.mainGameManger.numberOfStatusRemoved += 1;
        int index = target.statuses.IndexOf(this);
        target.statuses.RemoveAt(index);
        int statusindex = target.gameManager.allStatuses.IndexOf(this);
        target.gameManager.allStatuses.RemoveAt(statusindex);

        statusindex = target.gameManager.mainGameManger.allStatuses.IndexOf(this);
        target.gameManager.mainGameManger.allStatuses.RemoveAt(statusindex);
        RemoveUnusableStatuses(target);
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
            statusPriority = (int)(statusPriority * value);
        }
    }

    abstract public void ChangeQuicknessNonstandard(float value);

}
