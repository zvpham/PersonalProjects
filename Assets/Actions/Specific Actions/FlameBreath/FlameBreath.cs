using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/FlameBreath")]
public class FlameBreath : Action
{

    public GameObject animation;
    private GameObject activeAnimation;

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
        affectedUnit.notOnHold = false;
        Vector3 position = Vector3.zero;
        Quaternion rotation = new Quaternion(0, 0, 0, 1f);
        targetingSystem = Instantiate(targetingPrefab, position, rotation);
        targetingSystem.GetComponent<ConeAttack>().setParameters(affectedUnit.transform.position, blastRadius, blastAngle);

        targetingSystem.GetComponent<ConeAttack>().foundTarget += FoundTarget;
        //targetingSystem.GetComponent<PointAndClick>().setParameters(affectedUnit.transform.position);
        //targetingSystem.GetComponent<LineOfSightNoCollision>().setParameters(affectedUnit.transform.position, 6, careAboutRangeGiven: true
        //targetingSystem.GetComponent<BlastProjectileNoCollision>().setParameters(affectedUnit.transform.position, 15, 6, careAboutObstaclesGiven: true);
        //targetingSystem.GetComponent<BlastPointAndClick>().setParameters(affectedUnit.transform.position, 15);
    }   

    private void FoundTarget(List<Vector3> markerList, Vector3 Direction)
    {
        Debug.Log("SLow TIme Found Target " + markerList);
        targetingSystem.GetComponent<ConeAttack>().DestroySelf();
        Destroy(targetingSystem);

        affectedUnit.HandlePerformActions(actionType, actionName);
        activeAnimation = Instantiate(animation);
        activeAnimation.GetComponent<EmenateFromCenter>().SetParameters(affectedUnit.transform.position, Direction, blastAngle, createObject, blastRadius, markerList, 30, 20, 20, ignoreWalls: false);
        activeAnimation.GetComponent<EmenateFromCenter>().enabled = true;
        activeAnimation.GetComponent<EmenateFromCenter>().animationEnd += AnimationEnd;
    }

    private void AnimationEnd(int ignore)
    {
        activeAnimation.GetComponent<EmenateFromCenter>().DestroySelf();
        Destroy(activeAnimation);
        affectedUnit.notOnHold = true;
        affectedUnit.TurnEnd();
    }
}


