using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/EnemyMovement")]
public class EnemyMovement : Action
{
    public override void Activate(Unit self)
    {
        AStarPathfinding path = self.gameManager.mainGameManger.path;
        List<AStarPathNode> movementPath = path.FindPath(self.transform.position, self.enemyList[self.closestEnemyIndex].transform.position, true);
        if (movementPath != null && movementPath.Count > 1)
        {
            Vector2 newPosition = movementPath[1].grid.GetWorldPosition(movementPath[1].x, movementPath[1].y) - movementPath[0].grid.GetWorldPosition(movementPath[0].x, movementPath[0].y);
            Move.Movement(self, newPosition, self.gameManager, false);
        }
        else if (movementPath == null)
        {
            List<Vector3> temp = FindNearestEmptySpaceWithPath.FindEmptySpace(self.enemyList[self.closestEnemyIndex], self, true);
            if (temp != null && temp.Count > 1)
            {
                Vector2 newPosition = temp[1] - temp[0];
                Move.Movement(self, newPosition, self.gameManager, false);
            }
        }
        self.TurnEnd();
    }

    public override void Activate(Unit self, Vector3 targetLocation)
    {
        AStarPathfinding path = self.gameManager.mainGameManger.path;
        List<AStarPathNode> movementPath = path.FindPath(self.transform.position, targetLocation, true);
        if (movementPath != null && movementPath.Count > 1)
        {
            Vector2 newPosition = movementPath[1].grid.GetWorldPosition(movementPath[1].x, movementPath[1].y) - movementPath[0].grid.GetWorldPosition(movementPath[0].x, movementPath[0].y);
            Move.Movement(self, newPosition, self.gameManager, false, true);
        }
        else if (movementPath == null)
        {
            List<Vector3> temp = FindNearestEmptySpaceWithPath.FindEmptySpace(self.enemyList[self.closestEnemyIndex], self, true);
            if (temp != null && temp.Count > 1)
            {
                Vector2 newPosition = temp[1] - temp[0];
                Move.Movement(self, newPosition, self.gameManager, false, true);
            }
        }
        self.TurnEnd();
    }

    public override int CalculateWeight(Unit self)
    {
        return weight;
    }

    public override int CalculateWeight(Unit self, Vector3 targetLocation)
    {
        return weight;
    }

    public override void PlayerActivate(Unit self)
    {
        throw new System.NotImplementedException();
    }
}
