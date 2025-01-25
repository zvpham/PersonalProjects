using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TargetingSystem : MonoBehaviour
{
    public abstract void SelectNewPosition(Vector2Int currentlySelectedHex);
    public abstract void EndTargeting();

    public abstract void DeactivateTargetingSystem();

    public abstract void NextItem();

    public abstract void PreviousItem();

    public bool CanUnitMove(Unit movingUnit, int numActionPoints, int amountMoved, int currentMoveSpeed, int moveCostOverride = -1,
        int[,] moveCostGridOveride = null)
    {
        if(numActionPoints >= amountMoved + 1)
        {
            return true;
        }

        if(numActionPoints < 0)
        {
            return false;
        }

        int[,] moveCostGrid;
        if(moveCostGridOveride == null)
        {
            moveCostGrid = movingUnit.gameManager.moveCostMap;
        }
        else
        {
            moveCostGrid = moveCostGridOveride;
        }

        if (currentMoveSpeed > 0)
        {
            if(moveCostOverride == -1)
            {
                List<DijkstraMapNode> neighborHexes = movingUnit.gameManager.map.getGrid().GetGridObjectsInRing(movingUnit.x, 
                    movingUnit.y, 1);
                for (int i = 0; i < neighborHexes.Count; i++)
                {
                    DijkstraMapNode neighborNode = neighborHexes[i];
                    if (moveCostGrid[neighborNode.y, neighborNode.x] <= currentMoveSpeed)
                    {
                        return true;
                    }
                }
            }
            else if (moveCostOverride <= currentMoveSpeed)
            {
                return true;
            }
        }
        return false;
    }
    
    public bool IsHexInRange(Unit movingUnit, Vector2Int startHex,  Vector2Int endHex, int maxDistance)
    {
        if(startHex == endHex)
        {
            return true;
        }
        GridHex<DijkstraMapNode> grid = movingUnit.gameManager.map.getGrid();
        Vector3Int startCube = grid.OffsetToCube(startHex);
        Vector3Int endCube =  grid.OffsetToCube(endHex);
        int distance = grid.CubeDistance(startCube, endCube);
        return (distance <= maxDistance);
    }
}
