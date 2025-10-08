using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class Wall : MonoBehaviour
{
    public int wallIndex;
    public WallStates wallState;
    public int health;
    public int maxHealth;
    public int durability;
    public bool blockFlying;
    public bool blockMovement = true;
    public bool blockLineOfSight = true;

    public int spriteIndex = -1;

    public GameManager gameManager;

    public event UnityAction<DamageTypes, int> OnDamage;
    public event UnityAction OnDeath;
    public bool continueDeath;

    public virtual void Start()
    {
        int x = (int)(transform.position.x - gameManager.defaultGridPosition.x);
        int y = (int)(transform.position.y - gameManager.defaultGridPosition.y);
        gameManager.walls.Add(this);
        gameManager.initalRenderLocations.Add(new Vector3(x, y, wallIndex - 2));
        gameManager.ChangeWalls(transform.position, this);
    }

    public virtual bool IsSetPiece()
    {
        return false;
    }

    public void TakeDamage(Unit fromUnit, DamageTypes damageType, int value)
    {

        OnDamage?.Invoke(damageType, value);
        health -= value;
        Instantiate(UtilsClass.CreateWorldText(value.ToString(), localPosition: gameObject.transform.position).gameObject.AddComponent<DamageText>());

        if (health <= 0)
        {
            Death();
        }
    }
    public void TakeDamage(Unit fromUnit, FullDamage damageCalaculation, bool Melee, float damagePercentage = 1)
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

    public virtual void Death()
    {
        continueDeath = true;
        OnDeath?.Invoke();
        if (!continueDeath)
        {
            return;
        }

        int x, y;
        gameManager.obstacleGrid.GetXY(transform.position, out x, out y);

        //gameManager.scripts.RemoveAt(index);
        gameManager.ChangeWalls(gameObject.transform.position, null);
        gameManager.walls.Remove(this);

        for(int i = -1; i <= 1;  i++)
        {
            for(int  j = -1; j <= 1; j++)
            {
                Wall wall = gameManager.obstacleGrid.GetGridObject(x + i, y + j);
                if (wall != null && !wall.IsSetPiece()) 
                {
                    gameManager.RenderWall(x + i, y + j, wall.wallIndex - 2);
                }
            }
        }

        gameManager.FinalRender();

        Destroy(this);
        Destroy(gameObject);

    }

    public void Died()
    {
        OnDeath?.Invoke();
    }
}
