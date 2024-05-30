using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/EndTurn")]
public class EndTurn : Action
{
    public override void SelectAction(Unit self)
    {
        base.SelectAction(self);
        self.gameManager.spriteManager.ActivateActionConfirmationMenu(
            () =>
            {
                self.UseActionPoints(self.currentActionsPoints);
            },
            () =>
            {

            });
    }
}
