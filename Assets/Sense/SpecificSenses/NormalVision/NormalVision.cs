using Bresenhams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Sense/NormalVision")]
public class NormalVision : Sense
{
    public bool clearLineOfSightToEnemy;
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
        mainGameManger = self.gameManager.mainGameManger;
        for(int i = 0; i < self.gameManager.mainGameManger.activeGameManagers.Count; i++)
        {
            foreach (Unit unit in self.gameManager.mainGameManger.activeGameManagers[i].units)
            {
                if(Vector3.Distance(self.transform.position, unit.transform.position) >= range * 2)
                {
                    continue;
                }
                BresenhamsAlgorithm.PlotFunction plotFunction = isThereClearLineOfSightPeripheral;
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
    }

    private bool isThereClearLineOfSight(int x, int y, int numberMarkers)
    {
        Vector3 position = new Vector3(x, y, 0);
        if (numberMarkers <= range && (gameManager.obstacleGrid.GetGridObject(position) == null || 
            gameManager.obstacleGrid.GetGridObject(position).blockLineOfSight == false))
        {
            return true;
        }
        clearLineOfSightToEnemy = false;
        return false;
    }
    private bool isThereClearLineOfSightPeripheral(int x, int y, int numberMarkers)
    {
        Vector3 position = new Vector3(x, y, 0);
        gameManager = mainGameManger.GetGameManger(position);
        if (numberMarkers <= range && (gameManager.obstacleGrid.GetGridObject(position) == null || 
            gameManager.obstacleGrid.GetGridObject(position).blockLineOfSight == false))
        {
            return true;
        }
        clearLineOfSightToEnemy = false;
        return false;
    }
}
