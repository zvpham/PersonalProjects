using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Status/Jumping")]
public class Jumping : Status
{
    //private int jumpProgress = 0;
    public int speed = 0;
    public int currentJumpProgress = 0;
    override public void ApplyEffect(Unit target)
    {
        if(this.currentJumpProgress + this.speed == this.path.Count)
        {
            RemoveEffect(target);
        }
        Debug.Log("wadwd " + this.currentJumpProgress);
        foreach(Vector3 node in this.path)
        {
            Debug.Log(node);
        }
        if(this.isFirstTurn)
        {
            this.currentJumpProgress = 0;
            if(this.path.Count != 2)
            {
                this.speed = (this.path.Count / 2) + 1;
            }
            else
            {
                this.speed = this.path.Count / 2;
            }
            Debug.Log("This is speed " + this.speed);
            target.statuses.Add(this);
            this.isFirstTurn = false;
            target.statusDuration.Add(this.statusDuration);
        }
        for (int i = 0; i < this.speed; i++)
        {
            Debug.Log("whoaidhawd" + (i + this.currentJumpProgress));
            target.self.transform.position = this.path[i + this.currentJumpProgress];
        }
        this.isWorldTurnActivated = true;
        this.currentJumpProgress = this.speed;
        this.speed = this.path.Count - this.speed;

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
    }
}
