using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/ItemParameterSO")]
public class ItemParameterSO : ScriptableObject
{
    public ItemParameterName itemParameter;
    public string parameterName;
}

public enum ItemParameterName
{
    damge,
    armor,
    weight,
    spellPointGeneration,
    Capacity
}
