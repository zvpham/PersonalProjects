using Bresenhams;
using CodeMonkey.Utils;
using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.CanvasScaler;

public class Unit : MonoBehaviour, ISerializationCallbackReceiver
{
    //[SerializeField]
    //public Tilemap groundTilemap;

    //[SerializeField]
    //public Tilemap collisionTilemap;
    public GameObject self;

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

    public int wisdom = 16;
    public int wisdomMod;

    public int intelligence = 16;
    public int intelligenceMod;

    public int charisma = 16;
    public int charismaMod;

    public int closestEnemyIndex;
    public int visionRadius = 5;
    public bool clearLineOfSightToEnemy;

    public Faction faction;
    public List<Unit> enemyList = new List<Unit>();
    public List<Unit> allyList = new List<Unit>();

    public GameManager gameManager;
    public int index;

    public List<int> statusDuration;

    public TemplateHolder baseActionTemplate;

    public List<Action> baseActions;
    public List<Action> actions;
    public List<Status> statuses;
    public int hasLocationChangeStatus = 0;
    public static Unit instance;

    public Sprite originalSprite;
    public int spriteIndex = -1;

    public SoulItemSO[] physicalSouls = new SoulItemSO[3];
    public SoulItemSO[] mentalSouls = new SoulItemSO[3];


    public List<ActionTypes> _keysUnusableActionTypes = new List<ActionTypes> ();
    public List<int> _valuesUnusableActionTypes = new List<int> ();

    public Dictionary<ActionTypes, int> unusableActionTypes = new Dictionary<ActionTypes, int>();

    public List<GameObject> drops;

    public bool notOnHold = true;

    public bool inMelee;

    public event UnityAction<DamageTypes, int> OnDamage;
    public event UnityAction<ActionTypes[], ActionName> PerformedAction;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        baseActionTemplate = Instantiate(baseActionTemplate);
        foreach(Action templateAction in baseActionTemplate.Actions)
        {
            baseActions.Add(Instantiate(templateAction));
        }
    }

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


    public void DetermineAllyOrEnemy()
    {
        allyList.Clear();
        enemyList.Clear();
        foreach (Unit unit in gameManager.scripts)
        {
            if (unit.faction == this.faction)
            {
                allyList.Add(unit);
            }
            else
            {
                BresenhamsAlgorithm.PlotFunction plotFunction = CheckForBarriers;
                clearLineOfSightToEnemy = true;
                BresenhamsAlgorithm.Line((int) gameObject.transform.position.x, (int)gameObject.transform.position.y, (int) unit.self.transform.position.x, (int) unit.self.transform.position.y , plotFunction);
                if (clearLineOfSightToEnemy)
                {
                    enemyList.Add(unit);
                }
            }
        }
    }

    public void FindClosestEnemy()
    {

        float distance;
        float closestDistance = Vector3.Distance(gameObject.transform.position, enemyList[0].self.transform.position);
        closestEnemyIndex = 0;

        for (int i = 0; i < enemyList.Count; i++)
        {
            distance = Vector3.Distance(gameObject.transform.position, enemyList[i].self.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemyIndex = i;
            }
        }
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

    private bool CheckForBarriers(int x, int y, int numberMarkers)
    {
        Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(new Vector3(x, y, 0));
        if (numberMarkers <= visionRadius && !gameManager.collisionTilemap.HasTile(gridPosition))
        {
            return true;
        }
        clearLineOfSightToEnemy = false;
        return false;
    }

    public void TurnEnd()
    {
        if(actions.Count != 0)
        {
            foreach(Action action in actions)
            {
                if(action.currentCooldown > 0 && !action.isTurnActivated)
                {
                    if(action.isActiveAction && IsMatchingStatus(action.status[0]))
                    {
                        continue;
                    }
                    action.currentCooldown--;
                }
                if (action.isTurnActivated)
                {
                    action.isTurnActivated = false;
                }
            }
        }
        enabled = false;
    }

    public void CheckForStatusFields(Vector3 newPosition)
    {
        if (gameManager.StatusFields.Count > 0)
        {
            foreach (CreatedField field in gameManager.StatusFields)
            {
                try
                {
                    foreach (Status status in field.grid.GetGridObject(newPosition).statuses)
                    {
                        status.ApplyEffect(this);
                    }
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
                        if (gameManager.StatusFields.Count > 0)
                        {
                            foreach (CreatedField statusField in gameManager.StatusFields)
                            {
                                try
                                {
                                    if (statusField.grid.GetGridObject(self.transform.position).statuses.Contains(status))
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
        actions.Clear();
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
        index = gameManager.scripts.IndexOf(this);
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
        Instantiate(UtilsClass.CreateWorldText(value.ToString(), localPosition: self.transform.position).gameObject.AddComponent<DamageText>());
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
            Instantiate(UtilsClass.CreateWorldText(value.ToString(), localPosition: self.transform.position).gameObject.AddComponent<DamageText>());
        }
        if (health <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        if(drops.Count != 0)
        {
            foreach (GameObject drop in drops)
            {
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

        index = gameManager.scripts.IndexOf(this);
        if(index < gameManager.index)
        {
            gameManager.index -= 1;
        }
        gameManager.speeds.RemoveAt(index);
        gameManager.priority.RemoveAt(index);
        gameManager.scripts.RemoveAt(index);
        gameManager.enemies.RemoveAt(index - 1);
        gameManager.isLocationChangeStatus -= hasLocationChangeStatus;
        if(gameManager.grid.GetGridObject(self.transform.position) != null)
        {
            gameManager.grid.SetGridObject(self.transform.position, null);
        }
        else
        {
            gameManager.flyingGrid.SetGridObject(self.transform.position, null);
        }
        //gameManager.locations.RemoveAt(index);
        Destroy(this);
        Destroy(self);

    }
}