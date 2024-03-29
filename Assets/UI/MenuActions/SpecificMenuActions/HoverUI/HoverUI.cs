using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MenuAction/HoverUI")]
public class HoverUI : MenuAction
{
    public override void Activate(BaseUIPage activePage)
    {
        ChooseClassMenu classMenu = (ChooseClassMenu) activePage;
    }
}
