using Bresenhams;
using CodeMonkey.Utils;
using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.CanvasScaler;

public class Unit : MonoBehaviour, ISerializationCallbackReceiver
{
    public int unitResourceManagerIndex;

    public double quickness = 1;
    public float timeFlow = 1f;

    public int strength = 16;
    public int strengthMod;
    public int armorPenetration;

    public int agility = 16;
    public int agilityMod;
    public int dodgeValue;
    public int toHitBonus;

    public int endurance = 16;
    public int enduranceMod;
    public int armorValue;
    public int health;
    public int maxHealth;

    public int wisdom = 16;
    public int wisdomMod;

    public int intelligence = 16;
    public int intelligenceMod;

    public int charisma = 16;
    public int charismaMod;

    public int closestEnemyIndex;
    public int visionRadius = 5;
    public bool clearLineOfSightToEnemy;

    public float friendlyFireMultiplier = 0.5f;

    public Faction faction;
    public List<Unit> enemyList = new List<Unit>();
    public List<Unit> allyList = new List<Unit>();

    public bool inPeripheralGameManager = false;

    public GameManager gameManager;
    public int index;

    public List<int> statusDuration;

    public TemplateHolder baseActionTemplate;
    public List<Action> baseActions;

    public List<Action> actions;
    public List<ActionName> actionNames = new List<ActionName>();
    public List<int> actionCooldowns = new List<int>();

    public bool chasing;
    public List<ChaseAction> chaseActions;
    public Vector3 lastKnownEnemyLocation;
    public Vector3 locationUnitIsChasing;

    public List<Sense> senses = new List<Sense>();
    
    // These are only to be set on Load and Read after actions instanted
    // For Updating action cooldowns to be cooldowns in the saveData
    public List<ActionName> actionNamesForCoolDownOnLoad;
    public List<int> currentCooldownOnLoad;

    public List<Status> statuses;
    public int hasLocationChangeStatus = 0;


    public Sprite originalSprite;
    public int spriteIndex = -1;

    public List<SoulItemSO> physicalSouls = new List<SoulItemSO>();
    public List<SoulItemSO> mentalSouls = new List<SoulItemSO>();


    public List<ActionTypes> _keysUnusableActionTypes = new List<ActionTypes> ();
    public List<int> _valuesUnusableActionTypes = new List<int> ();

    public Dictionary<ActionTypes, int> unusableActionTypes = new Dictionary<ActionTypes, int>();

    public List<GameObject> drops;

    public unitForcedMovementPathData forcedMovementPathData = new unitForcedMovementPathData(
        new List<Vector3>(), 0, 0, 0, 0, 0);

    public bool notOnHold = true;
    public bool inMiddleMap = true;

    public bool inMelee;
    public bool continueDeath = true;

    public event UnityAction<DamageTypes, int> OnDamage;
    public event UnityAction<ActionTypes[], ActionName> PerformedAction;
    public event UnityAction OnDeath;
    public event UnityAction onTurnStart;

    public void OnBeforeSerialize()
    {
        _keysUnusableActionTypes.Clear();
        _valuesUnusableActionTypes.Clear();

        foreach (var kvp in unusableActionTypes)
        {
            _keysUnusableActionTypes.Add(kvp.Key);
            _valuesUnusableActionTypes.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        unusableActionTypes = new Dictionary<ActionTypes, int>();

        for (int i = 0; i != Mathf.Min(_keysUnusableActionTypes.Count, _valuesUnusableActionTypes.Count); i++)
            unusableActionTypes.Add(_keysUnusableActionTypes[i], _valuesUnusableActionTypes[i]);
    }

    void OnGUI()
    {
        foreach (var kvp in unusableActionTypes)
            GUILayout.Label("Key: " + kvp.Key + " value: " + kvp.Value);
    }

    public void OnTurnStart()
    {
        onTurnStart?.Invoke();
    }

    public void UseSenses(bool usePeripheralManagerSenses)
    {
        allyList.Clear();
        enemyList.Clear();
        if (usePeripheralManagerSenses)
        {
            for(int i = 0; i < senses.Count; i++)
            {
                senses[i].PeripheralManagerDetectUnits(this);
            }
        }
        else
        {
            for (int i = 0; i < senses.Count; i++)
            {
                senses[i].DetectNearbyUnits(this);
            }
        }
        
    }

    // There Should be enemies in enemyList if called
    public void FindClosestEnemy()
    {
        float distance;
        float closestDistance = Vector3.Distance(gameObject.transform.position, enemyList[0].gameObject.transform.position);
        closestEnemyIndex = 0;

        for (int i = 0; i < enemyList.Count; i++)
        {
            distance = Vector3.Distance(gameObject.transform.position, enemyList[i].gameObject.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemyIndex = i;
            }
        }
        lastKnownEnemyLocation = enemyList[closestEnemyIndex].gameObject.transform.position;
    }

    public void IsInMelee()
    {
        Unit unit;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                unit = gameManager.grid.GetGridObject((int)gameObject.transform.position.x + j, (int)gameObject.transform.position.y + i);

                if (unit != null && enemyList.Contains(unit))
                {
                   inMelee = true;
                   return;
                }
            }
        }
        inMelee = false;
    }

    public void TurnEnd()
    {
        if(actions.Count != 0)
        {
            foreach(Action action in actions)
            {
                if(action.currentCooldown > 0)
                {
                    if(action.isActiveAction && IsMatchingStatus(action.status[0]))
                    {
                        continue;
                    }
                    action.currentCooldown--;
                }
            }
        }
        onTurnEndPlayer();
        enabled = false;
    }

    public virtual void onTurnEndPlayer()
    {

    }

    public virtual void ActivateTargeting()
    {
        notOnHold = false;
    }

    public virtual void DeactivateTargeting()
    {
        notOnHold = true;
    }

    public void CheckForStatusFields(Vector3 newPosition)
    {
        if (gameManager.createdFields.Count > 0)
        {
            foreach (CreatedField field in gameManager.createdFields)
            {
                try
                {
                    field.grid.GetGridObject(newPosition).ApplyObject(this);
                }
                catch
                {
                }
            }
        }

        if (statuses.Count > 0)
        {
            try
            {
                foreach (Status status in statuses)
                {
                    bool foundMatchingStatus = false;
                    if (status.isFieldStatus)
                    {
                        if (gameManager.createdFields.Count > 0)
                        {
                            foreach (CreatedField statusField in gameManager.createdFields)
                            {
                                try
                                {
                                    if (statusField.grid.GetGridObject(gameObject.transform.position).CheckStatus(status))
                                    {
                                        foundMatchingStatus = true;
                                        break;
                                    }
                                }
                                catch
                                {

                                }
                            }
                            if (!foundMatchingStatus)
                            {
                                Debug.Log("Testing");
                                status.RemoveEffect(this);
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }
    }

    public bool IsMatchingStatus(Status actionStatus)
    {
        foreach (Status status in statuses)
        {
            if (actionStatus == status)
            {
                return true;
            }
        }
        return false;
    }

   public void HandlePerformActions(ActionTypes[] actionTypes, ActionName actionName)
    {
        PerformedAction?.Invoke(actionTypes, actionName);
    }

    public void UpdateActions()
    {
        for(int i = 0; i < actions.Count; i++)
        {
            actionNames.Add(actions[i].actionName);
            actionCooldowns.Add(actions[i].currentCooldown);
        }

        actions.Clear();
        chaseActions.Clear();
        chaseActions = new List<ChaseAction> ();
        actions = new List<Action>();
        foreach(SoulItemSO phyiscalSoul in physicalSouls)
        {
            if(phyiscalSoul != null)
            {
                phyiscalSoul.AddPhysicalSoul(this);
            }
        }

        foreach (SoulItemSO mentalSoul in mentalSouls)
        {
            if (mentalSoul != null)
            {
                mentalSoul.AddPhysicalSoul(this);
            }
        }

        for(int i  = 0; i < actions.Count; i++)
        {
            int actionIndex = actionNames.IndexOf(actions[i].actionName);
            if(actionIndex == -1)
            {
                actions[i].currentCooldown = 0;
            }
            else
            {
                actions[i].currentCooldown = actionCooldowns[actionIndex];
            }
        }

        for(int i = 0; i < actions.Count; i++)
        {
            if (actions[i].isChaseAction)
            {
                chaseActions.Add((ChaseAction) actions[i]);
            }
        }

        for (int i = 0; i < baseActions.Count; i++)
        {
            if (baseActions[i].isChaseAction)
            {
                chaseActions.Add((ChaseAction) baseActions[i]);
            }
        }

        actionNames.Clear();
        actionCooldowns.Clear();
    }

    public void ChangeStr(int value)
    {
        strength += value;
        if (strength % 2 == 0)
        {
            strengthMod = (strength - 16) / 2;
        }
        else if (strength % 2 == 1 && strength > 16)
        {
            strengthMod = (strength - 16 - 1) / 2;
        }
        else
        {
            strengthMod = (strength - 16 + 1) / 2;
        }
        armorPenetration = strengthMod;

    }

    public void ChangeAgi(int value)
    {
        agility += value;
        if (agility % 2 == 0)
        {
            agilityMod = (agility - 16) / 2;
        }
        else if (agility % 2 == 1 && agility > 16)
        {
            agilityMod = (agility - 16 - 1) / 2;
        }
        else
        {
            agilityMod = (agility - 16 + 1) / 2;
        }
        toHitBonus = agilityMod;
        dodgeValue = 6 + agilityMod;
    }

    public void ChangeEnd(int value)
    {
        endurance += value;
        if (endurance % 2 == 0)
        {
            enduranceMod = (endurance - 16) / 2;
        }
        else if (endurance % 2 == 1 && endurance > 16)
        {
            enduranceMod = (endurance - 16 - 1) / 2;
        }
        else
        {
            enduranceMod = (endurance - 16 + 1) / 2;
        }
        armorValue = enduranceMod;
        health = endurance + enduranceMod;
    }

    public void ChangeWis(int value)
    {
        wisdom += value;
        if (wisdom % 2 == 0)
        {
            wisdomMod = (wisdom - 16) / 2;
        }
        else if (wisdom % 2 == 1 && wisdom > 16)
        {
            wisdomMod = (wisdom - 16 - 1) / 2;
        }
        else
        {
            wisdomMod = (wisdom - 16 + 1) / 2;
        }
    }

    public void ChangeInt(int value)
    {
        intelligence += value;
        if (intelligence % 2 == 0)
        {
            intelligenceMod = (intelligence - 16) / 2;
        }
        else if (intelligence % 2 == 1 && intelligence > 16)
        {
            intelligenceMod = (intelligence - 16 - 1) / 2;
        }
        else
        {
            intelligenceMod = (intelligence - 16 + 1) / 2;
        }
    }

    public void ChangeCha(int value)
    {
        charisma += value;
        if (charisma % 2 == 0)
        {
            charismaMod = (charisma - 16) / 2;
        }
        else if (charisma % 2 == 1 && charisma > 16)
        {
            charismaMod = (charisma - 16 - 1) / 2;
        }
        else
        {
            charismaMod = (charisma - 16 + 1) / 2;
        }
    }

    public void ChangeQuickness(double value)
    {
        index = gameManager.units.IndexOf(this);
        gameManager.speeds[index] *= value;
        /*
        Debug.Log("Value: " + value);
        Debug.Log("Speed: " + gameManager.speeds[index]);
        Debug.Log("Priority: " + (int)(gameManager.priority[index] * value));
        */
        gameManager.priority[index] = (int) (gameManager.priority[index] * value);
    }

    public void ChangeTimeFlow(float value)
    {
        timeFlow *= value;  
        ChangeQuickness(value);
        foreach (Status status in statuses)
        {
            status.ChangeQuickness(value);
        }
    }

    public void ChangeSprite(Sprite sprite)
    {
        this.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public bool ContainsMatchingUnusableActionType(int i, bool isBaseAction)
    {
        if(unusableActionTypes.Count <= 0)
        {
            return false;
        }

        if (isBaseAction)
        {
            if (baseActions[i].actionType.Length != 0)
            {
                foreach (ActionTypes actionType in baseActions[i].actionType)
                {
                    if (unusableActionTypes.ContainsKey(actionType))
                    {
                        //Debug.Log("Can't Use Action" + baseActions[i].actionName.ToString());
                        return true;
                    }
                }
            }
            //Debug.Log("Can Use Action" + baseActions[i].actionName.ToString());
            return false;
        }
        else
        {
            if (actions[i].actionType.Length != 0)
            {
                foreach (ActionTypes actionType in actions[i].actionType)
                {
                    if (unusableActionTypes.ContainsKey(actionType))
                    {
                        Debug.Log("Can't Use Action" + actions[i].actionName.ToString());
                        return true;
                    }
                }
            }
            //Debug.Log("Can Use Action" + baseActions[i].actionName.ToString());
            return false;
        }
    }

    public void TakeDamage(DamageTypes damageType, int value)
    {
        OnDamage?.Invoke(damageType, value);
        health -= value;
        Instantiate(UtilsClass.CreateWorldText(value.ToString(), localPosition: gameObject.transform.position).gameObject.AddComponent<DamageText>());
        if (health <= 0)
        {
            Death();
        }
    }

    public void TakeDamage(FullDamage damageCalaculation, float damagePercentage = 1)
    {
        int value = 0;
        foreach (Tuple<DamageTypes, int> damage in damageCalaculation.RollForDamage())
        {
            value = (int)(damage.Item2 * damagePercentage);
            OnDamage?.Invoke(damage.Item1, value);
            health -= value;
            Instantiate(UtilsClass.CreateWorldText(value.ToString(), localPosition: gameObject.transform.position).gameObject.AddComponent<DamageText>());
        }
        if (health <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        continueDeath = true;
        OnDeath?.Invoke();
        if(!continueDeath)
        {
            return;
        }

        if(drops.Count != 0)
        {
            foreach (GameObject drop in drops)
            {
                if (drop == null)
                {
                    continue;
                }
                Instantiate(drop, this.transform.position, this.transform.rotation);
            }
        }

        if(statuses.Count != 0)
        {
            for(int i = 0; i < statuses.Count; i++)
            {
                statuses[i].RemoveStatusPreset(this);
                i--;
            }
        }

        int expectedLocationIndex = gameManager.unitWhoHaveLocationChangeStatus.IndexOf(this);
        if (expectedLocationIndex != -1)
        {
            gameManager.unitWhoHaveLocationChangeStatus.RemoveAt(expectedLocationIndex);
            gameManager.expectedLocationChangeList.RemoveAt(expectedLocationIndex);

        }

        index = gameManager.units.IndexOf(this);
        if(index < gameManager.index)
        {
            gameManager.index -= 1;
        }
        gameManager.speeds.RemoveAt(index);
        gameManager.priority.RemoveAt(index);
        gameManager.units.RemoveAt(index);
        gameManager.isLocationChangeStatus -= hasLocationChangeStatus;
        if(gameManager.grid.GetGridObject(gameObject.transform.position) != null)
        {
            gameManager.ChangeUnits(gameObject.transform.position, null);
        }
        else
        {
            gameManager.ChangeUnits(gameObject.transform.position, null, true);
        }
        Destroy(this);
        Destroy(gameObject);

    }
}