using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Attack/Modifier")]
public class Modifier : ScriptableObject
{
    public float value;
    public string modifierText;
    public attackState attackState;

    public Modifier(float value, string modifierText, attackState attackState)
    {
        this.value = value;
        this.modifierText = modifierText;
        this.attackState = attackState;
    }

    public Modifier(Modifier oldModifier)
    {
        this.value = oldModifier.value;
        this.modifierText = oldModifier.modifierText;
        this.attackState = oldModifier.attackState;
    }
}
