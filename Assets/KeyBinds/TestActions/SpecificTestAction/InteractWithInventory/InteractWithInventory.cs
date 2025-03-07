using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TestAction/InteractWithInventory")]
public class InteractWithInventory : TestAction
{
    public override void Activate(TestInputManager inputManager)
    {
        Debug.Log("Hello");
        inputManager.testHexGrid.InteractWithInventory();
    }
}
