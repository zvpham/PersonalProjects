using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Passive/SelfBuff/Stalwart")]
public class Stalwart : Passive
{
    public override void ActivatePassive(Unit unit, ActionData actionData)
    {
        throw new NotImplementedException();
    }

    public override void AddPassive(Unit unit)
    {
        throw new NotImplementedException();
    }

    public override void CalculatePredictedActionConsequences(AIActionData AiActionData, Unit actingUnit, Vector2Int orignalPosition, List<Vector2Int> path, List<Vector2Int> passiveArea)
    {
        throw new NotImplementedException();
    }

    public override void ChangeBoost(BoostType typeOfBoost, bool is_boost)
    {
        throw new NotImplementedException();
    }

    public override Tuple<Passive, Vector2Int> GetTargetingData(Vector2Int originalPosition, List<Vector2Int> path, List<Vector2Int> setPath, List<Vector2Int> passiveArea)
    {
        throw new NotImplementedException();
    }

    public override void ModifiyAction(Action action, AttackData attackData)
    {
        throw new NotImplementedException();
    }

    public override void RemovePassive(Unit unit)
    {
        throw new NotImplementedException();
    }
}
