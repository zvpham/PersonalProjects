using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/SlowTimeField")]
public class SlowTimeField : StatusAOETargetingAction
{
    public override int CalculateWeight(Unit self)
    {
        throw new System.NotImplementedException();
    }

    public override void Activate(Unit self)
    {
        throw new System.NotImplementedException();
    }

    public override void PlayerActivate(Unit self)
    {
        affectedUnit = self;
        affectedUnit.ActivateTargeting();
        Vector3 position = Vector3.zero;
        Quaternion rotation = new Quaternion(0, 0, 0, 1f);
        targetingSystem = Instantiate(targetingPrefab, position, rotation);
        targetingSystem.GetComponent<BlastPointAndClick>().setParameters(affectedUnit.transform.position, blastRadius, range);
        targetingSystem.GetComponent<BlastPointAndClick>().endPointFound += foundTarget;
    }


    private void foundTarget(Vector3 endpoint)
    {
        targetingSystem.GetComponent<BlastPointAndClick>().DestroySelf();
        Destroy(targetingSystem);
        CreatedField newField = Instantiate(createdField);
        newField.CreateGridOfObjects(affectedUnit.gameManager, affectedUnit, endpoint, blastRadius, duration);
        affectedUnit.HandlePerformActions(actionType, actionName);
        affectedUnit.DeactivateTargeting();
        affectedUnit.TurnEnd();
    }

    public override void AnimationEnd()
    {

    }
}   
