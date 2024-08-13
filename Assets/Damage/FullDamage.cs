using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Damage/DamageFull")]
public class FullDamage : ScriptableObject
{
    public List<DamageDice> damageDices;
    public List<DamageTypes> staticDamageType;
    public List<int> staticDamageValue;
    
    public List<Tuple<DamageTypes, int>> RollForDamage()
    {
        if(staticDamageType.Count != staticDamageValue.Count)
        {
            throw new Exception("FIX static DAMAGE for" +  this);  
        }

        List<Tuple<DamageTypes, int>> totalDamaage = new List<Tuple<DamageTypes, int>>();

        foreach(DamageDice damageDice in damageDices) 
        {
            int damage = 0;
            for(int i = 0; i < damageDice.numberDice; i++)
            {
                damage += UnityEngine.Random.Range(1, damageDice.diceFaces + 1);
            }
            if (staticDamageType.IndexOf(damageDice.damageType) != -1)
            {
                damage += staticDamageValue[staticDamageType.IndexOf(damageDice.damageType)];
            }
            totalDamaage.Add(new Tuple<DamageTypes, int>( damageDice.damageType, damage));
        }
        if (totalDamaage.Count == 0)
        {
            totalDamaage.Add(new Tuple<DamageTypes, int>(DamageTypes.physical, 0));
        }
        return totalDamaage;
    }
}
