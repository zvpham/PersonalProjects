using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
[CreateAssetMenu(menuName = "CreatedField/SlowTimeField")]
public class SlowTimeFieldField : CreatedField
{
    public override void CreateGridOfObjects(GameManager gameManager, Vector3 originPosition, int fieldRadius, int fieldDuration)
    {

        this.gameManager = gameManager;
        this.grid = new Grid<CreatedObjectStatus>(fieldRadius * 2 + 1, fieldRadius * 2 + 1, 1f, originPosition + new Vector3(-fieldRadius, -fieldRadius, 0), (Grid<CreatedObjectStatus> g, int x, int y) => new SlowTimeFieldObject(g, x, y, createdObjectPrefab, originPosition + new Vector3(-fieldRadius, -fieldRadius, 0), createdObjectStatuses, fieldDuration, fieldRadius));
        gameManager.StatusFields.Add(this);
        gameManager.StatusObjectDuration.Add(fieldDuration + 1);
        gameManager.statusObjectPriority.Add( (int)(gameManager.baseTurnTime * createdFieldQuickness));
        ApplyStatusOnCreation();
    }
}
