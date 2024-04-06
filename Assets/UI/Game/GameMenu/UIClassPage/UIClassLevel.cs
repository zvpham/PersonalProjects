using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIClassLevel : BaseGameUIObject
{
    public Class currentClass;
    public bool unlocked;
    public override void UseUi()
    {
        if(!unlocked)
        {
            UIClassPage classPage = (UIClassPage)UIPage;
            classPage.BeginClassLevelUp(currentClass);
        }
    }
}
