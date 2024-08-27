using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerAction/PreviousItemAction")]
public class PreviousItemAction : PlayerAction
{
    public override void Activate(InputManager inputManager)
    {
        inputManager.gameManager.spriteManager.PreviousItem();
    }
}
