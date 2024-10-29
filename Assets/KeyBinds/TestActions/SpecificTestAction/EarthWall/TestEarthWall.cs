using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TestAction/TestEarthWall")]
public class TestEarthWall : TestAction
{
    public override void Activate(TestInputManager inputManager)
    {
        inputManager.testHexGrid.EarthWall();
    }
}
