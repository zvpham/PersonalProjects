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
    public bool affectFlying;

    public int spriteIndex = -1;

    public GameManager gameManager;

    public event UnityAction<DamageTypes, int> OnDamage;
    public event UnityAction OnDeath;
    public bool continueDeath;

    public void Death()
    {
        continueDeath = true;
        OnDeath?.Invoke();
        if (!continueDeath)
        {
            return;
        }

        //gameManager.scripts.RemoveAt(index);
        if (gameManager.grid.GetGridObject(gameObject.transform.position) != null)
        {
            gameManager.grid.SetGridObject(gameObject.transform.position, null);
        }
        else
        {
            gameManager.flyingGrid.SetGridObject(gameObject.transform.position, null);
        }
        Destroy(this);
        Destroy(gameObject);

    }
}
