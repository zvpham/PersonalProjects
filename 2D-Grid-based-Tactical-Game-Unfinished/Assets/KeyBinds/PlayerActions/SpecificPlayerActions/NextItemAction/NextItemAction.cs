using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerAction/NextItemAction")]
public class NextItemAction : PlayerAction
{
    public override void Activate(InputManager inputManager)
    {
        inputManager.gameManager.spriteManager.NextItem();
    }
}
