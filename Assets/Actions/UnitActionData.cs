using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitActionData
{
    public Action action;
    public bool active;
    public int amountUsedDuringRound = 0;
    public int actionCoolDown = 0;
    public int actionUsesLeft = 0;
}
