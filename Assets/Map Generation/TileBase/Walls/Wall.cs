using System;
using System.Collections;
using System.Collections.Generic;
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

    public void Death()
    {
        continueDeath = true;
        OnDeath?.Invoke();
        if (!continueDeath)
        {
            return;
        }

        //gameManager.scripts.RemoveAt(index);
        gameManager.ChangeWalls(gameObject.transform.position, null);
        Destroy(this);
        Destroy(gameObject);

    }
}
