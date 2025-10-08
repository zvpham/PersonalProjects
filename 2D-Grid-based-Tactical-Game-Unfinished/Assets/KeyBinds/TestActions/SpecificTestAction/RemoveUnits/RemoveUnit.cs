using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TestAction/RemoveUnit")]
public class RemoveUnit : TestAction
{
    public override void Activate(TestInputManager inputManager)
    {
        inputManager.testHexGrid.RemoveUnit();
    }
}
