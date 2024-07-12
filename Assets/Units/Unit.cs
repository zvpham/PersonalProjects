using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Inventory.Model;

public class Unit : UnitSuperClass, IInititiave
{
    // Class Selection UI
    public bool inOverWorld = false;

    public Sprite unitProfile; 

    public UnitClass unitClass;
    public List<bool> skillTreeOneBranchOne = new List<bool>() { false, false, false, false };
    public List<bool> skillTreeOneBranchTwo = new List<bool>() { false, false, false, false };
    public List<bool> skillTreeTwoBranchOne = new List<bool>() { false, false, false, false };
    public List<bool> skillTreeTwoBranchTwo = new List<bool>() { false, false, false, false };

    public EquipableItemSO helmet;
    public EquipableItemSO armor;
    public EquipableItemSO legs;
    public EquipableItemSO mainHand;
    public EquipableItemSO offHand;
    public EquipableItemSO Item1;
    public EquipableItemSO Item2;
    public EquipableItemSO Item3;
    public EquipableItemSO Item4;

    public int[] itemUses = new int[4];

    public int maxHealth;
    public int currentHealth;
    public int maxArmor;
    public int currentArmor;

    public int moveSpeed;
    public List<Action> actions;
    public List<Passive> passives;
    public int maxActionsPoints = 2;
    public int currentActionsPoints = 0;
    public List<int> actionCooldowns = new List<int>();
    public List<int> actionUses = new List<int>();

    public List<Unit> selfInTheseUnitsThreatenedZones = new List<Unit>();

    public Team team;
    public UnitGroup group;

    public UnityAction onTurnStart;

    public CombatGameManager gameManager;

    public UnityAction<Action, TargetingSystem> OnSelectedAction;

    public void Awake()
    {
        unitProfile = unitClass.UIUnitProfile;
    }

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

    public void HandleSelectedAction (Action selectedAction, TargetingSystem actionTargetingSystem)
    {
        OnSelectedAction?.Invoke(selectedAction, actionTargetingSystem);
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
        if (!inOverWorld)
        {
            if (group == null)
            {
                gameManager.allinitiativeGroups.Add(this);
            }

            if (team == Team.Player)
            {
                gameManager.playerTurn.playerUnits.Add(this);
            }

            for (int i = 0; i < actions.Count; i++)
            {
                actionCooldowns.Add(0);
                actionUses.Add(actions[i].maxUses);
            }
            gameManager.SetGridObject(this, transform.position);
            gameManager.units.Add(this);
        }
    }
}
