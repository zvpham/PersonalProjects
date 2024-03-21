using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MenuAction/UseUI")]
public class UseUI : MenuAction
{
    public override void Activate(BaseUIPage activePage)
    {
        activePage.UseUI();
    }
}
