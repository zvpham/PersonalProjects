using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Passive : ScriptableObject
{
    public int passiveIndex;
    public Sprite UISkillImage;
    public string description;

    public Unit unit;

    public void Start()
    {
        unit.OnSelectedAction += OnSelectedAction;
    }

    public virtual void AddPassive(Unit unit)
    {
        unit.passives.Add(this);
        //unit.activePassiveLocations.Add(new List<Vector2Int>());
    }

    public abstract void RemovePassive(Unit unit);

    public abstract void OnSelectedAction(Action action, TargetingSystem targetingSystem);
}
