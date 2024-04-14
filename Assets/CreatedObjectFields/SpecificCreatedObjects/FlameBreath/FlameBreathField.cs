using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

[CreateAssetMenu(menuName = "CreatedField/FlameBreathField")]
public class FlameBreathField : CreatedField
{
    public override void CreateGridOfObjects(GameManager gameManager, Unit originUnit, Vector3 originPosition, int fieldRadius, int fieldDuration, bool onLoad)
    {
        throw new System.NotImplementedException();
    }

    public override void CreateGridOfObjects(GameManager gameManager, Unit originUnit, Grid<CreatedObject> grid, int fieldDuration, bool onLoad)
    {
        CreateGridofObjectsUsingGridPreset(gameManager, originUnit, grid, fieldDuration, onLoad);
    }
    public override CreatedObject CreateCreatedObject(Grid<CreatedObject> g, int x, int y, List<Vector3> validLocations)
    {
        return new FlameBreathCreatedObject(g, originUnit, x, y, createdObjectPrefab, validLocations);
    }

    public override void ApplyObject(float applyPercentage, Unit originUnit, GameManager gameManager, Vector3 Location)
    {
        FlameBreathCreatedObject.ApplyObject(applyPercentage, originUnit, gameManager, Location, damageCalculation);
    }
}
