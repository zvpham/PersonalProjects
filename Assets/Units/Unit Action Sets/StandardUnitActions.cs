using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ActionSets/StandardActions")]
public class StandardUnitActions : ScriptableObject
{
    public Move move;
    public EndTurn endTurn;
    public Evade evade;

    public void AddActions(Unit unit)
    {
        move.AddAction(unit, 0);
        endTurn.AddAction(unit, 1);
        evade.AddAction(unit, 2);
    }
}
