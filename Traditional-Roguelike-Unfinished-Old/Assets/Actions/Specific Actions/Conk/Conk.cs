using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Conk")]
public class Conk : StatusTargetAction
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
        targetingSystem.GetComponent<MeleeTargeting>().Canceled += EndTarget;
    }

    public void EndTarget(bool ignore)
    {
        targetingSystem.GetComponent<MeleeTargeting>().foundTarget -= FoundTarget;
        Destroy(targetingSystem);
        affectedUnit.DeactivateTargeting();
        affectedUnit.TurnEnd();
        affectedUnit.HandlePerformActions(actionType, actionName);
    }

    public void FoundTarget(Vector3 targetPosition)
    {
        targetingSystem.GetComponent<MeleeTargeting>().foundTarget -= FoundTarget;
        Destroy(targetingSystem);
        Unit targetUnit = affectedUnit.gameManager.grid.GetGridObject(targetPosition);

        int launchDistance = affectedUnit.strengthMod;

        if(launchDistance <= 3)
        {
            launchDistance = 3;
        } 

        List<Vector2> path = new List<Vector2>();

        Vector2 LaunchDirection = targetPosition - affectedUnit.transform.position;
        Vector2 currentPathLocation = targetPosition;

        path.Add(currentPathLocation);
        for(int i = 0; i < launchDistance; i++)
        {
            currentPathLocation += LaunchDirection;
            path.Add(currentPathLocation);
        }

        startMovementStatusPreset(targetUnit, path);
        affectedUnit.HandlePerformActions(actionType, actionName);
    }

    public override void AnimationEnd()
    {
        affectedUnit.DeactivateTargeting();
        affectedUnit.TurnEnd();
    }
}
