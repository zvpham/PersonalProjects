using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class CreatedObjectStatus : CreatedObject
{
    public float quickness = 1;
    public int duration;
    public Status[] statuses;
    public GameObject spriteObject;

    public override void RemoveObject(GameManager gameManager, bool affectFlying)
    {
        if (statuses != null)
        {
            Vector3 location = grid.GetWorldPosition(x, y);

            Unit unit = gameManager.grid.GetGridObject(location);
            if(unit != null)
            {
                if(unit.flying)
                {
                    if(affectFlying)
                    {
                        foreach (Status status in statuses)
                        {
                            if (unit.statuses.Contains(status))
                            {
                                status.RemoveEffect(unit);
                            }

                        }
                    }
                }
                else
                {
                    foreach (Status status in statuses)
                    {
                        if (unit.statuses.Contains(status))
                        {
                            status.RemoveEffect(unit);
                        }

                    }
                }
            }
        }
        GameObject.Destroy(spriteObject);
    }

    public override void ApplyObject(Unit unit)
    {
        foreach(Status status in statuses)
        {
            status.ApplyEffect(unit, duration);
        }
    }

    public override bool CheckStatus(Status status)
    {
        List<string> statusNames = new List<string>();
        foreach(Status tempStatus in statuses)
        {
            statusNames.Add(tempStatus.statusName);
        }
        foreach(string statusName in statusNames)
        {
            if (statusName.Equals(status.statusName))
            {
                return true;
            }
        }
        return false;
    }

    public override string ToString()
    {
        return timeflow.ToString();
    }
}
