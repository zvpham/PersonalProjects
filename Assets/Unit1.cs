/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class Unit : MonoBehaviour
{
    //[SerializeField]
    public Tilemap groundTilemap;

    //[SerializeField]
    public Tilemap collisionTilemap;
    public GameObject self;

    public EnemyData enemyData;


    public GameManager gameManager;
    public int index;

    public static Unit instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }



    public void TurnEnd()
    {
        enabled = false;
    }

    public void ChangeStr(int value)
    {
        enemyData.strength = value;
        if(enemyData.strength % 2 == 0)
        {
            enemyData.strengthMod = (enemyData.strength - 16) / 2;
        }
        else if ( enemyData.strength % 2 == 1 && enemyData.strength > 16) 
        {
            enemyData.strengthMod = (enemyData.strength - 16 - 1) / 2;
        }
        else
        {
            enemyData.strengthMod = (enemyData.strength - 16  + 1 ) / 2;
        }
        enemyData.armorPenetration = enemyData.strengthMod;

    }

    public void ChangeAgi(int value)
    {
        enemyData.speed = value;
        if (enemyData.speed % 2 == 0)
        {
            enemyData.speedMod = (enemyData.speed - 16) / 2;
        }
        else if (enemyData.speed % 2 == 1 && enemyData.speed > 16)
        {
            enemyData.speedMod = (enemyData.speed - 16 - 1) / 2;
        }
        else
        {
            enemyData.speedMod = (enemyData.speed - 16 + 1) / 2;
        }
        enemyData.toHitBonus = enemyData.speedMod;
        enemyData.dodgeValue = 6 + enemyData.speedMod;
    }

    public void ChangeEnd(int value)
    {
        enemyData.endurance = value;
        if (enemyData.endurance % 2 == 0)
        {
            enemyData.enduranceMod = (enemyData.endurance - 16) / 2;
        }
        else if (enemyData.endurance % 2 == 1 && enemyData.endurance > 16)
        {
            enemyData.enduranceMod = (enemyData.endurance - 16 - 1) / 2;
        }
        else
        {
            enemyData.enduranceMod = (enemyData.endurance - 16 + 1) / 2;
        }
        enemyData.armorValue = enemyData.enduranceMod;
        enemyData.health = enemyData.endurance + enemyData.enduranceMod; 
    }

    public void ChangeWis(int value)
    {
        enemyData.wisdom = value;
        if (enemyData.wisdom % 2 == 0)
        {
            enemyData.wisdomMod = (enemyData.wisdom - 16) / 2;
        }
        else if (enemyData.wisdom % 2 == 1 && enemyData.wisdom > 16)
        {
            enemyData.wisdomMod = (enemyData.wisdom - 16 - 1) / 2;
        }
        else
        {
            enemyData.wisdomMod = (enemyData.wisdom - 16 + 1) / 2;
        }
    }

    public void ChangeInt(int value)
    {
        enemyData.intelligence = value;
        if (enemyData.intelligence % 2 == 0)
        {
            enemyData.intelligenceMod = (enemyData.intelligence - 16) / 2;
        }
        else if (enemyData.intelligence % 2 == 1 && enemyData.intelligence > 16)
        {
            enemyData.intelligenceMod = (enemyData.intelligence - 16 - 1) / 2;
        }
        else
        {
            enemyData.intelligenceMod = (enemyData.intelligence - 16 + 1) / 2;
        }
    }

    public void ChangeCha(int value)
    {
        enemyData.charisma = value;
        if (enemyData.charisma % 2 == 0)
        {
            enemyData.charismaMod = (enemyData.charisma - 16) / 2;
        }
        else if (enemyData.charisma % 2 == 1 && enemyData.charisma > 16)
        {
            enemyData.charismaMod = (enemyData.charisma - 16 - 1) / 2;
        }
        else
        {
            enemyData.charismaMod = (enemyData.charisma - 16 + 1) / 2;
        }
    }

    public void TakeDamage(int value)
    {
        enemyData.health -= value;
    }

    public void Death()
    {
        for( int i = index + 1; i < gameManager.speeds.Count; i++ )
        {
            gameManager.scripts[i].index -= 1;
        }
        Debug.Log("Index" + index);
        foreach(int speed in gameManager.speeds)
        {
            Debug.Log("speed " + enemyData.speed);
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
*/