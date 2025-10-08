using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TestAction/RaiseViewElevation")]
public class RaiseViewElevation : TestAction
{
    public override void Activate(TestInputManager inputManager)
    {
        inputManager.testHexGrid.ChangeViewElevation(1);
    }
}
