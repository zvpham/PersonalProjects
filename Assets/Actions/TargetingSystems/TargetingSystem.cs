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

        int[,] moveCostGrid;
        if(moveCostGridOveride == null)
        {
            moveCostGrid = movingUnit.gameManager.spriteManager.elevationOfHexes;
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
}
