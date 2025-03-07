using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Passive : ScriptableObject
{
    public int passiveIndex;
    public Sprite UISkillImage;
    public string description;
    public PassiveAreaClassification passiveClassification;
    public int maxUseAmount = 1;

    public virtual void AddPassive(Unit unit)
    {
        UnitPassiveData unitPassiveData = new UnitPassiveData();
        unitPassiveData.passive = this;
        unitPassiveData.active = true;
        unitPassiveData.passiveUseAmount = 0;
        unitPassiveData.passiveMaxUseAmount = maxUseAmount;
        unit.passives.Add(unitPassiveData);

        //unit.activePassiveLocations.Add(new List<Vector2Int>());
    }

    public abstract void ActivatePassive(Unit unit, ActionData actionData);

    public abstract void RemovePassive(Unit unit);

    public abstract Tuple<Passive, Vector2Int> GetTargetingData(Vector2Int originalPosition, List<Vector2Int> path, List<Vector2Int> setPath, List<Vector2Int> passiveArea);

    public abstract void CalculatePredictedActionConsequences(AIActionData AiActionData, Unit actingUnit, Vector2Int orignalPosition, List<Vector2Int> path, List<Vector2Int> passiveArea);
    abstract public void ModifiyAction(Action action, AttackData attackData);

    public int GetPassiveIndex(Unit unit)
    {
        for(int i = 0; i < unit.passives.Count; i++)
        {
            if (unit.passives[i].passive == this)
            {
                return i;
            }
        }
        return -1;
    }
}
