using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Inventory.Model;
using JetBrains.Annotations;
using System;

public class Unit : UnitSuperClass, IInititiave
{
    // Class Selection UI
    public bool inOverWorld = false;

    public Sprite unitProfile;
    public CombatAttackUI combatAttackUi;

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
    public EquipableItemSO backUpMainHand;
    public EquipableItemSO backUpOffHand;
    public EquipableItemSO Item1;
    public EquipableItemSO Item2;
    public EquipableItemSO Item3;
    public EquipableItemSO Item4;

    public int[] itemUses = new int[4];

    public int currentWeight;
    public int backUpWeight;
    public bool overWieght;

    public int lowWeight;
    public int mediumWeight;
    public int highWeight;

    public int strength = 0;
    public int dexterity = 0;


    public int maxHealth = 100;
    public int currentHealth;
    public int maxArmor;
    public int currentArmor;
    public int moveSpeed = 3;

    public float damageResistence = 0f;
    public float shieldDamageResistence = 0f;

    public int x;
    public int y;

    public Action endTurn;
    public Action move;
    public int amountMoveUsedDuringRound;
    public List<Action> actions;
    public List<bool> actionsActives;
    public List<Passive> passives;
    public int maxActionsPoints = 2;
    public int currentActionsPoints = 0;
    public List<int> actionCooldowns = new List<int>();
    public List<int> actionUses = new List<int>();
    public List<int> amountActionUsedDuringRound = new List<int>();

    public List<Unit> selfInTheseUnitsThreatenedZones = new List<Unit>();

    public MoveModifier moveModifier;
    public int moveModifierPriority = -1;

    public Team team;
    public UnitGroup group;

    public bool confirmDeath;

    public UnityAction onTurnStart;
    public UnityAction<Unit> OnDeath;
    // Self Unit, Damage Dealing Unit, Damage, IsMelee, isFirstInstanceOfDamage
    public UnityAction<Unit, Unit, bool, bool> OnDamage;

    public CombatGameManager gameManager;

    public UnityAction<Action, TargetingSystem> OnSelectedAction;

    // Start is called before the first frame update
    void Start()
    {
        if(unitClass != null)
        {
            unitProfile = unitClass.UIUnitProfile;
        }
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
            if(Item1.GetType() == typeof(EquipableAmmoSO))
            {
                itemUses[0] = Item1.mainThreeMin;
            }
        }
        if (Item2 != null)
        {
            Item2.EquipItem(this);
            if (Item1.GetType() == typeof(EquipableAmmoSO))
            {
                itemUses[1] = Item2.mainThreeMin;
            }
        }
        if (Item3 != null)
        {
            Item3.EquipItem(this);
            if (Item1.GetType() == typeof(EquipableAmmoSO))
            {
                itemUses[2] = Item3.mainThreeMin;
            }
        }
        if (Item4 != null)
        {
            Item4.EquipItem(this);
            if (Item1.GetType() == typeof(EquipableAmmoSO))
            {
                itemUses[3] = Item4.mainThreeMin;
            }
        }
        if (backUpMainHand != null)
        {
            backUpMainHand.EquipItem(this, true);
        }
        if (backUpOffHand != null)
        {
            backUpOffHand.EquipItem(this, true);
        }

        ChangeStrength(strength);
        ChangeDexterity(dexterity);
        if (!inOverWorld && !gameManager.testing)
        {
            if (moveModifier == null)
            {
                moveModifier = gameManager.resourceManager.moveModifiers[0];
            }

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
            gameManager.spriteManager.CreateSpriteRenderer(0, unitProfile, transform.position);
            gameManager.StartCombat();

            move = gameManager.resourceManager.actions[0];
            endTurn = gameManager.resourceManager.actions[1];
            currentHealth = maxHealth;
        }
    }

    public void ChangeStrength(int newStrength)
    {
        strength = newStrength;
        int weightAdjustmentModifier = 25;
        lowWeight = 50 + (weightAdjustmentModifier * ( strength / 2));
        mediumWeight = 100 + (weightAdjustmentModifier * (strength / 2));
        highWeight = 150 + (weightAdjustmentModifier * (strength / 2));
        ChangeWeight(currentWeight);
    }

    public float GetMaximumDamageModifer()
    {
        return .1f * strength;
    }

    public void ChangeDexterity(int newDexterity)
    {
        dexterity =  newDexterity;
        moveSpeed = 3 + (newDexterity / 2);
    }

    public float GetMinimumDamageModifer()
    {
        return .1f * dexterity;
    }


    public void ChangeWeight(int newWeight)
    {
        currentWeight = newWeight;
        overWieght = false;
        if(currentWeight > highWeight)
        {
            overWieght = true;
            maxActionsPoints = 1;
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

    public void ChangeBackUpWeight(int newWeight)
    {
        backUpWeight = newWeight;
    }

    public int CalculateInititive()
    {
        return 2;
    }

    public void StartTurn()
    {
        onTurnStart?.Invoke();
        currentActionsPoints = maxActionsPoints;
        CheckActionsDisabled();
        if (team == Team.Player)
        {
            gameManager.StartPlayerTurn(this);
        }
        else
        {
            UseActionPoints(currentActionsPoints);
        }
    }

    public void CheckActionsDisabled()
    {
        actionsActives = new List<bool>();
        for(int i = 0; i < actions.Count; i++)
        {
            actionsActives.Add(actions[i].CheckActionUsable(this));
        }
    }

    public void HandleSelectedAction (Action selectedAction, TargetingSystem actionTargetingSystem)
    {
        OnSelectedAction?.Invoke(selectedAction, actionTargetingSystem);
    }

    public void UseActionPoints(int usedActionPoints, bool isAnotherActionMoveAndOnlyMoved = true)
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
        else if(team == Team.Player &&  isAnotherActionMoveAndOnlyMoved)
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

    public Tuple<int, int, List<AttackDataUI>> CalculateEstimatedDamage(int minDamageValue, int maxDamageValue, bool ignoreShield)
    {
        float combatDamageResistenceValue = damageResistence;
        List<AttackDataUI> unitAttackedData = new List<AttackDataUI> ();

        if (!ignoreShield && shieldDamageResistence != 0)
        {
            combatDamageResistenceValue += shieldDamageResistence;
            AttackDataUI shieldedAttackDataUI = new AttackDataUI();
            shieldedAttackDataUI.data = "Shielded " + "-" + shieldDamageResistence;
            shieldedAttackDataUI.attackState = attackState.Benediction;
            shieldedAttackDataUI.attackDataType = attackDataType.Modifier;
            unitAttackedData.Add(shieldedAttackDataUI);
        }

        if (damageResistence > 1)
        {
            combatDamageResistenceValue = 1f;
        }

        int newMinDamageValue = minDamageValue - (int)(minDamageValue * combatDamageResistenceValue);
        int newMaxDamageValue =  maxDamageValue - (int)(maxDamageValue * combatDamageResistenceValue);

        Tuple<int, int, List<AttackDataUI>> attackedData = new Tuple<int, int, List<AttackDataUI>> (newMinDamageValue, newMaxDamageValue, unitAttackedData);
        return attackedData;
    }

    public void TakeDamage(Unit damageDealingUnit, List<int> minDamage, List<int> maxDamage, bool meleeContact, bool ignoreShield, bool ignoreArmor,
        bool firstInstanceOfDamageSetUp)
    {
        if(minDamage.Count != maxDamage.Count)
        {
            Debug.LogError("min Damaage and Max Damage should be the same");
            return;
        }
        int seed = System.DateTime.Now.Millisecond;
        UnityEngine.Random.InitState(seed);

        float combatDamageResistenceValue = damageResistence;

        if (!ignoreShield)
        {
            combatDamageResistenceValue += shieldDamageResistence;
        }

        if (damageResistence > 1)
        {
            combatDamageResistenceValue = 1f;
        }

        int intialArmor = currentArmor;
        int intialHealth = currentHealth;
        bool firstInstanceOfDamage = firstInstanceOfDamageSetUp;
        for(int i = 0; i <  minDamage.Count; i++)
        {
            int damageValue = UnityEngine.Random.Range(minDamage[i], maxDamage[i] + 1);
            damageValue = damageValue - (int) (damageValue * combatDamageResistenceValue);
            OnDamage?.Invoke(this, damageDealingUnit, meleeContact, firstInstanceOfDamage);
            firstInstanceOfDamage = false;

            if(ignoreArmor)
            {
                currentHealth -= damageValue;
            }
            else
            {
                currentArmor -= damageValue;
                if (currentArmor < 0)
                {
                    currentHealth += currentArmor;
                    currentArmor = 0;
                }
            }
        }

        if(currentHealth <= 0)
        {
            Death();
        }
        else
        {
            Debug.Log("damage");
            DamageAnimation damageAnimation = Instantiate(gameManager.resourceManager.damageAnimation);
            damageAnimation.SetParameters(combatAttackUi, this, intialArmor, intialHealth);

            AttackedAnimation attackedAnimation = Instantiate(gameManager.resourceManager.attackedAnimation);
            attackedAnimation.SetParameters(gameManager, damageDealingUnit.transform.position, this.transform.position);

        }

    }

    public void Death()
    {
        confirmDeath = true;
        OnDeath?.Invoke(this);

        if(!confirmDeath)
        {
            return;
        }

    }
}
