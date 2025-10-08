using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MenuAction/CloseMenu")]
public class CloseMenu : MenuAction
{
    public override void Activate(BaseUIPage activeMenuPage)
    {
        activeMenuPage.gameMenu.CloseMenu();
    }
}
