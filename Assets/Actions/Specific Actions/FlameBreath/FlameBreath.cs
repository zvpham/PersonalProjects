using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/FlameBreath")]
public class FlameBreath : Action
{

    public GameObject animation;
    private GameObject activeAnimation;
    List<Vector3> blastLocations;
    Vector3 finalDirection = Vector3.zero;

    public override int CalculateWeight(Unit self)
    {
        int weightModifier = 0;
        int highestWeightModifier = -1;
        float distanceModifier = 0;
        Vector3 tempDirection =  Vector3.zero;

        for (int i = 0; i < self.enemyList.Count; i++)
        {
            List<Vector3> pathAI = LineOfSightAI.MakeLine(self.transform.position, self.enemyList[i].transform.position, blastRadius, true);
            if (pathAI[pathAI.Count - 1] == self.enemyList[i].transform.position)
            {
                Tuple<List<Vector3>, Vector3> temp = ConeAttackAI.GetCone(self.transform.position, self.enemyList[i].transform.position, blastRadius, blastAngle);
                blastLocations = temp.Item1;
                tempDirection = temp.Item2;
                distanceModifier = 1 - pathAI.Count / blastRadius;
            }

            foreach(Vector3 location in blastLocations)
            {
                try
                {
                    if (self.gameManager.grid.GetGridObject(location).faction == self.faction)
                    {
                        weightModifier -= (int)(weight * distanceModifier * self.friendlyFireMultiplier);
                    }
                    else
                    {
                        weightModifier += (int)(weight * distanceModifier);
                    }
                }
                catch
                {

                }
            }
            
            if(weightModifier > highestWeightModifier)
            {
                highestWeightModifier = weightModifier;
                finalDirection = tempDirection;
            }
        }

        return  highestWeightModifier;

    }
    public override void Activate(Unit self)
    {
        affectedUnit = self;
        affectedUnit.ActivateTargeting();
        affectedUnit.HandlePerformActions(actionType, actionName);
        activeAnimation = Instantiate(animation);
        activeAnimation.GetComponent<EmenateFromCenterField>().SetParameters(affectedUnit.transform.position, finalDirection, blastAngle, createObject, createObjectHolder, createdField, blastRadius, 30, 30, 20, ignoreWalls: false);
        activeAnimation.GetComponent<EmenateFromCenterField>().enabled = true;
        activeAnimation.GetComponent<EmenateFromCenterField>().animationEnd += AnimationEnd;
    }

    public override void PlayerActivate(Unit self)
    {
        affectedUnit = self;
        affectedUnit.ActivateTargeting();
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
        
    private void FoundTarget(Vector3 Direction)
    {
        targetingSystem.GetComponent<ConeAttack>().DestroySelf();
        Destroy(targetingSystem);

        affectedUnit.HandlePerformActions(actionType, actionName);
        activeAnimation = Instantiate(animation);
        activeAnimation.GetComponent<EmenateFromCenterField>().SetParameters(affectedUnit.transform.position, Direction, blastAngle, createObject, createObjectHolder, createdField, blastRadius, 30, 30, 20, ignoreWalls: false);
        activeAnimation.GetComponent<EmenateFromCenterField>().enabled = true;
        activeAnimation.GetComponent<EmenateFromCenterField>().animationEnd += AnimationEnd;
    }

    private void AnimationEnd(int ignore)
    {
        activeAnimation.GetComponent<EmenateFromCenterField>().animationEnd -= AnimationEnd;
        affectedUnit.TurnEnd();
        affectedUnit.DeactivateTargeting();
    }
}


