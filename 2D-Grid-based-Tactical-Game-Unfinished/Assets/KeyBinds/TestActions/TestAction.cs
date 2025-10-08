using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TestAction : ScriptableObject
{
    public TestActionName actionName;
    public abstract void Activate(TestInputManager inputManager);
}
