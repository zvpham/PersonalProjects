using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class Unit : MonoBehaviour, ISerializationCallbackReceiver
{
    //[SerializeField]
    //public Tilemap groundTilemap;

    //[SerializeField]
    //public Tilemap collisionTilemap;
    public GameObject self;

    public double quickness = 1;

    public int strength = 16;
    public int strengthMod;
    public int armorPenetration;
    public int damage;

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
        Debug.Log("Value: " + value);
        gameManager.speeds[index] *= value;
        Debug.Log("Speed: " + gameManager.speeds[index]);
        Debug.Log("Quickness: " + (int)(gameManager.priority[index] * value));
        gameManager.priority[index] = (int) (gameManager.priority[index] * value);
    }

    public void TakeDamage(int value)
    {
        health -= value;
    }

    public void ChangeSprite(Sprite sprite)
    {
        this.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public void Death()
    {
        if(drops.Count != 0)
        {
            foreach (GameObject drop in drops)
            {
                Debug.Log("SOMEONE DIED   A SDADAD" + drop);
                Instantiate(drop, this.transform.position, this.transform.rotation);
            }
        }
        for (int i = index + 1; i < gameManager.speeds.Count; i++)
        {
            gameManager.scripts[i].index -= 1;
        }
        Debug.Log("Index" + index);
        foreach (int speed in gameManager.speeds)
        {
            Debug.Log("Speed " + speed);
        }
        gameManager.speeds.RemoveAt(index);
        gameManager.priority.RemoveAt(index);
        gameManager.scripts.RemoveAt(index);
        gameManager.enemies.RemoveAt(index - 1);
        gameManager.locations.RemoveAt(index);
        Destroy(this);
        Destroy(self);

    }
}