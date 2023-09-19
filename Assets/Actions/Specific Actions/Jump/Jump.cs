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
        foreach (Status statuseffect in status)
        {
            Status temp = Instantiate(statuseffect);
            temp.path = pathAI;
            temp.ApplyEffect(affectedUnit);
        }
        self.TurnEnd();
    }

    public override void PlayerActivate(Unit self)
    {   
        affectedUnit.notOnHold = false;
        Vector3 position = Vector3.zero;
        Quaternion rotation = new Quaternion(0, 0, 0, 1f);
        targetingSystem = Instantiate(targetingPrefab, position, rotation);
        targetingSystem.GetComponent<LineOfSight>().setParameters(affectedUnit.transform.position, range, self.originalSprite, numSections: 2);
        targetingSystem.GetComponent<LineOfSight>().lineMade += foundTarget;
    }


    private void foundTarget(List<Vector3> path)
    {
        targetingSystem.GetComponent<LineOfSight>().DestroySelf();
        Destroy(targetingSystem);

        foreach (Status statuseffect in status)
        {
            Status temp = Instantiate(statuseffect);
            temp.path = path;
            temp.ApplyEffect(affectedUnit);
        }

        affectedUnit.HandlePerformActions(actionType, actionName);
        affectedUnit.notOnHold = true;
        affectedUnit.TurnEnd();
    }
}