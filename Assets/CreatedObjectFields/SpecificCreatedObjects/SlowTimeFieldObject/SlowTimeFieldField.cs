using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
[CreateAssetMenu(menuName = "CreatedField/SlowTimeField")]
public class SlowTimeFieldField : CreatedField
{
    public override void CreateGridOfObjects(GameManager gameManager, Vector3 originPosition, int fieldRadius, int fieldDuration, bool onLoad)
    {
        this.createdWithBlastRadius = true;
        this.originPosition = originPosition;
        this.fieldRadius = fieldRadius; 
        this.gameManager = gameManager;
        this.grid = new Grid<CreatedObject>(fieldRadius * 2 + 1, fieldRadius * 2 + 1, 1f, originPosition + new Vector3(-fieldRadius, -fieldRadius, 0), (Grid<CreatedObject> g, int x, int y) => new SlowTimeFieldObject(g, x, y, createdObjectPrefab, originPosition + new Vector3(-fieldRadius, -fieldRadius, 0), createdObjectStatuses, fieldDuration, fieldRadius));
        gameManager.createdFields.Add(this);
        if (!onLoad)
        {
            gameManager.createdFieldDuration.Add(fieldDuration + 1);
            gameManager.createdFieldPriority.Add((int)(gameManager.baseTurnTime * createdFieldQuickness));
            ApplyStatusOnCreation();
        }
    }

    public override void CreateGridOfObjects(GameManager gameManager, Grid<CreatedObject> grid, int fieldDuration, bool onLoad)
    {
        this.grid = grid;
        this.gameManager = gameManager;
        if (!onLoad)
        {
            gameManager.createdFields.Add(this);
            gameManager.createdFieldDuration.Add(fieldDuration);
            gameManager.createdFieldPriority.Add((int)(gameManager.baseTurnTime * createdFieldQuickness) + gameManager.least);
        }
        else
        {
            for (int i = 0; i < gameManager.createdFields.Count; i++)
            {
                if (gameManager.createdFields[i] == null)
                {
                    gameManager.createdFields[i] = this;
                    return;
                }
            }
            Debug.LogError("Didn't find an open createdFields SLot");
        }
    }
}
