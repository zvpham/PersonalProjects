using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR;
using UnityEngine;

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

    public List<ActionTypes> actionTypesNotPermitted;
    public List<ActionTypes> actionTypesThatCancelStatus;
    public List<Vector3> path;
    public int currentProgress;

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

    public void ChangeQuickness(float value)
    {
        if(nonStandardDuration)
        {
            ChangeQuicknessNonstandard(value);
        }
        else
        {
            int index = targetUnit.gameManager.allStatuses.IndexOf(this);
            Debug.Log("Value: " + value);
            targetUnit.gameManager.allStatuses[index].statusQuickness *= value;
            Debug.Log("Speed: " + targetUnit.gameManager.allStatuses[index].statusQuickness);
            Debug.Log("Quickness: " + (int)(targetUnit.gameManager.priority[index] * value));
            targetUnit.gameManager.priority[index] = (int)(targetUnit.gameManager.statusPriority[index] * value);
        }
    }

    abstract public void ChangeQuicknessNonstandard(float value);

}
