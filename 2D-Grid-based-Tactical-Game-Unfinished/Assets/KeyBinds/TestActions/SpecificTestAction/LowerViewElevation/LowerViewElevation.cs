using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TestAction/LowerViewElevation")]
public class LowerViewElevation : TestAction
{
    public override void Activate(TestInputManager inputManager)
    {
        inputManager.testHexGrid.ChangeViewElevation(-1);
    }
}
