using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "PlayerAction/ConfirmAction")]
public class ConfirmAction : PlayerAction
{
    public override void Activate(InputManager inputManager)
    {
        inputManager.gameManager.spriteManager.ConfirmAction();
    }
}
