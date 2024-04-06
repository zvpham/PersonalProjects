using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
    
[CreateAssetMenu(menuName = "CreatedField/SlowTimeField")]
public class SlowTimeFieldField : CreatedField
{
    public override void CreateGridOfObjects(GameManager gameManager, Unit originUnit, Vector3 originPosition, int fieldRadius,
        int fieldDuration, bool onLoad)
    {
        for(int i = 0; i < createdObjectStatuses.Count(); i++)
        {
            createdObjectStatuses[i].isFieldStatus = true;
        }
        this.createdWithBlastRadius = true;
        this.originPosition = originPosition;
        this.fieldRadius = fieldRadius; 
        this.gameManager = gameManager;
        this.grid = new Grid<CreatedObject>(fieldRadius * 2 + 1, fieldRadius * 2 + 1, 1f, originPosition +
            new Vector3(-fieldRadius, -fieldRadius, 0), (Grid<CreatedObject> g, int x, int y) => 
            new SlowTimeFieldObject(g, originUnit, x, y, gameManager, createdObjectPrefab, originPosition + new Vector3(-fieldRadius, -fieldRadius, 0),
            createdObjectStatuses, fieldDuration, fieldRadius));
        gameManager.createdFields.Add(this);
        gameManager.mainGameManger.createdFields.Add(this);
        if (!onLoad)
        {
            currentCreatedFieldDuration = fieldDuration + 1;
            createdFieldPriority = (int)(gameManager.baseTurnTime * createdFieldQuickness);
            ApplyStatusOnCreation();
        }
    }

    public override void CreateGridOfObjects(GameManager gameManager, Unit originUnit, Grid<CreatedObject> grid, int fieldDuration, bool onLoad)
    {
        CreateGridofObjectsUsingGridPreset(gameManager, originUnit, grid, fieldDuration, onLoad);
    }

    public override CreatedObject CreateCreatedObject(Grid<CreatedObject> g, int x, int y, List<Vector3> validLocations)
    {
        throw new System.NotImplementedException();
    }

    public override void ApplyObject(float applyPercentage, GameManager gameManager, Vector3 Location)
    {
        throw new System.NotImplementedException();
    }
}
