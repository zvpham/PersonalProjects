using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PlayerAction/ResetAction")]
public class ResetTest : PlayerAction
{
    public override void Activate(InputManager inputManager)
    {
        inputManager.gameManager.ResetTest();
    }
}
