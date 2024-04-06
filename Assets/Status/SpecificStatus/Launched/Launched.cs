using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launched : MovementStatus
{
    public override void ApplyEffect(Unit target, int newDuration)
    {
        AddStatusPreset(target, newDuration);
        MovementStatusPreset(target, forcedMovementPath.Count, forcedMovementPath, false, true);
    }

    public override void ChangeQuicknessNonstandard(float value)
    {

    }

    public override void EndForcedMovement(Unit self)
    {
        self.gameManager.ChangeUnits(self.gameObject.transform.position, self);
    }

    public override bool OnHitUnit(Unit self, Unit target)
    {
        Vector2 direction =  target.transform.position - self.transform.position;

        //Perpendicular to direction
        List<Vector2> idealDirections = new List<Vector2>();
        //Not forward or backwards
        List<Vector2> nextBestDirections = new List<Vector2>();     

        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                float dotProduct = Vector2.Dot(direction, new Vector2(j, i));
                if (dotProduct == 0)
                {
                    idealDirections.Add(direction);
                }
                else if(dotProduct != 1 && dotProduct != -1) 
                { 
                    nextBestDirections.Add(direction);
                }
            }
        }


        //Change this
        for(int i = 0; i < idealDirections.Count; i++)
        {
            Vector3 newPosition =  (Vector2) target.transform.position + idealDirections[i];
            if (self.gameManager.obstacleGrid.GetGridObject(newPosition) == null && self.gameManager.grid.GetGridObject(newPosition) == null)
            {
                target.gameManager.ChangeUnits(target.transform.position, null);
                return true;
            }
        }
        return false;
    }

    public override bool OnHitWall(Unit self, Wall wall)
    {
        throw new System.NotImplementedException();
    }

    public override void onLoadApply(Unit target)
    {
        AddStatusOnLoadPreset(target);
    }

    override public void RemoveEffect(Unit target)
    {
        RemoveStatusPreset(target);
    }
}
