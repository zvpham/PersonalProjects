using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuAction : ScriptableObject
{
    public MenuInputNames menuInputName;
    public abstract void Activate(BaseUIPage activePage);
}
