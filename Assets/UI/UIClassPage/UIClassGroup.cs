using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIClassGroup : BaseGameUIObject
{
    [SerializeField]
    private TMP_Text className;
    public TMPHolder unlockedAbilities;
    public TMPHolder lockedAbilities;

    public void SetClass(string className)
    {
        this.className.text = className;
    }
}
