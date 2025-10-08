using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Damage/DamageDice")]
public class DamageDice : ScriptableObject
{
    public int numberDice;
    public int diceFaces;
    public DamageTypes damageType;
}
