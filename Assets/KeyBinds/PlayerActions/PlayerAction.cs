using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAction : ScriptableObject
{
    public PlayerActionName actionName;
    public abstract void Activate(InputManager inputManager);
}
