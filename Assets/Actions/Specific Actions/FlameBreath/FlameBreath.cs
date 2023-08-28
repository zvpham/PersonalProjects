using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/FlameBreath")]
public class FlameBreath : Action
{
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
    }

    private void foundTarget(Vector3 endpoint)
    {
        Debug.Log("SLow TIme Found Target " + endpoint);
        targetingSystem.GetComponent<LineOfSight>().DestroySelf();
        Destroy(targetingSystem);

        createdField.CreateGridOfObjects(affectedUnit.gameManager, blastRadius, endpoint);
        affectedUnit.notOnHold = true;
    }
}
