using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Movement/MoveWest")]
public class MoveWest : Action
{

    public override void Activate(Unit self)
    {
        Vector2 newPosition = new Vector2();
        newPosition.Set(-1f, 0f);
        Move.Movement(self, newPosition, self.gameManager);
    }

    public override void PlayerActivate(Unit self)
    {
        Activate(self);
    }
}
