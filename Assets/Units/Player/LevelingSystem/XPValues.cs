using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveling/XPValues")]
public class XPValues : ScriptableObject
{
    public int baseCommonXpValue = 20;
    public int baseUncommonXpValue = 20;
    public int baseRareXpValue = 20;

    public float commonClassLevelMultiplier = 1.5f;
    public float uncommonClassLevelMultiplier = 1.5f;
    public float rarelassLevelMultiplier = 1.5f;
}
