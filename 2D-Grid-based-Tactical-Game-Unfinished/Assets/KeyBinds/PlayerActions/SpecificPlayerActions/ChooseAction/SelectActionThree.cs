using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "PlayerAction/SelectActionThree")]
public class SelectActionThree : PlayerAction
{
    public override void Activate(InputManager input)
    {
        if (input.player.currentlySelectedUnit.actions.Count >= 2 &&
            input.player.currentlySelectedAction != input.player.currentlySelectedUnit.actions[2].action)
            input.player.SelectAction(input.player.currentlySelectedUnit.actions[2].action);
        else
        {
            input.player.SelectAction(input.player.currentlySelectedUnit.actions[0].action);
        }
    }
}
