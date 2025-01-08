using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Attack/Modifier")]
public class Modifier : ScriptableObject
{
    public float value;
    public string modifierText;
    public attackState attackState;
}
