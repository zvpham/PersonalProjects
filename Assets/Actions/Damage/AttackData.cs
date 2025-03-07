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
    public List<Modifier> modifiers;
    public Unit originUnit;
    //List<Status> 

    public AttackData (List<Damage> allDamage, float armorDamagePercentage, Unit originUnit)
    {
        this.allDamage = allDamage;
        this.armorDamagePercentage = armorDamagePercentage;
        this.originUnit = originUnit;
        modifiers = new List<Modifier>();
    }

    public List<AttackDataUI> DisplayCalculatedAttackData(Unit targetUnit)
    {
        List<Damage> tempAllDamage = new List<Damage>();
        foreach (Damage damage in allDamage)
        {
            Damage tempDamage = new Damage();
            tempDamage.minDamage = (int)(damage.minDamage * originUnit.GetMinimumDamageModifer());
            tempDamage.maxDamage = (int)(damage.maxDamage * originUnit.GetMaximumDamageModifer());
            tempDamage.damageType= damage.damageType;
            if (tempDamage.minDamage > tempDamage.maxDamage)
            {
                tempDamage.maxDamage = tempDamage.minDamage;
            }
            tempAllDamage.Add(tempDamage);
        }

        List<AttackDataUI> AttackDisplayData =  targetUnit.DisplayCalculatedEstimatedDamage(this, tempAllDamage, true);

        bool foundDamageValue = false;
        for (int i = 0; i < tempAllDamage.Count; i++)
        {
            if (tempAllDamage[i].maxDamage == 0)
            {
                tempAllDamage.RemoveAt(i);
                i--;
            }
            else
            {
                foundDamageValue = true;
            }
        }

        List<AttackDataUI> allAttackData = new List<AttackDataUI>();
        if (foundDamageValue)
        {
            foreach (Damage damage in tempAllDamage)
            {
                AttackDataUI tempAttackDataUI = new AttackDataUI();
                tempAttackDataUI.data = damage.minDamage.ToString() + " - " + damage.maxDamage.ToString();
                tempAttackDataUI.min = damage.minDamage;
                tempAttackDataUI.max = damage.maxDamage;
                tempAttackDataUI.attackDataType = attackDataType.Main;
                tempAttackDataUI.attackState = attackState.Benign;
                allAttackData.Add(tempAttackDataUI);
            }
        }
        else
        {
            AttackDataUI tempAttackDataUI = new AttackDataUI();
            tempAttackDataUI.data = 0.ToString();
            tempAttackDataUI.min = 0;
            tempAttackDataUI.max = 0;
            tempAttackDataUI.attackDataType = attackDataType.Main;
            tempAttackDataUI.attackState = attackState.Benign;
            allAttackData.Add(tempAttackDataUI);
        }

        if (armorDamagePercentage > 1)
        {
            AttackDataUI antiArmorAttackDataUI = new AttackDataUI();
            antiArmorAttackDataUI.data = "AntiArmor " + armorDamagePercentage.ToString() + "X";
            antiArmorAttackDataUI.attackDataType = attackDataType.Modifier;
            antiArmorAttackDataUI.attackState = attackState.Benificial;
            allAttackData.Add(antiArmorAttackDataUI);
        }
        else if (armorDamagePercentage < 1)
        {
            AttackDataUI weakArmourAttackDataUI = new AttackDataUI();
            weakArmourAttackDataUI.data = "WeakArmour " + armorDamagePercentage.ToString() + "X";
            weakArmourAttackDataUI.attackDataType = attackDataType.Modifier;
            weakArmourAttackDataUI.attackState = attackState.Benediction;
            allAttackData.Add(weakArmourAttackDataUI);
        }

        return allAttackData;
    }

    public AttackData GetCalculatedAttackData(Unit targetUnit)
    {
        List<Damage> tempAllDamage = new List<Damage>();
        foreach (Damage damage in allDamage)
        {
            Damage tempDamage = new Damage();
            tempDamage.minDamage = (int)(damage.minDamage * originUnit.GetMinimumDamageModifer());
            tempDamage.maxDamage = (int)(damage.maxDamage * originUnit.GetMaximumDamageModifer());
            tempDamage.damageType = damage.damageType;
            if (tempDamage.minDamage > tempDamage.maxDamage)
            {
                tempDamage.maxDamage = tempDamage.minDamage;
            }
            tempAllDamage.Add(tempDamage);
        }

        Modifier resistances = targetUnit.GetCalculatedEstimatedDamage(this, tempAllDamage, true);

        for (int i = 0; i < tempAllDamage.Count; i++)
        {
            if (tempAllDamage[i].maxDamage == 0)
            {
                tempAllDamage.RemoveAt(i);
                i--;
            }
        }

        AttackData tempAttackData = new AttackData(allDamage, armorDamagePercentage, originUnit);
        tempAttackData.modifiers = modifiers;
        tempAttackData.ignoreArmour = ignoreArmour;
        return tempAttackData;
    }

    public List<AttackDataUI> CalculateAttackData(Unit targetUnit)
    {
        AttackDataUI mainAttack = new AttackDataUI();
        int adjustedMinimumDamage = 10;
        int adjustmedMaximumDamage = 10;

        Tuple<int, int, List<AttackDataUI>> attackData = targetUnit.CalculateEstimatedDamage(adjustedMinimumDamage, adjustmedMaximumDamage, true);
        adjustedMinimumDamage = attackData.Item1;
        adjustmedMaximumDamage = attackData.Item2;
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

        List<AttackDataUI> allAttackData = new List<AttackDataUI>();
        allAttackData.Add(mainAttack);
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
}
