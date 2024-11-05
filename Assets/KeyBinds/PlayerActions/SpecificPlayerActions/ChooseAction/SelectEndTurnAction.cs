using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "PlayerAction/SelectEndTurnAction")]
public class SelectEndTurnAction : PlayerAction
{
    public override void Activate(InputManager input)
    {
        if (input.player.currentlySelectedAction != input.player.currentlySelectedUnit.actions[1].action)
        {
            input.player.SelectAction(input.player.currentlySelectedUnit.actions[1].action);
        }
        else
        {
            input.player.SelectAction(input.player.currentlySelectedUnit.actions[0].action);
        }
    }
}
