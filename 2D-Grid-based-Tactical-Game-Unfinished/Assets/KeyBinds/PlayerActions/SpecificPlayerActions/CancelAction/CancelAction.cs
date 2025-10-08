using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerAction/CancelAction")]
public class CancelAction : PlayerAction
{
    public override void Activate(InputManager inputManager)
    {
        inputManager.gameManager.spriteManager.CancelAction();
    }
}
