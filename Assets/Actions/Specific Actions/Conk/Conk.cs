using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Conk")]
public class Conk : Action
{
    public override void Activate(Unit self)
    {
        throw new System.NotImplementedException();
    }

    public override int CalculateWeight(Unit self)
    {
        throw new System.NotImplementedException();
    }

    public override void PlayerActivate(Unit self)
    {
        affectedUnit = self;
        self.ActivateTargeting();
        Vector3 position = Vector3.zero;
        Quaternion rotation = new Quaternion(0, 0, 0, 1f);
        targetingSystem = Instantiate(targetingPrefab, position, rotation);
        targetingSystem.GetComponent<MeleeTargeting>().setParameters(affectedUnit.transform.position);
        targetingSystem.GetComponent<MeleeTargeting>().foundTarget += FoundTarget;
    }

    public void FoundTarget(Vector3 targetPosition)
    {
        targetingSystem.GetComponent<MeleeTargeting>().foundTarget -= FoundTarget;
        Unit targetUnit = affectedUnit.gameManager.grid.GetGridObject(targetPosition);
        startStatusPresets(targetUnit);
        affectedUnit.DeactivateTargeting();
        affectedUnit.TurnEnd();
    }
}
