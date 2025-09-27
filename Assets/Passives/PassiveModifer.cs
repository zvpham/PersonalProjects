using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PassiveModifer : ScriptableObject
{
    public abstract void Unlock(Passive passive);

    public abstract void Lock(Passive passive);
}
