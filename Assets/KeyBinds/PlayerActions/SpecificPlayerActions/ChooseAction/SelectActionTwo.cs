using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "PlayerAction/SelectActionTwo")]
public class SelectActionTwo : PlayerAction
{
    public override void Activate(InputManager input)
    {
        if (input.player.currentlySelectedUnit.actions.Count >= 2)
            input.player.SelectAction(input.player.currentlySelectedUnit.actions[1]);
    }
}
