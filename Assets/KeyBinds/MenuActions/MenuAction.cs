using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuAction : ScriptableObject
{
    public MenuActionName actionName;
    public abstract void Activate(MenuInputManager inputManager);
}
