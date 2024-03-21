using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MenuAction/IndexDown")]
public class IndexDown : MenuAction
{
    public override void Activate(BaseUIPage activePage)
    {
        activePage.IndexDown();
    }
}
