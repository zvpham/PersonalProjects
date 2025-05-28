using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Inventory.Model;
using JetBrains.Annotations;
using System;
using Unity.VisualScripting;
using static UnityEngine.UI.CanvasScaler;

public class Unit : UnitSuperClass, IInititiave
{
    // Class Selection UI
    public bool inOverWorld = false;
    public bool isLoaded = false;

    public Sprite unitProfile;
    public GameObject unitSpriteRenderer;
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

    public int totalMoveSpeedModifier = 0;
    public int currentMoveSpeed = 18;
    public int moveSpeedPerMoveAction;
    public int moveSpeed = 3;

    public float damageResistence = 0f;
    public float shieldDamageResistence = 0f;

    public int x;
    public int y;

    public MeleeAttack MeleeAttack;
    public List<UnitActionData> actions = new List<UnitActionData>();

    public List<UnitPassiveData> passives = new List<UnitPassiveData>();
    public List<PassiveEffectArea> passiveEffects = new List<PassiveEffectArea>();
    
    public List<StatusData> statuses = new List<StatusData>();

    public int maxMajorActionsPoints = 2;
    public int maxMinorActionsPoints = 0;
    public int currentMajorActionsPoints = 0;
    public int currentMinorActionsPoints = 0;


    public List<Unit> selfInTheseUnitsThreatenedZones = new List<Unit>();

    public MoveModifier moveModifier;
    public int moveModifierPriority = -1;

    public UnitGroup group;

    public bool unitMovedThisTurn =  false;
    public bool forceEndTurn = false;
    public bool confirmDeath;
    public bool isDead = false;
    public bool confirmDamage;
    //AI Help Data
    public float targetValue;
    public List<AIUnitResponses> AIResponseToUnit = new List<AIUnitResponses>();

    public UnityAction onTurnStart;
    public UnityAction<Vector2Int, Vector2Int, Unit, bool> OnPositionChanged;
    public UnityAction<Unit> OnDeath;
    // Self Unit, Damage Dealing Unit, Damage, IsMelee, isFirstInstanceOfDamage
    public UnityAction<Unit, Unit, AttackData> PreDamage;
    public UnityAction<Unit, Unit, bool, bool> OnDamage;
    public UnityAction<Unit, Unit, AttackData> PostDamage;

    public CombatGameManager gameManager;

    public UnityAction<Action, TargetingSystem> OnSelectedAction;

    // [Unwalkable, BadWalkin, GoodWalkin]
    // AIStatus is for AI checking to see if a status will improve movement by causing passive areas to be ignored
    public List<List<PassiveEffectArea>> CalculuatePassiveAreas(Status AIStatus = null)
    {
        List<List<PassiveEffectArea>> classifiedPassives = new List<List<PassiveEffectArea>>() { new List<PassiveEffectArea>(),
        new List<PassiveEffectArea>(), new List<PassiveEffectArea>()};
        List<PassiveEffectArea> allPassiveAreas = gameManager.passiveAreas;
        for (int i = 0; i < allPassiveAreas.Count; i++)
        {
            PassiveAreaClassification classification = allPassiveAreas[i].passive.passive.passiveClassification;
            int classificationIndex = -1;
            switch(classification)
            {
                case (PassiveAreaClassification.EnemyUnwalkable):
                    if (allPassiveAreas[i].originUnit.team != this.team)
                    {
                        classificationIndex = 0;
                    }
                    break;
                case (PassiveAreaClassification.EnemyBadWalkIn):
                    if (allPassiveAreas[i].originUnit.team != this.team)
                    {
                        classificationIndex = 1;
                    }
                    break;
                case (PassiveAreaClassification.AllyWantToWalk):
                    if (allPassiveAreas[i].originUnit.team == this.team)
                    {
                        classificationIndex = 2;
                    }
                    break;
            }
            if(classificationIndex != -1 && CheckStatuses(null, allPassiveAreas[i].passive.passive, AIStatus))
            {
                classifiedPassives[classificationIndex].Add(allPassiveAreas[i]);
            }
        }
        return classifiedPassives;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!isLoaded)
        {
            if (unitClass != null)
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
                if (Item1.GetType() == typeof(EquipableAmmoSO))
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
            if (!inOverWorld && (!gameManager.testing || (gameManager.testing && gameManager.playing)))
            {
                GetReadyForCombat();
            }
            isLoaded = true;
        }
        else
        {
            List<UnitPassiveData> tempPassives = new List<UnitPassiveData>();
            passiveEffects.Clear();
            for(int i = 0; i < passives.Count; i++)
            {
                tempPassives.Add(passives[i]);
            }
            passives.Clear();
            for(int i = 0; i < tempPassives.Count; i++)
            {
                tempPassives[i].passive.AddPassive(this);
            }
        }
    }

    public void GetReadyForCombat()
    {
        if (moveModifier == null)
        {
            moveModifier = gameManager.resourceManager.moveModifiers[0];
        }
        if (group == null)
        {
            gameManager.allinitiativeGroups.Add(this);
        }

        switch (team)
        {
            case Team.Player:
                gameManager.playerTurn.playerUnits.Add(this);
                break;
            case Team.Team2:
                if (group == null)
                {
                    gameManager.AITurn1.unitSuperClasses.Add(this);
                }
                gameManager.AITurn1.units.Add(this);
                break;
            case Team.Team3:
                if (group == null)
                {
                    gameManager.AITurn2.unitSuperClasses.Add(this);
                }
                gameManager.AITurn2.units.Add(this);
                break;
            case Team.Team4:
                if (group == null)
                {
                    gameManager.AITurn3.unitSuperClasses.Add(this);
                }
                gameManager.AITurn3.units.Add(this);
                break;
        }

        CalculateTargetPrioritySelf();

        moveSpeedPerMoveAction = 18 + totalMoveSpeedModifier;

        gameManager.SetGridObject(this, transform.position);
        gameManager.units.Add(this);
        if (!gameManager.testing)
        {
             unitSpriteRenderer = gameManager.spriteManager.CreateSpriteRenderer(0, unitProfile, transform.position);
        }
        gameManager.StartCombat();

        gameManager.resourceManager.standardUnitActions.AddActions(this);
        gameManager.OnActivateAction += CheckPassives;
        currentHealth = maxHealth;
        gameManager.spriteManager.spriteGrid.GetXY(transform.position, out int xUnit, out int yUnit);
        x = xUnit;
        y = yUnit;
        MovePositions(this.transform.position, this.transform.position, true);
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
        return 1;
    }

    public void ChangeDexterity(int newDexterity)
    {
        dexterity =  newDexterity;
        moveSpeed = 3 + (newDexterity / 2);
    }

    public float GetMinimumDamageModifer()
    {
        return 1;
    }


    public void ChangeWeight(int newWeight)
    {
        currentWeight = newWeight;
        overWieght = false;
        if(currentWeight > highWeight)
        {
            overWieght = true;
            maxMajorActionsPoints = 1;
            maxMinorActionsPoints = 0;
        }
        else if(currentWeight > mediumWeight)
        {
            maxMajorActionsPoints = 2;
            maxMinorActionsPoints = 0;
        }
        else if (currentWeight > lowWeight)
        {
            maxMajorActionsPoints = 2;
            maxMinorActionsPoints = 1;
        }
        else
        {
            maxMajorActionsPoints = 2;
            maxMinorActionsPoints = 2;
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

    public void CalculateTargetPrioritySelf()
    {
        targetValue = powerLevel / .33f;
        targetValue += 1;
    }

    // ----------------------------------------------------------------------------
    // Turn Management/ Action Management
    public void StartTurn()
    {
        Debug.Log("Turn Start: " + team + ", " + this);
        onTurnStart?.Invoke();
        currentMajorActionsPoints = maxMajorActionsPoints;
        currentMoveSpeed = 0;
        CheckActionsDisabled();
        if (team == Team.Player)
        {
            gameManager.StartPlayerTurn(this);
        }
        else
        {
            gameManager.StartAITurn(this, team);
        }
    }

    public void ContinueAITurn()
    {
        gameManager.StartAITurn(this, team);    
    }

    public void CheckActionsDisabled()
    {
        for(int i = 0; i < actions.Count; i++)
        {
            actions[i].active = actions[i].action.CheckActionUsable(this);
        }
    }

    public void HandleSelectedAction (Action selectedAction, TargetingSystem actionTargetingSystem)
    {
        OnSelectedAction?.Invoke(selectedAction, actionTargetingSystem);
    }

    public void UseActionPoints(int usedActionPoints, bool minorActionsFirst = false)
    {

        //Debug.Log("current Major Action Points: " + currentMajorActionsPoints + "current minor Action Points: " + currentMinorActionsPoints + ", actionPoints Used" + usedActionPoints);
        if(team == Team.Player)
        {
            Debug.Log("Used Action Points: " + usedActionPoints);
        }
        
        if(minorActionsFirst)
        {
            if (usedActionPoints != 1)
            {
                Debug.LogError("bonus action first should only be used for actions that are just 1 bonus action");
            }
            else
            {
                if(currentMinorActionsPoints > 0)
                {
                    currentMinorActionsPoints -= 1;
                }
                else
                {
                    currentMajorActionsPoints -= 1;
                }
            }
        }
        else if(currentMajorActionsPoints > 0)
        {
            currentMajorActionsPoints -= 1;
            int remainingUsedActionpoints = usedActionPoints - 1;
            for(int i = 0; i < remainingUsedActionpoints; i++)
            {
                if (currentMinorActionsPoints > 0)
                {
                    currentMinorActionsPoints -= 1;
                }
                else
                {
                    currentMajorActionsPoints -= 1;
                }
                remainingUsedActionpoints -= 1;
            }
        }

        if (team == Team.Player)
        {
            Debug.Log("current ACtion Points: " + currentMajorActionsPoints);
        }

        if (currentMajorActionsPoints < 0 || currentMinorActionsPoints < 0)
        {
            Debug.LogError("Action Points when below 0, current Major Points: " + currentMajorActionsPoints + ", current minor points: " + currentMinorActionsPoints);
        }
        CheckActionsDisabled();
    }

    public void ActionsFinishedActivating()
    {
        if (team == Team.Player)
        {
            if (currentMajorActionsPoints <= 0)
            {
                if (!forceEndTurn && CanMove())
                {
                    gameManager.playerTurn.SelectAction(actions[0].action);
                    return;
                }
                Debug.Log("Player End Turn");
                EndTurn();
            }
            else
            {
                gameManager.playerTurn.SelectAction(gameManager.playerTurn.currentlySelectedAction);
            }
        }
        else
        {
            if (currentMajorActionsPoints <= 0)
            {
                if (!forceEndTurn && CanMove())
                {
                    ContinueAITurn();
                    return;
                }
                Debug.LogWarning    ("Ai End Turn");
                EndTurn();
            }
            else
            {
                ContinueAITurn();
            }
        }
    }

    public void CheckPassives(ActionData actionData)
    {
        for(int i = 0; i < passives.Count; i++)
        {
            passives[i].passive.ActivatePassive(this, actionData);
        }
    }

    // AI Status is for AI Statuses checking passiveFields so they can move in the best way
    public bool CheckStatuses(Action occuringAction, Passive occuringPassive, Status AIstatus = null)
    {
        bool actionOrPassiveContinue = true;
        for(int i = 0; i < statuses.Count; i++)
        {
            if(!statuses[i].status.ContinueEvent(occuringAction, occuringPassive))
            {
                actionOrPassiveContinue = false; 
                break;
            }
        }

        if(!actionOrPassiveContinue && AIstatus != null)
        {
            if (!AIstatus.ContinueEvent(occuringAction, occuringPassive))
            {
                Debug.Log("False");
                actionOrPassiveContinue = false;
            }
            else
            {
                Debug.Log("True");
            }
        }

        return actionOrPassiveContinue;
    }

    // Gets modifiers to actions - ex: fire bonus damage on melee attacks
    public void GetActionModifiers(Action action, AttackData attackData)
    {
        for(int i = 0; i < statuses.Count; i++)
        {
            statuses[i].status.ModifiyAction(action, attackData);
        }

        for(int i = 0; i < passives.Count; i++)
        {
            passives[i].passive.ModifiyAction(action, attackData);
        }
    }

    public void EndTurnAction()
    {
        AIActionData tempActionData = new AIActionData();
        tempActionData.unit = this;
        for(int i = 0; i < actions.Count; i++)
        {
            if(actions[i].action.GetType() == typeof(EndTurn))
            {
                actions[i].action.AIUseAction(tempActionData);
            }
        }
    }

    public void EndTurn()
    {
        forceEndTurn = false;
        for(int i = 0; i < statuses.Count; i++)
        {
            statuses[i].duration -= 1;
            if (statuses[i].duration <= 0)
            {
                statuses.RemoveAt(i);
                i--;
            }
        }

        if (group != null)
        {
            group.EndTurn(this);
        }
        else
        {
            switch (team)
            {
                case (Team.Player):
                    gameManager.playerTurn.TurnEnd();
                    break;
                case (Team.Team2):
                    gameManager.AITurn1.AIEndTurn(new List<Unit>() { this });
                    break;
                case (Team.Team3):
                    gameManager.AITurn2.AIEndTurn(new List<Unit>() { this });
                    break;
                case (Team.Team4):
                    gameManager.AITurn3.AIEndTurn(new List<Unit>() { this });
                    break;
            }
            gameManager.TurnEnd(this);
        }
    }

    public void MovePositions(Vector3 originalPosition, Vector3 newPosition, bool finalMove)
    {
        Vector2Int oldPosition = new Vector2Int(x, y);
        gameManager.SetGridObject(null, originalPosition);
        gameManager.SetGridObject(this, newPosition);
        gameManager.spriteManager.spriteGrid.GetXY(newPosition, out int unitX, out int unitY);
        x = unitX;
        y = unitY;
        this.transform.position = newPosition;
        OnPositionChanged?.Invoke(oldPosition,  new Vector2Int(x, y), this, finalMove);
    }

    public bool LineOfSight(Vector2Int unitPosition, Vector2Int targetPosition)
    {
        List<List<Vector2Int>> sightLines = gameManager.grid.LineOfSightSuperCover(unitPosition, targetPosition);
        int originalElevation = gameManager.spriteManager.elevationOfHexes[unitPosition.x, unitPosition.y];
        bool reachedEndHex = false;
        for (int i = 0; i < sightLines.Count; i++)
        {
            reachedEndHex = true;
            for (int j = 0; j < sightLines[i].Count - 1; j++)
            {
                Vector2Int currentHex = sightLines[i][j];
                if (gameManager.spriteManager.elevationOfHexes[currentHex.x, currentHex.y] > originalElevation)
                {
                    reachedEndHex = false;
                    break;
                }
            }
            if (reachedEndHex)
            {
                break;
            }
        }

        return reachedEndHex;
    }
    //-----------------------------------------------------
    //Damage Calculation and Death

    public List<AttackDataUI> DisplayCalculatedEstimatedDamage(AttackData attackData, List<Damage> allDamage, bool ignoreShield)
    {
        float combatDamageResistenceValue = damageResistence;
        List<AttackDataUI> unitAttackedData = new List<AttackDataUI>();

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
        for(int i = 0; i < allDamage.Count; i++)
        {
            int minDamage = allDamage[i].minDamage;
            int maxDamage = allDamage[i].maxDamage;
            allDamage[i].minDamage = minDamage - (int)(minDamage * combatDamageResistenceValue);
            allDamage[i].maxDamage = maxDamage - (int)(maxDamage * combatDamageResistenceValue);
        }
        return unitAttackedData;
    }

    // 
    public Modifier GetCalculatedEstimatedDamage(AttackData attackData, List<Damage> allDamage, bool ignoreShield)
    {
        float combatDamageResistenceValue = damageResistence;
        Modifier tempShield =  null;

        if (!ignoreShield && shieldDamageResistence != 0)
        {
            combatDamageResistenceValue += shieldDamageResistence;
            tempShield = new Modifier();
            tempShield.value = shieldDamageResistence;
            tempShield.modifierText = "Shielded " + "-" + shieldDamageResistence;
            tempShield.attackState = attackState.Benediction;
        }

        if (damageResistence > 1)
        {
            combatDamageResistenceValue = 1f;
        }
        for (int i = 0; i < allDamage.Count; i++)
        {
            int minDamage = allDamage[i].minDamage;
            int maxDamage = allDamage[i].maxDamage;
            allDamage[i].minDamage = minDamage - (int)(minDamage * combatDamageResistenceValue);
            allDamage[i].maxDamage = maxDamage - (int)(maxDamage * combatDamageResistenceValue);
        }
        return tempShield;
    }

    // This is for the AI to help determine the value of doing an action
    //Health Damage, Armour Damage
    public Tuple<int, int, List<Status>> GetEstimatedDamageValues(AttackData attackData)
    {
        int totalDamage = 0;
        float totalModifer = 1;
        attackData.ApplyMinAndMaxDamageModifiers();

        Modifier resistances = this.GetCalculatedEstimatedDamage(attackData, attackData.allDamage, true);

        for (int k = 0; k < attackData.modifiers.Count; k++)
        {
            totalModifer = totalModifer * attackData.modifiers[k].value;
        }

       // Debug.Log("Total Modifier: " + totalModifer);
        for (int k = 0; k < attackData.allDamage.Count; k++)
        {
            Damage currentDamage = attackData.allDamage[k];
            //Debug.Log("Estimated Current Damage: " +  currentDamage.minDamage + ", " + currentDamage.maxDamage);
            int tempDamage = (currentDamage.minDamage + currentDamage.maxDamage) / 2;
            tempDamage = (int)(tempDamage * totalModifer);
            totalDamage += tempDamage;
        }
        //Debug.Log("Estimated Total Damage: " + totalDamage);

        // 
        List<Status> finalApplicableStatuses = new List<Status>();
        Tuple<int, int> finalDamageValues = TempApplyDamage(totalDamage, attackData.armorDamagePercentage, attackData.ignoreArmour);
        return new Tuple<int, int, List<Status>>(finalDamageValues.Item1, finalDamageValues.Item2, finalApplicableStatuses);
    }

    //Do Resistences before This Step
    //Health Damage, Armour Damage
    public Tuple<int, int> TempApplyDamage(int damage, float armourEffectPercentage, bool ignoreArmour)
    {
        int healthDamage = 0;
        int armourDamage = 0;
        if (ignoreArmour)
        {
            healthDamage = damage;
        }
        else
        {
            int currentDamge = damage;
            currentDamge =  (int) (currentArmor -  (damage * armourEffectPercentage));
            if(currentDamge < 0)
            {
                currentDamge = (int)((currentDamge * -1) / armourEffectPercentage);
                armourDamage = currentArmor;
                healthDamage = currentDamge;
            }
            else
            {
                armourDamage = (int)(damage * armourEffectPercentage);
            }
        }
        return new Tuple<int, int>(healthDamage, armourDamage);
    }

    // This is old Replace as soon as possible
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
   

    public void TakeDamage(Unit unitDealingDamage, AttackData attackData, bool firstInstanceOfDamageSetUp)
    {
        int seed = System.DateTime.Now.Millisecond;
        UnityEngine.Random.InitState(seed);

        float combatDamageResistenceValue = damageResistence;

        if (!attackData.ignoreShield)
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
        confirmDamage = true;
        PreDamage?.Invoke(this, unitDealingDamage, attackData);
        OnDamage?.Invoke(this, unitDealingDamage, attackData.meleeContact, firstInstanceOfDamage);

        if(!confirmDamage)
        {
            return;
        }

        // Seperated Becuase I am unsure If I want different damage Type to do different Things
        for (int i = 0; i < attackData.allDamage.Count; i++)
        {
            Damage damage = attackData.allDamage[i];
            int damageValue = UnityEngine.Random.Range(damage.minDamage, damage.maxDamage + 1);
            damageValue = damageValue - (int)(damageValue * combatDamageResistenceValue);
            firstInstanceOfDamage = false;

            if (attackData.ignoreArmour)
            {
                currentHealth -= damageValue;
                Debug.Log("Health Damage Taken: " + damageValue);
            }
            else
            {
                currentArmor -= damageValue;
                if (currentArmor < 0)
                {
                    Debug.Log("Health Damage Taken: " + (currentArmor * -1));
                    currentHealth += currentArmor;
                    currentArmor = 0;
                }
            }
        }

        Debug.Log("Damage dealt: ");
        PostDamage?.Invoke(this, unitDealingDamage, attackData);

        if (currentHealth <= 0)
        {
            Death();
        }
        else
        {
            Debug.Log("damage");
            DamageAnimation damageAnimation = Instantiate(gameManager.resourceManager.damageAnimation);
            damageAnimation.SetParameters(combatAttackUi, this, intialArmor, intialHealth);

            AttackedAnimation attackedAnimation = Instantiate(gameManager.resourceManager.attackedAnimation);
            attackedAnimation.SetParameters(this, unitDealingDamage.transform.position, this.transform.position);

        }
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
                Debug.Log("Health Damage Taken: " +  damageValue);
            }
            else
            {
                currentArmor -= damageValue;
                if (currentArmor < 0)
                {
                    Debug.Log("Health Damage Taken: " + currentArmor);
                    currentHealth += currentArmor;
                    currentArmor = 0;
                }
            }
        }

        if(currentHealth <= 0)
        {
            if(combatAttackUi != null)
            {
                combatAttackUi.readyToReset = true;
            }

            gameManager.spriteManager.ResetCombatAttackUI();
            Death();
        }
        else
        {
            Debug.Log("damage");
            DamageAnimation damageAnimation = Instantiate(gameManager.resourceManager.damageAnimation);
            damageAnimation.SetParameters(combatAttackUi, this, intialArmor, intialHealth);

            AttackedAnimation attackedAnimation = Instantiate(gameManager.resourceManager.attackedAnimation);
            attackedAnimation.SetParameters(this, damageDealingUnit.transform.position, this.transform.position);

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

        Debug.Log("Unit Died: " + name);

        // Death Confirmed
        gameManager.UnitDied(this);
        DeathAnimation deathAnimation = Instantiate(gameManager.resourceManager.deathAnimation);
        deathAnimation.SetParameters(this);
        Destroy(this);
        Destroy(this.gameObject);
        isDead = true;
        gameManager.OnActivateAction -= CheckPassives;
    }

    //-----------------------------------------------------
    //Helpful Functions

    public bool CanMove()
    {
        if(team != Team.Player)
        {
            bool canMove = false;
            AIActionData actionData = new AIActionData();
            actionData.unit = this;
            for (int i = 0; i < actions.Count; i++)
            {
                if ((actions[i].action.actionTypes.Contains(ActionType.Movement)))
                {
                    canMove =  actions[i].action.CanMove(actionData);
                    if(canMove)
                    {
                        return true;
                    }
                }
            }
        }
        else
        {
            // Check to See if Player Can Still Move;
            if (currentMoveSpeed > 0)
            {
                List<DijkstraMapNode> neighborHexes = gameManager.map.getGrid().GetGridObjectsInRing(x, y, 1);
                for (int i = 0; i < neighborHexes.Count; i++)
                {
                    DijkstraMapNode neighborNode = neighborHexes[i];
                    if (gameManager.spriteManager.elevationOfHexes[neighborNode.y, neighborNode.x] <= currentMoveSpeed)
                    {
                        //Action[0] is move Action
                        gameManager.playerTurn.SelectAction(actions[0].action);
                        Debug.Log("Can Move");
                        return true;
                    }
                }
            }
        }

        Debug.Log("Can't Move: " +  currentMoveSpeed);
        return false;
    }

    public bool CanUnitMove(Unit movingUnit, int numActionPoints, int amountMoved, int currentMoveSpeed, int moveCostOverride = -1,
       int[,] moveCostGridOveride = null)
    {
        if (numActionPoints >= amountMoved + 1)
        {
            return true;
        }

        if (numActionPoints < 0)
        {
            return false;
        }

        int[,] moveCostGrid;
        if (moveCostGridOveride == null)
        {
            moveCostGrid = movingUnit.gameManager.spriteManager.elevationOfHexes;
        }
        else
        {
            moveCostGrid = moveCostGridOveride;
        }

        if (currentMoveSpeed > 0)
        {
            if (moveCostOverride == -1)
            {
                List<DijkstraMapNode> neighborHexes = movingUnit.gameManager.map.getGrid().GetGridObjectsInRing(movingUnit.x,
                    movingUnit.y, 1);
                for (int i = 0; i < neighborHexes.Count; i++)
                {
                    DijkstraMapNode neighborNode = neighborHexes[i];
                    if (moveCostGrid[neighborNode.y, neighborNode.x] <= currentMoveSpeed)
                    {
                        return true;
                    }
                }
            }
            else if (moveCostOverride <= currentMoveSpeed)
            {
                return true;
            }
        }
        return false;
    }

    public int GetMoveActionIndex()
    {
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i].action.GetType() == typeof(Move))
            {
                return i;
            }
        }
        return -1;
    }
}
