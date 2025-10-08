using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TestAction/PlaceUnit")]
public class PlaceUnit : TestAction
{
    public int teamIndex;
    public override void Activate(TestInputManager inputManager)
    {
        inputManager.testHexGrid.PlaceUnit(teamIndex);
    }
}
