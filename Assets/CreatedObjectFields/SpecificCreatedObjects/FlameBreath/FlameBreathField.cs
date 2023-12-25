using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

[CreateAssetMenu(menuName = "CreatedField/FlameBreathField")]
public class FlameBreathField : CreatedField
{
    public override void CreateGridOfObjects(GameManager gameManager, Vector3 originPosition, int fieldRadius, int fieldDuration, bool onLoad)
    {
        throw new System.NotImplementedException();
    }

    public override void CreateGridOfObjects(GameManager gameManager, Grid<CreatedObject> grid, int fieldDuration, bool onLoad)
    {
        CreateGridofObjectsUsingGridPreset(gameManager, grid, fieldDuration, onLoad);
    }
    public override CreatedObject CreateCreatedObject(Grid<CreatedObject> g, int x, int y, List<Vector3> validLocations)
    {
        return new FlameBreathCreatedObject(g, x, y, createdObjectPrefab, validLocations);
    }

    public override void ApplyObject(float applyPercentage, GameManager gameManager, Vector3 Location)
    {
        FlameBreathCreatedObject.ApplyObject(applyPercentage, gameManager, Location, damageCalculation);
    }
}
