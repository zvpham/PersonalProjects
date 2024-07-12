using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Passive : ScriptableObject
{
    public Sprite UISkillImage;
    public string description;

    public Unit unit;

    public void Start()
    {
        unit.OnSelectedAction += OnSelectedAction;
    }

    public abstract void AddPassive(Unit unit);

    public abstract void RemovePassive(Unit unit);

    public abstract void OnSelectedAction(Action action, TargetingSystem targetingSystem);
}
