using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TestAction/RaiseElevation")]
public class RaiseElevation : TestAction
{
    public override void Activate(TestInputManager inputManager)
    {
        inputManager.testHexGrid.ChangeElevation(1);
    }
}
