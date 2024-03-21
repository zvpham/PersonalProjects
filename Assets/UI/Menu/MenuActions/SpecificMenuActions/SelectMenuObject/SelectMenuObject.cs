using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MenuAction/SelectMenuObject")]
public class SelectMenuObject : MenuAction
{
    public int menuOptionIndex;
    public override void Activate(BaseUIPage activeMenuPage)
    {
        activeMenuPage.SelectMenuObject(menuOptionIndex);
    }
}
