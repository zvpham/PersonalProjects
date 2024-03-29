using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MenuAction/IndexUp")]
public class IndexUp : MenuAction
{
    public override void Activate(BaseUIPage activePage)
    {
        activePage.IndexUp();
    }
}
