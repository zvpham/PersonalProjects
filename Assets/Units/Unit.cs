using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Unit : MonoBehaviour, IInititiave
{
    public int moveSpeed;
    public List<Action> actions;

    public int maxActionsPoints = 2;
    public int currentActionsPoints = 0;
    public List<int> actionCooldowns = new List<int>();
    public List<int> actionUses = new List<int>();

    public List<Unit> selfInTheseUnitsThreatenedZones = new List<Unit>();

    public Team team;
    public UnitGroup group;

    public UnityAction onTurnStart;

    public CombatGameManager gameManager;

    public int CalculateInititive()
    {
        return 2;
    }

    public void StartTurn()
    {
        onTurnStart?.Invoke();
        currentActionsPoints = maxActionsPoints;
        if (team == Team.Player)
        {
            gameManager.StartPlayerTurn(this);
        }
        else
        {
            UseActionPoints(2);
        }
    }

    public void UseActionPoints(int usedActionPoints)
    {
        currentActionsPoints -= usedActionPoints;
        if(currentActionsPoints <= 0)
        {
            EndTurn();
        }
        else if(team == Team.Player)
        {
            gameManager.playerTurn.SelectAction(gameManager.playerTurn.currentlySelectedAction);
        }
    }

    public void EndTurn()
    {
        if (group != null)
        {
            group.EndTurn(this);
        }
        else
        {
            if(team == Team.Player)
            {
                gameManager.playerTurn.TurnEnd();
            }
            gameManager.TurnEnd(this);
        }
    }

    public void MovePositions(Vector3 originalPosition, Vector3 newPosition)
    {
        gameManager.SetGridObject(null, originalPosition);
        gameManager.SetGridObject(this, newPosition);
        this.transform.position = newPosition;
    }

    public void AddAction(Action newAction)
    {
        actions.Add(newAction);
        actionCooldowns.Add(0);
        actionUses.Add(newAction.maxUses);
    }

    // Start is called before the first frame update
    void Start()
    {
        if(group == null)
        {
            gameManager.allinitiativeGroups.Add(this);
        }

        if(team == Team.Player)
        {
            gameManager.playerTurn.playerUnits.Add(this);
        }
        
        for(int i = 0; i < actions.Count; i++)
        {
            actionCooldowns.Add(0);
            actionUses.Add(actions[i].maxUses);
        }
        gameManager.SetGridObject(this, transform.position);
        gameManager.units.Add(this);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
