using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/MeleeAttack")]
public class MeleeAttack : Action
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
            for (int i = 1; i <= self.moveSpeed + 1; i++)
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

                    Unit targetUnit = self.gameManager.grid.GetGridObject(currentNodePosition.x, currentNodePosition.y).unit;
                    if (targetUnit != null && targetUnit.team != self.team && map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= self.moveSpeed + 1)
                    {
                        self.gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 5);
                        self.higlightedPositions.Add(currentNodePosition);
                    }
                }
            }
        }
        else if (self.currentActionsPoints == 1)
        {
            List<DijkstraMapNode> mapNodes = self.gameManager.map.getGrid().GetGridObjectsInRing(x, y, 1);
            for (int j = 0; j < mapNodes.Count; j++)
            {
                Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                Unit targetUnit = self.gameManager.grid.GetGridObject(currentNodePosition.x, currentNodePosition.y).unit;
                if (targetUnit != null && targetUnit.team != self.team && map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= self.moveSpeed + 1)
                {
                    self.gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 5);
                    self.higlightedPositions.Add(currentNodePosition);
                }
            }
            
        }
        self.gameManager.spriteManager.ActivateMeleeAttackTargeting(self, new Vector2Int(x, y), false, self.currentActionsPoints, 1);
        self.gameManager.spriteManager.meleeTargeting.OnFoundTarget += FoundTarget;
    }

    public override void DeselectAction(Unit self)
    {
        for (int i = 0; i < self.higlightedPositions.Count; i++)
        {
            self.gameManager.spriteManager.ChangeTile(self.higlightedPositions[i], 1, 0);
        }
        self.higlightedPositions = new List<Vector2Int>();
    }

    public void FoundTarget(List<Vector2Int> path, Unit movingUnit, bool foundTarget)
    {
        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
    }

    public override void Activate(Unit self)
    {
        throw new System.NotImplementedException();
    }

    public override void PlayerActivate(Unit self)
    {
        throw new System.NotImplementedException();
    }   
}
