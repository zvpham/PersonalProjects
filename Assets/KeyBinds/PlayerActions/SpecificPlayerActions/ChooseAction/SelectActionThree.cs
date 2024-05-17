using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "PlayerAction/SelectActionThree")]
public class SelectActionThree : PlayerAction
{
    public override void Activate(InputManager input)
    {
        if (input.player.currentlySelectedUnit.actions.Count >= 3)
            input.player.SelectAction(input.player.currentlySelectedUnit.actions[2]);
    }
}
