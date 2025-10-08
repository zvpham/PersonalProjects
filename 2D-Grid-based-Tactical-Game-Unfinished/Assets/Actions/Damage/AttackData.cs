using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackData
{
    public List<Damage> allDamage = new List<Damage>();
    public bool ignoreArmour = false;
    public bool ignoreShield = false;
    public bool meleeContact = false;
    public float armorDamagePercentage;
    public List<Modifier> modifiers =  new List<Modifier>();
    public Unit originUnit;
    //List<Status> 

    public AttackData(Unit originUnit)
    {
        this.originUnit = originUnit;
        modifiers = new List<Modifier>();
    }

    public AttackData (List<Damage> allDamage, float armorDamagePercentage, Unit originUnit)
    {
        this.allDamage = allDamage;
        this.armorDamagePercentage = armorDamagePercentage;
        this.originUnit = originUnit;
        modifiers = new List<Modifier>();
    }

    public AttackData (AttackData oldAttackData)
    {
;        for(int i  = 0; i < oldAttackData.allDamage.Count; i++)
        {
            allDamage.Add(new Damage(oldAttackData.allDamage[i]));
        }
        Debug.Log("Num Old Damage count: " + oldAttackData.allDamage.Count + ", New Damage Count : " + allDamage.Count);
        ignoreArmour = oldAttackData.ignoreArmour;
        ignoreShield = oldAttackData.ignoreShield;
        meleeContact = oldAttackData.meleeContact;
        armorDamagePercentage = oldAttackData.armorDamagePercentage;

        for(int i = 0; i < oldAttackData.modifiers.Count; i++)
        {
            modifiers.Add(oldAttackData.modifiers[i]);
        }
        originUnit = oldAttackData.originUnit;
    }
    public void AddStatus(Status status)
    {
        status.ModifyAttackData(this);
    }

    public AttackData ApplyMinAndMaxDamageModifiers()
    {
        foreach (Damage damage in allDamage)
        {
            damage.minDamage = (int)(damage.minDamage * originUnit.GetMinimumDamageModifer());
            damage.maxDamage = (int)(damage.maxDamage * originUnit.GetMaximumDamageModifer());
            if (damage.minDamage > damage.maxDamage)
            {
                damage.maxDamage = damage.minDamage;
            }
        }

        return this;
    }

    public List<AttackDataUI> CalculateAttackData(Unit targetUnit, bool doesDamage)
    {
        AttackDataUI mainAttack = new AttackDataUI();
        if(doesDamage)
        {
            int adjustedMinimumDamage = 0;
            int adjustmedMaximumDamage = 0;
            for (int i = 0; i < allDamage.Count; i++)
            {
                adjustedMinimumDamage += allDamage[i].minDamage;
                adjustmedMaximumDamage += allDamage[i].maxDamage;
            }

            Tuple<int, int, List<AttackDataUI>> attackData = targetUnit.CalculateEstimatedDamage(adjustedMinimumDamage, adjustmedMaximumDamage, true);
            if (adjustedMinimumDamage > adjustmedMaximumDamage)
            {
                adjustmedMaximumDamage = adjustedMinimumDamage;
            }

            if (adjustmedMaximumDamage == 0)
            {
                mainAttack.data = 0.ToString();
            }
            else
            {
                mainAttack.data = adjustedMinimumDamage.ToString() + " - " + adjustmedMaximumDamage.ToString();
            }

            mainAttack.min = adjustedMinimumDamage;
            mainAttack.max = adjustmedMaximumDamage;
            mainAttack.attackDataType = attackDataType.Main;
            mainAttack.attackState = attackState.Benign;

            List<AttackDataUI> allAttackData = new List<AttackDataUI>() { mainAttack };
            for (int i = 0; i < attackData.Item3.Count; i++)
            {
                allAttackData.Add(attackData.Item3[i]);
            }

            if (armorDamagePercentage > 1)
            {
                AttackDataUI antiArmorAttackDataUI = new AttackDataUI();
                antiArmorAttackDataUI.data = "AntiArmor " + armorDamagePercentage.ToString() + "X";
                antiArmorAttackDataUI.attackDataType = attackDataType.Modifier;
                antiArmorAttackDataUI.attackState = attackState.Benificial;
                allAttackData.Add(antiArmorAttackDataUI);
            }

            return allAttackData;
        }
        else
        {
            Debug.Log("Num Modifiers: " + modifiers.Count);
            List<AttackDataUI> allAttackData = new List<AttackDataUI>();
            for (int i = 0; i < modifiers.Count; i++)
            {
                AttackDataUI newModifierUI = new AttackDataUI();
                newModifierUI.data = modifiers[i].modifierText;
                newModifierUI.attackState = modifiers[i].attackState;
                newModifierUI.attackDataType = attackDataType.Modifier;
                allAttackData.Add((newModifierUI));
            }
            return allAttackData;
        }
    }
}
