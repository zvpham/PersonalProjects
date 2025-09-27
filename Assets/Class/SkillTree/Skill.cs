using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill : ScriptableObject
{
    public string skillNameKey;
    public Sprite skillimage;

    public abstract void AddSkill(Unit unit);
    public abstract void RemoveSkill(Unit unit);
}
