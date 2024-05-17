using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PlayerAction/SelectActionFour")]
public class SelectActionFour : PlayerAction
{
    public override void Activate(InputManager input)
    {
        if (input.player.currentlySelectedUnit.actions.Count >= 4)
            input.player.SelectAction(input.player.currentlySelectedUnit.actions[3]);
    }
}
