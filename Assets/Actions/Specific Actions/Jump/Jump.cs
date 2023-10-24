using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Action/Jump")]
public class Jump : Action
{
    public List<Vector3> pathAI;
    public override int CalculateWeight(Unit self)
    {
        bool pathFound = false;
        int index = self.closestEnemyIndex;
        for (int i = 0; i < self.enemyList.Count; i++)
        {
            pathAI = LineOfSightAI.MakeLine(self.transform.position, self.enemyList[index].transform.position, range, true);
            if(pathAI !=  null)
            {
                //Debug.Log("Jump Index " + (int)(pathAI.Count / 2f) + " " + self);
                if(self.gameManager.flyingGrid.GetGridObject(pathAI[(int)(pathAI.Count / 2f)]) == null){
                    pathFound = true;
                    break;
                }
            }
            index += 1;
            if(index >= self.enemyList.Count)
            {
                index = 0;
            }
        }

        if(pathFound)
        {
            return weight;
        }
        return 0;
    }

    public override void Activate(Unit self)
    {
        self.forcedMovementPathData.forcedMovementPath = pathAI;
        foreach (Status statuseffect in status)
        {
            Status temp = Instantiate(statuseffect);
            temp.ApplyEffect(self);
        }
        self.TurnEnd();
    }

    public override void PlayerActivate(Unit self)
    {
        self.ActivateTargeting();
        Vector3 position = Vector3.zero;
        Quaternion rotation = new Quaternion(0, 0, 0, 1f);
        targetingSystem = Instantiate(targetingPrefab, position, rotation);
        targetingSystem.GetComponent<LineOfSight>().setParameters(self.transform.position, range, self.originalSprite, numSections: 2);
        targetingSystem.GetComponent<LineOfSight>().lineMade += foundTarget;
    }


    private void foundTarget(List<Vector3> path)
    {
        targetingSystem.GetComponent<LineOfSight>().DestroySelf();
        Destroy(targetingSystem);
        affectedUnit.forcedMovementPathData.forcedMovementPath = path;
        foreach (Status statuseffect in status)
        {
            Status temp = Instantiate(statuseffect);
            temp.ApplyEffect(affectedUnit);
        }

        affectedUnit.HandlePerformActions(actionType, actionName);
        affectedUnit.DeactivateTargeting();
        affectedUnit.TurnEnd();
    }
}