using Bresenhams;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
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
            if (Vector3.Distance(self.transform.position, unit.transform.position) >= range * 2)
            {
                continue;
            }
            clearLineOfSightToEnemy = true;
            BresenhamsAlgorithm.PlotFunction plotFunction = isThereClearLineOfSight;
            BresenhamsAlgorithm.Line((int)self.transform.position.x, (int)self.transform.position.y, (int)unit.transform.position.x, (int)unit.transform.position.y, plotFunction);
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
                clearLineOfSightToEnemy = true;
                BresenhamsAlgorithm.PlotFunction plotFunction = isThereClearLineOfSightPeripheral;
                BresenhamsAlgorithm.Line((int)self.transform.position.x, (int)self.transform.position.y, (int)unit.transform.position.x, (int)unit.transform.position.y, plotFunction);
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

    private int DiagDistance(int x0, int y0,  int x1, int y1)
    {
        int dx = x1 - x0;
        int dy = y1 - y0; ;
        return math.max(math.abs(dx), math.abs(dy));
    }

    private float Lerp(float start, float end, float t)
    {
        return start + t * (end - start);
    }

    public override void PlayerUseSense(Player player)
    {
        GameManager gameManager = player.gameManager;
        int mapWidth =  gameManager.mainGameManger.mapWidth;
        int mapHeight = gameManager.mainGameManger.mapHeight;

        int x = (int) player.transform.position.x;
        int y = (int) player.transform.position.y;

        float currentDegree = 0;
        // 736, .049f
        for(int i = 0; i < 750; i++)
        {
            float degree = currentDegree * Mathf.Deg2Rad;
            currentDegree += 0.48f;
            //Finds Endpoint of Line
            int nx = (int) math.round(math.cos(degree) * range) + x;
            int ny = (int) math.round(math.sin(degree) * range) + y;

            //Finds Number of tiles between player and endpoint
            int distance = DiagDistance(x, y, nx, ny);
            // Loop through all of the tiles that the line goes through
            for(int j = 0; j < distance; j++)
            {
                int tx = (int)math.round(Lerp(x, nx, j/((float) distance)));
                int ty = (int)math.round(Lerp(y, ny, j / ((float)distance)));

                // Move to next tile if line is not in Bounds
                if(tx < gameManager.defaultGridPosition.x || ty < gameManager.defaultGridPosition.y || 
                    tx >= gameManager.defaultGridPosition.x + mapWidth || ty >= gameManager.defaultGridPosition.y + mapHeight)
                {
                    continue;
                }

                //Set tile in line to Seen in GameManager;
                Vector3 tilePosition = new Vector3(tx, ty, 0);
                Wall wall = gameManager.obstacleGrid.GetGridObject(tilePosition);
                gameManager.tilesBeingActivelySeen.Add(new Vector2Int(tx, ty));
                gameManager.tileVisibilityStates[(int)(tx - gameManager.defaultGridPosition.x),
                    (int)(ty - gameManager.defaultGridPosition.y)] = 2;
                gameManager.spriteGrid.GetGridObject(tilePosition).sprites[0].color = new Color(0, 0, 0, gameManager.KnownSpaceAlpha);

                // If tile is wall stop early.
                if (wall != null && wall.blockLineOfSight)
                {
                    break;
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
