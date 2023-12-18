using Bresenhams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Sense/NormalVision")]
public class NormalVision : Sense
{
    public bool clearLineOfSightToEnemy;
    public GameManager gameManager;
    public override void DetectNearbyUnits(Unit self)
    {
        gameManager = self.gameManager;
        foreach (Unit unit in self.gameManager.units)
        {
            BresenhamsAlgorithm.PlotFunction plotFunction = isThereClearLineOfSight;
            BresenhamsAlgorithm.Line((int)self.transform.position.x, (int)self.transform.position.y, (int)unit.transform.position.x, (int)unit.transform.position.y, plotFunction);
            clearLineOfSightToEnemy = true;
            if (clearLineOfSightToEnemy && unit.faction == self.faction)
            {
                self.allyList.Add(unit);
            }
            else if (clearLineOfSightToEnemy)
            {
                self.enemyList.Add(unit);
            }
        }
    }

    public override void PeripheralManagerDetectUnits(Unit self)
    {   
        throw new System.NotImplementedException();
    }

    private bool isThereClearLineOfSight(int x, int y, int numberMarkers)
    {
        Vector3 position = new Vector3(x, y, 0) + gameManager.defaultGridPosition;
        if (numberMarkers <= range && (gameManager.obstacleGrid.GetGridObject(position) == null && gameManager.obstacleGrid.GetGridObject(position).blockLineOfSight == true))
        {
            return true;
        }
        clearLineOfSightToEnemy = false;
        return false;
    }
}
