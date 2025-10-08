using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MenuInputAction/Back")]
public class BackMenuAction : MenuAction
{
    public override void Activate(MenuInputManager inputManager)
    {
        inputManager.overWorldMenu.CloseMenu();
    }
}
