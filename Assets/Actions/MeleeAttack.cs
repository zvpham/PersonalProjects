using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : Action
{
    public static bool isCritical = false;
    public static int[] penetration = new int[3];
    // Start is called before the first frame update
    public static void Attack(Unit target, int toHitBonus, int armorPenetration, int damage)
    {
        if (hit(target, toHitBonus))
        {
            Damage(target, Penetrate(target, armorPenetration), damage);
        }
    }

    public static bool hit(Unit target, int toHitBonus)
    {
        isCritical = false;
        Debug.Log("WHY");
        int roll = Random.Range(0, 21);
        if (roll == 20)
        {
            isCritical = true;
            Debug.Log("Critical");
            return true;
        }
        if (roll + toHitBonus  + target.agilityMod >= target.dodgeValue)
        {
            return true;
        }
        Debug.Log("Miss");
        return false;
    }

    public static int Penetrate(Unit target, int armorPenetration)
    {
        int currentArmorPen = armorPenetration;
        int amountPen = 0;
        for (int i = 0; i < penetration.Length; i++)
        {
            penetration[i] = 0;
        }

        int roll;
        int penCheck = 0;
        while (true)
        {
            for(int i = 0; i < 3; i++)
            {
                roll = Random.Range(0, 11) - 2;
                penetration[i] += roll;
                if(roll == 8)
                {
                    Debug.Log("ReRoll");
                    i -= 1;
                }
                else
                {
                    penetration[i] += currentArmorPen;
                    if (penetration[i] > target.armorValue)
                    {
                        penCheck += 1;
                    }
                }
            }
            if (penCheck == 3)
            {
                Debug.Log("Hit Amount Increase");
                currentArmorPen -= 2;
                amountPen += 1;
            }
            else if (penCheck > 0)
            {
                amountPen += 1;
                return amountPen;
            }
            else
            {
                return amountPen;
            }
        }

    }

    public static void Damage(Unit target, int penAmount, int damage)
    {
        if(penAmount == 0)
        {
            Debug.Log("Couldn't Penetrate Armor");
            return;
        }

        if (isCritical)
        {
            for (int i = 0; i < penAmount; i++)
            {
                target.TakeDamage(damage * 2);
            }
            Debug.Log("You Crit");
        }
        else
        {
            for (int i = 0; i < penAmount; i++)
            {
                target.TakeDamage(damage);
            }
            Debug.Log("You hit for Damage");
        }

        if(target.health <= 0)
        {
            target.Death();
        }

    }

    public override void Activate(Unit self)
    {
        throw new System.NotImplementedException();
    }

    public override void PlayerActivate(Unit self)
    {
        throw new System.NotImplementedException();
    }
}