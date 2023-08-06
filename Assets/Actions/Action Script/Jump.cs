using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Jump")]
public class Jump : Action
{
    GameObject targetingSystem;
    public override void Activate(Unit self)
    {
        startActionPresets();
    }

    public override void PlayerActivate(Unit self)
    {   
        affectedUnit = self;
        affectedUnit.notOnHold = false;
        Vector3 position = Vector3.zero;
        Quaternion rotation = new Quaternion(0, 0, 0, 1f);
        targetingSystem = Instantiate(targeting, position, rotation);
        targetingSystem.GetComponent<LineOfSight>().setParameters(affectedUnit.transform.position, true, 9, self.originalSprite, numSections: 2);
        targetingSystem.GetComponent<LineOfSight>().lineMade += foundTarget;
    }


    private void foundTarget(List<Vector3> path)
    {
        Destroy(targetingSystem);
        Debug.Log("How Many Times");
        foreach (Status statuseffect in status)
        {
            Status temp = Instantiate(statuseffect);
            temp.path = path;
            temp.ApplyEffect(affectedUnit);
        }
        foreach (Vector3 point in path)
        {
            Debug.Log(point);
        }
        affectedUnit.notOnHold = true;
    }
    new public void CalculateWeight()
    {

    }
}