using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitGroup : MonoBehaviour, IInititiave
{
    [SerializeField] private List<Unit> units = new List<Unit>();
    public List<Unit> activeUnits = new List<Unit>();  
    public Team team;
    public CombatGameManager gameManager;
    public int CalculateInititive()
    {
        return 1;
    }

    public void StartTurn()
    {
        for(int i = 0; i < units.Count; i++)
        {
            activeUnits.Add(units[i]);
        }

        for(int i = 0; i < activeUnits.Count; i++)
        {
            activeUnits[i].currentActionsPoints = activeUnits[i].maxActionsPoints; 
        }

        if (team == Team.Player)
        {
            gameManager.StartPlayerTurn(activeUnits);
        }
    }

    public void EndTurn(Unit unitWhoEndedTurn)
    {
        activeUnits.Remove(unitWhoEndedTurn);
        if(activeUnits.Count == 0)
        {
            if (team == Team.Player)
            {
                gameManager.playerTurn.TurnEnd();
            }
            gameManager.TurnEnd(this);
        }
        else if (team == Team.Player)
        {
            gameManager.playerTurn.UnitGroupTurnEnd();
        }
    }

    public void Start()
    {
        gameManager.allinitiativeGroups.Add(this);
    }
}
