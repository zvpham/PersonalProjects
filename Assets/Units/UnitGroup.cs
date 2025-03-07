using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitGroup : UnitSuperClass, IInititiave
{
    public int mercenaryIndex;

    [SerializeField] public List<Unit> units = new List<Unit>();
    public List<Unit> activeUnits = new List<Unit>();  
    public CombatGameManager gameManager;

    public bool inOverWorld = false;

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
            activeUnits[i].currentMajorActionsPoints = activeUnits[i].maxMajorActionsPoints; 
        }

        if (team == Team.Player)
        {
            gameManager.StartPlayerTurn(activeUnits);
        }
        else
        {
            gameManager.StartAITurn(activeUnits, team);
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
        else
        {
            if (team == Team.Player)
            {
                gameManager.playerTurn.UnitGroupTurnEnd();
            }
            else
            {
                IInititiave unitInitiative = activeUnits[0];
                unitInitiative.StartTurn();
            }
        }
    }

    public void Start()
    {
        if (!inOverWorld && !gameManager.testing)
        {
            GetReadyForCombat();
        }
    }

    public void GetReadyForCombat()
    {
        gameManager.allinitiativeGroups.Add(this);
        switch (team)
        {
            case Team.Team2:
                gameManager.AITurn1.unitSuperClasses.Add(this);
                break;
            case Team.Team3:
                gameManager.AITurn2.unitSuperClasses.Add(this);
                break;
            case Team.Team4:
                gameManager.AITurn3.unitSuperClasses.Add(this);
                break;
        }
    }

    public void UnitDeath(Unit unit)
    {
        units.Remove(unit);
        activeUnits.Remove(unit);
        if(units.Count == 0)
        {
            gameManager.UnitGroupDeath(unit);
        }
    }
}
