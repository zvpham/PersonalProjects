using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIClassGroup : BaseGameUIObject
{
    public BaseGameUIObject unlockedAbilities;
    public BaseGameUIObject lockedAbilities;
    public Class currentClass;

    public override void UseUi()
    {
        UIClassPage classPage = (UIClassPage)UIPage;
        classPage.BeginClassLevelUp(currentClass);
    }
}
