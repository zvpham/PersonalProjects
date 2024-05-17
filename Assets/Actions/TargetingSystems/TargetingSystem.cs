using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TargetingSystem : MonoBehaviour
{
    public abstract void SelectNewPosition(Vector3 newPosition);
    public abstract void EndMovementTargeting();

    public abstract void DeactivateTargetingSystem();
}
