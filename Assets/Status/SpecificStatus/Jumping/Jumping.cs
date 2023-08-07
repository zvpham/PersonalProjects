using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/Jumping")]
public class Jumping : Status
{
    //private int jumpProgress = 0;
    public int speed = 0;
    override public void ApplyEffect(Unit target)
    {
        if(this.isFirstTurn)
        {
            this.currentProgress = 0;
            if(this.path.Count != 2)
            {
                this.speed = (this.path.Count / 2) + 1;
            }
            else
            {
                this.speed = this.path.Count / 2;
            }
            target.statuses.Add(this);
            this.isWorldTurnActivated = true;
            this.isFirstTurn = false;
            target.statusDuration.Add(this.statusDuration);
            target.gameManager.isLocationChangeStatus += 1;
            target.hasLocationChangeStatus += 1;

            AddUnusableStatuses(target);
        }
        for (int i = 0; i < this.speed; i++)
        {
            target.self.transform.position = this.path[i + this.currentProgress];
            target.gameManager.locations[target.index] = target.self.transform.position;
        }
        this.currentProgress = this.speed;
        this.speed = this.path.Count - this.speed;


        if (target.self.transform.position == this.path[this.path.Count - 1])
        {
            RemoveEffect(target);
        }

        for (int i = 0; i < target.statuses.Count; i++)
        {
            if(target.statuses[i].statusName.Equals(this.statusName))
            {
                if (target.statusDuration[i] < this.statusDuration) 
                {
                    target.statusDuration[i] = this.statusDuration;
                }
                return;
            }
        }
    }

    // Update is called once per frame
    override public void RemoveEffect(Unit target)
    {
        int index = target.statuses.IndexOf(this);
        target.statuses.RemoveAt(index);
        target.statusDuration.RemoveAt(index);
        target.gameManager.isLocationChangeStatus -= 1;
        target.hasLocationChangeStatus -= 1;
        foreach (ActionTypes actionType in actionTypesNotPermitted)
        {
            Debug.Log(target.unusableActionTypes[actionType] + " Testing");
            target.unusableActionTypes[actionType] = target.unusableActionTypes[actionType] - 1;
            if(target.unusableActionTypes[actionType] <= 0) { }
            target.unusableActionTypes.Remove(actionType);
        }
    }
}
