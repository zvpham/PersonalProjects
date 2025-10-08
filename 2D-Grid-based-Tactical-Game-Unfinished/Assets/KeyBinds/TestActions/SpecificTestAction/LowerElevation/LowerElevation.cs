using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TestAction/LowerElevation")]
public class LowerElevation : TestAction
{
    public override void Activate(TestInputManager inputManager)
    {
        inputManager.testHexGrid.ChangeElevation(-1);
    }
}
