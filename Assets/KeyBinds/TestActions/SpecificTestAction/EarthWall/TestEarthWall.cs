using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TestAction/TestEarthWall")]
public class TestEarthWall : TestAction
{
    public int elevationChange = 0;
    public override void Activate(TestInputManager inputManager)
    {
        inputManager.testHexGrid.EarthWall(elevationChange);
    }
}
