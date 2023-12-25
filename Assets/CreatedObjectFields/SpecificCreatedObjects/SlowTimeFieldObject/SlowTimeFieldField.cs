using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
[CreateAssetMenu(menuName = "CreatedField/SlowTimeField")]
public class SlowTimeFieldField : CreatedField
{
    public override void CreateGridOfObjects(GameManager gameManager, Vector3 originPosition, int fieldRadius,
        int fieldDuration, bool onLoad)
    {
        this.createdWithBlastRadius = true;
        this.originPosition = originPosition;
        this.fieldRadius = fieldRadius; 
        this.gameManager = gameManager;
        this.grid = new Grid<CreatedObject>(fieldRadius * 2 + 1, fieldRadius * 2 + 1, 1f, originPosition +
            new Vector3(-fieldRadius, -fieldRadius, 0), (Grid<CreatedObject> g, int x, int y) => 
            new SlowTimeFieldObject(g, x, y, createdObjectPrefab, originPosition + new Vector3(-fieldRadius, -fieldRadius, 0),
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

    public override void CreateGridOfObjects(GameManager gameManager, Grid<CreatedObject> grid, int fieldDuration, bool onLoad)
    {
        CreateGridofObjectsUsingGridPreset(gameManager,grid, fieldDuration, onLoad);
    }
}
