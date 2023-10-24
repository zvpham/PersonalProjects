using System.Collections;
using System.Collections.Generic;
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
}
