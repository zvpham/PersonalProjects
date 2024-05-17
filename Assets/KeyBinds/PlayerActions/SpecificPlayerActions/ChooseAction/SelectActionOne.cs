using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "PlayerAction/SelectActionOne")]
public class SelectActionOne : PlayerAction
{
    public override void Activate(InputManager input)
    {
        if(input.player.currentlySelectedUnit.actions.Count >= 1)
            input.player.SelectAction(input.player.currentlySelectedUnit.actions[0]);
    }
}
