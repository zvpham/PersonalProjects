using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : Action
{
    public static bool isCritical = false;
    public static int[] penetration = new int[3];
    // Start is called before the first frame update

    public override int CalculateWeight(Unit self)
    {
        throw new System.NotImplementedException();
    }

    public static void Attack(Unit target, Unit attacker)
    {
        if (hit(target, attacker.toHitBonus))
        {
            Damage(target, attacker, Penetrate(target, attacker.armorPenetration), attacker.defaultMeleeDamage);
        }
    }

    public static bool hit(Unit target, int toHitBonus)
    {
        isCritical = false;
        int roll = Random.Range(0, 21);
        if (roll == 20)
        {
            isCritical = true;
            return true;
        }
        if (roll + toHitBonus  + target.agilityMod >= target.dodgeValue)
        {
            return true;
        }
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

    public static void Damage(Unit target, Unit attacker, int penAmount, FullDamage defaultMeleeDamage)
    {
        if(penAmount == 0)
        {
            return;
        }

        if (isCritical)
        {
            target.TakeDamage(attacker, defaultMeleeDamage, penAmount * 2);
        }
        else
        {
            target.TakeDamage(attacker, defaultMeleeDamage, penAmount);
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