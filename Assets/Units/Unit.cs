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

    public int heroIndex = -1;  

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

    public int currentWeight;
    public bool overWieght;

    public int lowWeight;
    public int mediumWeight;
    public int highWeight;

    public int strength = 10;
    public int dexterity = 10;


    public int maxHealth = 100;
    public int currentHealth;
    public int maxArmor;
    public int currentArmor;
    public int moveSpeed = 3;

    public Action endTurn;
    public Action move;
    public int amountMoveUsedDuringRound;
    public List<Action> actions;
    public List<Passive> passives;
    public int maxActionsPoints = 2;
    public int currentActionsPoints = 0;
    public List<int> actionCooldowns = new List<int>();
    public List<int> actionUses = new List<int>();
    public List<int> amountActionUsedDuringRound = new List<int>();

    public List<Unit> selfInTheseUnitsThreatenedZones = new List<Unit>();

    public Team team;
    public UnitGroup group;

    public UnityAction onTurnStart;

    public CombatGameManager gameManager;

    public UnityAction<Action, TargetingSystem> OnSelectedAction;

    // Start is called before the first frame update
    void Start()
    {
        unitProfile = unitClass.UIUnitProfile;
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
            gameManager.spriteManager.CreateSpriteRenderer(0, -7, unitProfile, transform.position);
            gameManager.grid.GetXY(transform.position, out int x, out int y);
            gameManager.StartCombat();

            move = gameManager.resourceManager.actions[0];
            endTurn = gameManager.resourceManager.actions[1];
        }
        else
        {
            for (int j = 0; j < skillTreeOneBranchOne.Count; j++)
            {
                if (skillTreeOneBranchOne[j])
                {
                    unitClass.skillTree1.branch1.BranchSkills[j].UnlockSkill(this);
                }
            }

            for (int j = 0; j < skillTreeOneBranchTwo.Count; j++)
            {
                if (skillTreeOneBranchTwo[j])
                {
                    unitClass.skillTree1.branch2.BranchSkills[j].UnlockSkill(this);
                }
            }

            for (int j = 0; j < skillTreeTwoBranchOne.Count; j++)
            {
                if (skillTreeTwoBranchOne[j])
                {
                    unitClass.skillTree2.branch1.BranchSkills[j].UnlockSkill(this);
                }
            }

            for (int j = 0; j < skillTreeTwoBranchTwo.Count; j++)
            {
                if (skillTreeTwoBranchTwo[j])
                {
                    unitClass.skillTree2.branch2.BranchSkills[j].UnlockSkill(this);
                }
            }

            if (helmet != null)
            {
                helmet.EquipItem(this);
            }
            if (armor != null)
            {
                armor.EquipItem(this);
            }
            if (legs != null)
            {
                legs.EquipItem(this);
            }
            if (mainHand != null)
            {
                mainHand.EquipItem(this);
            }
            if (offHand != null)
            {
                offHand.EquipItem(this);
            }
            if (Item1 != null)
            {
                Item1.EquipItem(this);
            }
            if (Item2 != null)
            {
                Item2.EquipItem(this);
            }
            if (Item3 != null)
            {
                Item3.EquipItem(this);
            }
            if (Item4 != null)
            {
                Item4.EquipItem(this);
            }

            ChangeStrength(strength);
            ChangeDexterity(dexterity);
        }

    }

    public void ChangeStrength(int newStrength)
    {
        strength = newStrength;
        int strengthModifier = newStrength - 10;
        lowWeight = 50 + (25 * strengthModifier);
        mediumWeight = 100 + (25 * strengthModifier);
        highWeight = 150 + (25 * strengthModifier);
        ChangeWeight(currentWeight);
    }

    public int GetStrength()
    {
        return strength - 10;
    }

    public void ChangeDexterity(int newDexterity)
    {
        dexterity =  newDexterity;
        int dexModifier = (dexterity - 10) / 2;
        moveSpeed = 3 + dexModifier;
    }

    public int GetDexterityModifier()
    {
        return dexterity - 10;
    }

    public void ChangeWeight(int newWeight)
    {
        currentWeight = newWeight;
        overWieght = false;
        if(currentWeight > highWeight)
        {
            overWieght = true;
        }
        else if(currentWeight > mediumWeight)
        {
            maxActionsPoints = 2;
        }
        else if (currentWeight > lowWeight)
        {
            maxActionsPoints = 3;
        }
        else
        {
            maxActionsPoints = 4;
        }
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
        if(team == Team.Player)
        {
            Debug.Log("Used Action Points: " + usedActionPoints);
        }
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
        amountActionUsedDuringRound.Add(0);
    }
}
