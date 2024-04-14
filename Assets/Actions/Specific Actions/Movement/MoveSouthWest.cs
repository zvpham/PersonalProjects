using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Movement/MoveSouthWest")]
public class MoveSouthWest : Action
{

    public override void Activate(Unit self)
    {
        Vector2 newPosition = new Vector2();
        newPosition.Set(-1f, -1f);
        Move.Movement(self, newPosition, self.gameManager);
        affectedUnit.TurnEnd();
    }

    public override void PlayerActivate(Unit self)
    {
        Activate(self);
    }

    public override int CalculateWeight(Unit self)
    {
        throw new System.NotImplementedException();
    }

    public override void AnimationEnd()
    {

    }
}
