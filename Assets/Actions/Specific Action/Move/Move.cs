using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Move")]
public class Move : Action
{
    public override void SelectAction(Unit self)
    {
        base.SelectAction(self);
        DijkstraMap map = self.gameManager.map;
        map.getGrid().GetXY(self.transform.position, out int x, out int y);
        map.ResetMap();
        map.SetGoals(new List<Vector2Int>() { new Vector2Int(x, y) });

        self.firstRangeBracket = new List<Vector2Int>();
        self.secondRangeBracket = new List<Vector2Int>();
        if (self.currentActionsPoints == 2)
        {
            for (int i = 1; i <= self.moveSpeed * 2; i++)
            {
                List<DijkstraMapNode> mapNodes = self.gameManager.map.getGrid().GetGridObjectsInRing(x, y, i);
                for (int j = 0; j < mapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                    if (map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= self.moveSpeed)
                    {
                        self.gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 2);
                        self.higlightedPositions.Add(currentNodePosition);
                        self.firstRangeBracket.Add(new Vector2Int(mapNodes[j].x, mapNodes[j].y));
                    }
                    else if (map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= self.moveSpeed * 2)
                    {
                        self.gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 4);
                        self.higlightedPositions.Add(currentNodePosition);
                        self.secondRangeBracket.Add(new Vector2Int(mapNodes[j].x, mapNodes[j].y));
                    }
                }
            }
        }
        else if (self.currentActionsPoints == 1)
        {
            for (int i = 1; i <= self.moveSpeed; i++)
            {
                List<DijkstraMapNode> mapNodes = self.gameManager.map.getGrid().GetGridObjectsInRing(x, y, i);
                for (int j = 0; j < mapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                    if (map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= self.moveSpeed)
                    {
                        self.gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 4);
                        self.higlightedPositions.Add(currentNodePosition);
                        self.firstRangeBracket.Add(new Vector2Int(mapNodes[j].x, mapNodes[j].y));
                    }
                }
            }
        }
        self.gameManager.spriteManager.ActivateMovementTargeting(self, new Vector2Int(x, y), false, self.currentActionsPoints);
        self.gameManager.spriteManager.movementTargeting.OnFoundTarget += FoundTarget;
    }
    public override void DeselectAction(Unit self)
    {
        for(int i = 0; i < self.higlightedPositions.Count; i++)
        {
            self.gameManager.spriteManager.ChangeTile(self.higlightedPositions[i], 1, 0);
        }
        self.higlightedPositions = new List<Vector2Int>();
    }

    public void FoundTarget(List<Vector2Int> path, Unit movingUnit, bool foundTarget)
    {
        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
        if (foundTarget)
        {
            DijkstraMap map = movingUnit.gameManager.map;
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 newPosition = map.getGrid().GetWorldPosition(path[i].x, path[i].y);
                movingUnit.MovePositions(movingUnit.transform.position, newPosition);
                movingUnit.transform.position = newPosition;
            }
            DeselectAction(movingUnit);
            if (movingUnit.secondRangeBracket.Contains(path[path.Count - 1]))
            {
                movingUnit.UseActionPoints(2);
            }
            else
            {
                movingUnit.UseActionPoints(1);
            }
        }
        else
        {
            DeselectAction(movingUnit);
        }
    }

    public override void Activate(Unit self)
    {

    }

    public override void PlayerActivate(Unit self)
    {
        throw new System.NotImplementedException();
    }
}
