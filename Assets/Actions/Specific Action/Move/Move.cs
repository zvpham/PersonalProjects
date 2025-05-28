using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Move")]
public class Move : Action
{
    public override int CalculateWeight(AIActionData AIActionData)
    {
        return -2;  
    }

    public override void FindOptimalPosition(AIActionData AIActionData)
    {
        return;
    }

    public override bool CheckIfActionIsInRange(AIActionData AIActionData)
    {
        return false;
    }

    public override bool CanMove(AIActionData AIActionData)
    {
        Unit movingUnit = AIActionData.unit;
        int x = movingUnit.x;
        int y = movingUnit.y;

        CombatGameManager gameManager = movingUnit.gameManager;
        List<PassiveEffectArea>[,] passives = new List<PassiveEffectArea>[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < gameManager.mapSize; i++)
        {
            for (int j = 0; j < gameManager.mapSize; j++)
            {
                passives[i, j] = new List<PassiveEffectArea>();
            }
        }

        List<List<PassiveEffectArea>> classifiedPassiveEffectArea = movingUnit.CalculuatePassiveAreas();
        bool[,] unwalkablePassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];

        for (int i = 0; i < classifiedPassiveEffectArea[0].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[0][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[0][i].passiveLocations[j];
                unwalkablePassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[0][i]);
            }
        }

        bool[,] badWalkInPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < classifiedPassiveEffectArea[1].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[1][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[1][i].passiveLocations[j];
                badWalkInPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[1][i]);
            }
        }

        bool[,] goodWalkinPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < classifiedPassiveEffectArea[2].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[2][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[2][i].passiveLocations[j];
                goodWalkinPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[2][i]);
            }
        }


        int usableActionPoints = movingUnit.currentMajorActionsPoints;

        int startValue = movingUnit.currentMoveSpeed + (movingUnit.moveSpeedPerMoveAction * usableActionPoints);
        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
        gameManager.map.ResetMap(false, false);
        List<DijkstraMapNode> nodesInMovementRange = gameManager.map.GetNodesInMovementRange(x, y, startValue, movingUnit.moveModifier, gameManager, badWalkInPassivesValues);
        GridHex<GridPosition> grid = gameManager.grid;
        List<DijkstraMapNode> emptyNodes = new List<DijkstraMapNode>(); 
        for (int i = 0; i < nodesInMovementRange.Count; i++)
        {
            x = nodesInMovementRange[i].x;
            y = nodesInMovementRange[i].y;

            if (grid.GetGridObject(x, y).unit == null)
            {
                emptyNodes.Add(nodesInMovementRange[i]);
            }
        }
        return emptyNodes.Count > 0;
    }

    public override Tuple<int, Vector2Int, List<Action>, List<Vector2Int>> MoveUnitToEnemyUnits(AIActionData AiActionData, bool IgnorePassives = false)
    {
        CombatGameManager gameManager = AiActionData.unit.gameManager;
        DijkstraMap map = gameManager.map;
        Unit movingUnit = AiActionData.unit;
        map.ResetMap(true);

        bool[,] badWalkInPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        if (!IgnorePassives)
        {
            List<PassiveEffectArea>[,] passives = new List<PassiveEffectArea>[gameManager.mapSize, gameManager.mapSize];
            for (int i = 0; i < gameManager.mapSize; i++)
            {
                for (int j = 0; j < gameManager.mapSize; j++)
                {
                    passives[i, j] = new List<PassiveEffectArea>();
                }
            }

            List<List<PassiveEffectArea>> classifiedPassiveEffectArea = movingUnit.CalculuatePassiveAreas();
            bool[,] unwalkablePassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];

            for (int i = 0; i < classifiedPassiveEffectArea[0].Count; i++)
            {
                for (int j = 0; j < classifiedPassiveEffectArea[0][i].passiveLocations.Count; j++)
                {
                    Vector2Int passiveLocation = classifiedPassiveEffectArea[0][i].passiveLocations[j];
                    unwalkablePassivesValues[passiveLocation.x, passiveLocation.y] = true;
                    passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[0][i]);
                }
            }

            for (int i = 0; i < classifiedPassiveEffectArea[1].Count; i++)
            {
                for (int j = 0; j < classifiedPassiveEffectArea[1][i].passiveLocations.Count; j++)
                {
                    Vector2Int passiveLocation = classifiedPassiveEffectArea[1][i].passiveLocations[j];
                    badWalkInPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                    passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[1][i]);
                }
            }

            bool[,] goodWalkinPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
            for (int i = 0; i < classifiedPassiveEffectArea[2].Count; i++)
            {
                for (int j = 0; j < classifiedPassiveEffectArea[2][i].passiveLocations.Count; j++)
                {
                    Vector2Int passiveLocation = classifiedPassiveEffectArea[2][i].passiveLocations[j];
                    goodWalkinPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                    passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[2][i]);
                }
            }
        }
      
        AiActionData.unit.moveModifier.SetUnwalkable(gameManager, AiActionData.unit);

        List<Vector2Int> goals = new List<Vector2Int>();
        for (int i = 0; i < AiActionData.enemyUnits.Count; i++)
        {
            Vector2Int enemyUnitPosition = AiActionData.enemyUnits[i];
            List<GridPosition> potentialGoals = gameManager.grid.GetGridObjectsInRing(enemyUnitPosition.x, enemyUnitPosition.y, 1);
            for (int j = 0; j < potentialGoals.Count; j++)
            {
                Vector2Int potentialGoalPosition = new Vector2Int(potentialGoals[j].x, potentialGoals[j].y);
                if (gameManager.grid.GetGridObject(potentialGoalPosition).CheckIfTileIsEmpty() &&
                    movingUnit.moveModifier.validElevationDifference(gameManager, enemyUnitPosition, potentialGoalPosition, 1) && !goals.Contains(potentialGoalPosition))
                {
                    goals.Add(potentialGoalPosition);
                }
            }
        }

        if (goals.Count == 0)
        {
            goals = AiActionData.enemyUnits;
        }


        if(!IgnorePassives)
        {
            map.SetGoalsNew(goals, gameManager, AiActionData.unit.moveModifier, badWalkInPassivesValues);
        }
        else
        {
            map.SetGoalsNew(goals, gameManager, AiActionData.unit.moveModifier);
        }
        int x = movingUnit.x;
        int y = movingUnit.y;
        List<Vector2Int> path = new List<Vector2Int>();
        path.Clear();

        int initialActionPoints = movingUnit.currentMajorActionsPoints;

        int currentMoveSpeed = movingUnit.currentMoveSpeed + (movingUnit.moveSpeedPerMoveAction * initialActionPoints);
        int startMoveSpeed = currentMoveSpeed;
        int previousNodeMoveValue = map.getGrid().GetGridObject(x, y).value;
        DijkstraMapNode currentNode;
        while (true)
        {
            currentNode = map.GetLowestNearbyNode(x, y, movingUnit.moveModifier, gameManager);
            x = currentNode.x;
            y = currentNode.y;
            int currentNodeMoveValue = currentNode.value;
            int moveSpeedDifference = previousNodeMoveValue - currentNodeMoveValue;
            if (currentNode.value == int.MaxValue || currentMoveSpeed < moveSpeedDifference)
                break;
            currentMoveSpeed -= moveSpeedDifference;
            previousNodeMoveValue = currentNodeMoveValue;
            path.Add(new Vector2Int(x, y));

            if (AiActionData.enemyUnits.Contains(path[path.Count - 1]))
            {
                break;
            }
            else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
            {
                path.RemoveAt(path.Count - 1);
                break;
            }
        }

        // Check to see if reason Unit can't' find path is due to badWalkInSpacess.
        if (currentNode.value == int.MaxValue)
        {
            //attempt to path to goal
            map.ResetMap(true);
            {
                map.SetGoalsNew(goals, gameManager, AiActionData.unit.moveModifier);
            }
            x = movingUnit.x;
            y = movingUnit.y;
            previousNodeMoveValue = map.getGrid().GetGridObject(x, y).value;
            while (true)
            {
                currentNode = map.GetLowestNearbyNode(x, y, movingUnit.moveModifier, gameManager);
                x = currentNode.x;
                y = currentNode.y;
                int currentNodeMoveValue = currentNode.value;
                int moveSpeedDifference = previousNodeMoveValue - currentNodeMoveValue;
                if (currentNode.value == int.MaxValue || currentMoveSpeed < moveSpeedDifference)
                    break;
                currentMoveSpeed -= moveSpeedDifference;
                previousNodeMoveValue = currentNodeMoveValue;
                path.Add(new Vector2Int(x, y));

                if (AiActionData.enemyUnits.Contains(path[path.Count - 1]))
                {
                    break;
                }
                else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
                {
                    path.RemoveAt(path.Count - 1);
                    break;
                }
            }
            // Can't find node because Unit is stuck in elevation
            if(currentNode.value == int.MaxValue)
            {
                Debug.LogWarning("Test Unit Isolated on high terrain while they can't see enemyUnits: " + currentNode);
                return new Tuple<int, Vector2Int, List<Action>, List<Vector2Int>>(int.MaxValue, Vector2Int.zero, null, new List<Vector2Int>());
            }
        }
        
        if (path.Count > 0)
        {
            x = path[path.Count - 1].x;
            y = path[path.Count - 1].y;
        }

        // Desired end position has a Unit on it, use modified path where friendly units are walkable
        if (gameManager.grid.GetGridObject(x, y).unit != null)
        {
            List<DijkstraMapNode> tempNewGoals = map.GetNodesInMovementRangeNoChangeGrid(movingUnit.x, movingUnit.y, startMoveSpeed);
            if (tempNewGoals == null)
            {
                Debug.LogWarning("Couldn't find nodes in movement Range for AI: " + movingUnit);
                return new Tuple<int, Vector2Int, List<Action>, List<Vector2Int>>(int.MaxValue, Vector2Int.zero, null, new List<Vector2Int>());
            }
            GridHex<GridPosition> combatGrid = gameManager.grid;
            int lowestNodeValue = int.MaxValue;
            DijkstraMapNode currentNodeGoal = null;
            for (int i = 0; i < tempNewGoals.Count; i++)
            {
                DijkstraMapNode node = tempNewGoals[i];
                if (combatGrid.GetGridObject(node.x, node.y).unit == null && node.value < lowestNodeValue)
                {
                    currentNodeGoal = node;
                    lowestNodeValue = node.value;
                }
            }

            // if new goal makes is further than starting position -  End Turn
            if (currentNodeGoal != null && currentNodeGoal.value >= map.getGrid().GetGridObject(movingUnit.x, movingUnit.y).value)
            {
                return new Tuple<int, Vector2Int, List<Action>, List<Vector2Int>>(int.MaxValue, Vector2Int.zero, null, new List<Vector2Int>());
            }
            List<Vector2Int> newGoal = new List<Vector2Int>() { new Vector2Int(currentNodeGoal.x, currentNodeGoal.y) };

            map.ResetMap();
            map.SetGoalsNew(newGoal, gameManager, AiActionData.unit.moveModifier);

            x = movingUnit.x;
            y = movingUnit.y;
            currentMoveSpeed = startMoveSpeed;
            path = new List<Vector2Int>();
            path.Clear();
            previousNodeMoveValue = map.getGrid().GetGridObject(x, y).value;
            while (true)
            {
                currentNode = map.GetLowestNearbyNode(x, y, movingUnit.moveModifier, gameManager);
                x = currentNode.x;
                y = currentNode.y;
                int currentNodeMoveValue = currentNode.value;
                int moveSpeedDifference = previousNodeMoveValue - currentNodeMoveValue;
                if (currentNode.value == int.MaxValue || currentMoveSpeed < moveSpeedDifference)
                {
                    //Debug.LogWarning("Break 1: " + x + ", " + y + ", " + currentNode.value + ", " + currentMoveSpeed + ", " + moveSpeedDifference);
                    break;
                }
                currentMoveSpeed -= moveSpeedDifference;
                previousNodeMoveValue = currentNodeMoveValue;
                path.Add(new Vector2Int(x, y));

                if (AiActionData.enemyUnits.Contains(path[path.Count - 1]))
                {
                    //Debug.LogWarning("Break 2: " + x + ", " + y);
                    break;
                }
                else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
                {
                    path.RemoveAt(path.Count - 1);
                    Debug.LogWarning("Break 3: " + x + ", " + y);
                    break;
                }
            }

            if (path.Count == 0)
            {
                Debug.LogWarning("Path not found when attempting to approach enemyUnits for: " + movingUnit.name);
            }
        }


        if (path.Count != 0)
        {
            return new Tuple<int, Vector2Int, List<Action>, List<Vector2Int>>(map.getGrid().GetGridObject(path[path.Count - 1]).value, path[path.Count - 1], 
                new List<Action> { this }, new List<Vector2Int>() { new Vector2Int(movingUnit.x, movingUnit.y) });
        }
        else
        {
            return new Tuple<int, Vector2Int, List<Action>, List<Vector2Int>>(int.MaxValue, Vector2Int.zero, null, new List<Vector2Int>());
        }
    }

    public override void AIUseAction(AIActionData AIActionData, bool finalAction = false)
    {
        Vector2Int endPosition = AIActionData.desiredEndPosition;
        int movementActionIndex = AIActionData.movementActions[endPosition.x, endPosition.y].IndexOf(this);
        CombatGameManager gameManager = AIActionData.unit.gameManager;
        Unit movingUnit = AIActionData.unit;
        bool ignorePassiveArea = AIActionData.ignorePassiveArea[endPosition.x, endPosition.y];
        bool[,] unwalkablePassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        bool[,] badWalkInPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        bool[,] goodWalkinPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        if (!ignorePassiveArea)
        {
            List<PassiveEffectArea>[,] passives = new List<PassiveEffectArea>[gameManager.mapSize, gameManager.mapSize];
            for (int i = 0; i < gameManager.mapSize; i++)
            {
                for (int j = 0; j < gameManager.mapSize; j++)
                {
                    passives[i, j] = new List<PassiveEffectArea>();
                }
            }

            List<List<PassiveEffectArea>> classifiedPassiveEffectArea = movingUnit.CalculuatePassiveAreas();
            for (int i = 0; i < classifiedPassiveEffectArea[0].Count; i++)
            {
                for (int j = 0; j < classifiedPassiveEffectArea[0][i].passiveLocations.Count; j++)
                {
                    Vector2Int passiveLocation = classifiedPassiveEffectArea[0][i].passiveLocations[j];
                    unwalkablePassivesValues[passiveLocation.x, passiveLocation.y] = true;
                    passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[0][i]);
                }
            }

            for (int i = 0; i < classifiedPassiveEffectArea[1].Count; i++)
            {
                for (int j = 0; j < classifiedPassiveEffectArea[1][i].passiveLocations.Count; j++)
                {
                    Vector2Int passiveLocation = classifiedPassiveEffectArea[1][i].passiveLocations[j];
                    badWalkInPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                    passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[1][i]);
                }
            }

            for (int i = 0; i < classifiedPassiveEffectArea[2].Count; i++)
            {
                for (int j = 0; j < classifiedPassiveEffectArea[2][i].passiveLocations.Count; j++)
                {
                    Vector2Int passiveLocation = classifiedPassiveEffectArea[2][i].passiveLocations[j];
                    goodWalkinPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                    passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[2][i]);
                }
            }
        }
        

        //Set Proper start position based on where the moveAction is in MovementActions
        Vector2Int currentEndPosition;
        Vector2Int startingPosition = AIActionData.startPositions[endPosition.x, endPosition.y][movementActionIndex];
        if (movementActionIndex + 1 == AIActionData.movementActions[endPosition.x, endPosition.y].Count)
        {
            currentEndPosition = endPosition;
        }
        else
        {
            currentEndPosition = AIActionData.startPositions[endPosition.x, endPosition.y][movementActionIndex + 1];
        }

        AIActionData.unit.gameManager.map.ResetMap(true);
        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
        if(!ignorePassiveArea)
        {
            AIActionData.unit.gameManager.map.SetGoalsNew(new List<Vector2Int>() { currentEndPosition }, gameManager, movingUnit.moveModifier, badWalkInPassivesValues);
        }   
        else
        {
            AIActionData.unit.gameManager.map.SetGoalsNew(new List<Vector2Int>() { currentEndPosition }, gameManager, movingUnit.moveModifier);
        }
        DijkstraMap map = gameManager.map;
        int x = startingPosition.x;
        int y = startingPosition.y;
        int previousNodeMoveValue = map.getGrid().GetGridObject(x, y).value;

        int initialActionPoints = AIActionData.expectedCurrentActionPoints;

        int currentMoveSpeed = AIActionData.expectedInitialMoveSpeed + movingUnit.moveSpeedPerMoveAction * initialActionPoints;
        bool foundEndPosition = false;
        List<Vector2Int> path = new List<Vector2Int>();
        while (true)
        {
            DijkstraMapNode currentNode;
            currentNode = map.GetLowestNearbyNode(x, y, currentEndPosition, movingUnit.moveModifier, gameManager);
            x = currentNode.x;
            y = currentNode.y;
            int currentNodeMoveValue = currentNode.value;
            int moveSpeedDifference = previousNodeMoveValue - currentNodeMoveValue;
            if (currentNode.value == int.MaxValue || currentMoveSpeed < moveSpeedDifference)
                break;
            currentMoveSpeed -= moveSpeedDifference;
            previousNodeMoveValue = currentNodeMoveValue;
            path.Add(new Vector2Int(x, y));

            if (currentEndPosition == path[path.Count - 1])
            {
                foundEndPosition = true;
                break;
            }
            else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
            {
                path.RemoveAt(path.Count - 1);
                foundEndPosition = false;
                break;
            }
        }

        if (foundEndPosition)
        {
            ActionData actionData = new ActionData();
            actionData.action = this;
            actionData.actingUnit = movingUnit;
            actionData.originLocation = new Vector2Int(movingUnit.x, movingUnit.y);

            List<Vector2Int> tempPath = new List<Vector2Int>() { path[0] };
            actionData.path = tempPath;
            movingUnit.gameManager.AddActionToQueue(actionData, false, false);


            for (int i = 1; i < path.Count; i++)
            {
                actionData = new ActionData();
                actionData.action = this;
                actionData.actingUnit = movingUnit;
                actionData.originLocation = new Vector2Int(path[i - 1].x, path[i - 1].y);

                tempPath = new List<Vector2Int>() { path[i] };
                actionData.path = tempPath;
                movingUnit.gameManager.AddActionToQueue(actionData, false, false);
            }
            if (finalAction)
            {
                movingUnit.gameManager.PlayActions();
            }
        }
        else
        {
            Debug.LogError("Couldn't find desiered end position when: " + AIActionData.unit + ", tried to move");
        }
    }

    public void MoveIsOnlyActionAIAction(AIActionData aIActionData)
    {
        Vector2Int endPosition = aIActionData.desiredEndPosition;
        CombatGameManager gameManager = aIActionData.unit.gameManager;
        Unit movingUnit = aIActionData.unit;
        Vector2Int currentPosition = new Vector2Int(movingUnit.x, movingUnit.y);
        if (endPosition == currentPosition)
        {
           movingUnit.EndTurnAction();
           return;
        }
        
        bool ignorePassiveArea = aIActionData.ignorePassiveArea[endPosition.x, endPosition.y];
        bool[,] unwalkablePassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        bool[,] badWalkInPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        bool[,] goodWalkinPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        if (!ignorePassiveArea)
        {
            List<PassiveEffectArea>[,] passives = new List<PassiveEffectArea>[gameManager.mapSize, gameManager.mapSize];
            for (int i = 0; i < gameManager.mapSize; i++)
            {
                for (int j = 0; j < gameManager.mapSize; j++)
                {
                    passives[i, j] = new List<PassiveEffectArea>();
                }
            }

            List<List<PassiveEffectArea>> classifiedPassiveEffectArea = movingUnit.CalculuatePassiveAreas();
            for (int i = 0; i < classifiedPassiveEffectArea[0].Count; i++)
            {
                for (int j = 0; j < classifiedPassiveEffectArea[0][i].passiveLocations.Count; j++)
                {
                    Vector2Int passiveLocation = classifiedPassiveEffectArea[0][i].passiveLocations[j];
                    unwalkablePassivesValues[passiveLocation.x, passiveLocation.y] = true;
                    passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[0][i]);
                }
            }

            for (int i = 0; i < classifiedPassiveEffectArea[1].Count; i++)
            {
                for (int j = 0; j < classifiedPassiveEffectArea[1][i].passiveLocations.Count; j++)
                {
                    Vector2Int passiveLocation = classifiedPassiveEffectArea[1][i].passiveLocations[j];
                    badWalkInPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                    passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[1][i]);
                }
            }

            for (int i = 0; i < classifiedPassiveEffectArea[2].Count; i++)
            {
                for (int j = 0; j < classifiedPassiveEffectArea[2][i].passiveLocations.Count; j++)
                {
                    Vector2Int passiveLocation = classifiedPassiveEffectArea[2][i].passiveLocations[j];
                    goodWalkinPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                    passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[2][i]);
                }
            }
        }


        //Set Proper start position based on where the moveAction is in MovementActions
        Vector2Int currentEndPosition = endPosition;
        Vector2Int startingPosition = new Vector2Int(aIActionData.unit.x, aIActionData.unit.y);

        aIActionData.unit.gameManager.map.ResetMap(true);
        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
        if (!ignorePassiveArea)
        {
            aIActionData.unit.gameManager.map.SetGoalsNew(new List<Vector2Int>() { currentEndPosition }, gameManager, movingUnit.moveModifier, badWalkInPassivesValues);
        }
        else
        {
            aIActionData.unit.gameManager.map.SetGoalsNew(new List<Vector2Int>() { currentEndPosition }, gameManager, movingUnit.moveModifier);
        }
        DijkstraMap map = gameManager.map;
        int x = startingPosition.x;
        int y = startingPosition.y;
        int previousNodeMoveValue = map.getGrid().GetGridObject(x, y).value;

        int initialActionPoints = aIActionData.expectedCurrentActionPoints;

        int currentMoveSpeed = aIActionData.expectedInitialMoveSpeed + movingUnit.moveSpeedPerMoveAction * initialActionPoints;
        List<Vector2Int> path = new List<Vector2Int>();
        while (true)
        {
            DijkstraMapNode currentNode;
            currentNode = map.GetLowestNearbyNode(x, y, currentEndPosition, movingUnit.moveModifier, gameManager);
            x = currentNode.x;
            y = currentNode.y;
            int currentNodeMoveValue = currentNode.value;
            int moveSpeedDifference = previousNodeMoveValue - currentNodeMoveValue;
            if (currentNode.value == int.MaxValue || currentMoveSpeed < moveSpeedDifference)
                break;
            currentMoveSpeed -= moveSpeedDifference;
            previousNodeMoveValue = currentNodeMoveValue;
            path.Add(new Vector2Int(x, y));

            if (currentEndPosition == path[path.Count - 1])
            {
                break;
            }
            else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
            {
                path.RemoveAt(path.Count - 1);
                break;
            }
        }

        ActionData actionData = new ActionData();
        actionData.action = this;
        actionData.actingUnit = movingUnit;
        actionData.originLocation = new Vector2Int(movingUnit.x, movingUnit.y);

        List<Vector2Int> tempPath = new List<Vector2Int>() { path[0] };
        actionData.path = tempPath;
        movingUnit.gameManager.AddActionToQueue(actionData, false, false);


        for (int i = 1; i < path.Count; i++)
        {
            actionData = new ActionData();
            actionData.action = this;
            actionData.actingUnit = movingUnit;
            actionData.originLocation = new Vector2Int(path[i - 1].x, path[i - 1].y);

            tempPath = new List<Vector2Int>() { path[i] };
            actionData.path = tempPath;
            movingUnit.gameManager.AddActionToQueue(actionData, false, false);
        }
        movingUnit.gameManager.PlayActions();
    }

    public override void CalculateActionConsequences(AIActionData AiActionData, Vector2Int desiredEndPosition)
    {
        bool ignorePassives = AiActionData.ignorePassiveArea[desiredEndPosition.x, desiredEndPosition.y];

        Vector2Int endPosition = desiredEndPosition;
        int movementActionIndex = AiActionData.movementActions[endPosition.x, endPosition.y].IndexOf(this);
        CombatGameManager gameManager = AiActionData.unit.gameManager;
        Unit movingUnit = AiActionData.unit;
        bool[,] unwalkablePassivesValues = AiActionData.unwalkablePassivesValues;
        bool[,] badWalkInPassivesValues = AiActionData.badWalkInPassivesValues;
        bool[,] goodWalkinPassivesValues = AiActionData.goodWalkinPassivesValues;
        List<PassiveEffectArea>[,] passives = AiActionData.passives;

        //Set Proper start position based on where the moveAction is in MovementActions
        Vector2Int currentEndPosition;
        Vector2Int startingPosition = AiActionData.startPositions[endPosition.x, endPosition.y][movementActionIndex];
        if (movementActionIndex + 1 == AiActionData.movementActions[endPosition.x, endPosition.y].Count)
        {
            currentEndPosition = endPosition;
        }
        else
        {
            currentEndPosition = AiActionData.startPositions[endPosition.x, endPosition.y][movementActionIndex + 1];
        }

        AiActionData.unit.gameManager.map.ResetMap(true);
        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
        if (!ignorePassives)
        {
            AiActionData.unit.gameManager.map.SetGoalsNew(new List<Vector2Int>() { currentEndPosition }, gameManager, movingUnit.moveModifier, badWalkInPassivesValues);
        }
        else
        {
            AiActionData.unit.gameManager.map.SetGoalsNew(new List<Vector2Int>() { currentEndPosition }, gameManager, movingUnit.moveModifier);
        }
        DijkstraMap map = gameManager.map;
        int x = startingPosition.x;
        int y = startingPosition.y;
        int previousNodeMoveValue = map.getGrid().GetGridObject(x, y).value;

        int initialActionPoints = AiActionData.expectedCurrentActionPoints;

        int currentMoveSpeed = AiActionData.expectedInitialMoveSpeed + movingUnit.moveSpeedPerMoveAction * initialActionPoints;
        bool foundEndPosition = false;
        List<Vector2Int> path = new List<Vector2Int>();
        while (true)
        {
            DijkstraMapNode currentNode;
            currentNode = map.GetLowestNearbyNode(x, y, currentEndPosition, movingUnit.moveModifier, gameManager);
            x = currentNode.x;
            y = currentNode.y;
            int currentNodeMoveValue = currentNode.value;
            int moveSpeedDifference = previousNodeMoveValue - currentNodeMoveValue;
            if (currentNode.value == int.MaxValue || currentMoveSpeed < moveSpeedDifference)
                break;
            currentMoveSpeed -= moveSpeedDifference;
            previousNodeMoveValue = currentNodeMoveValue;
            path.Add(new Vector2Int(x, y));

            if (currentEndPosition == path[path.Count - 1])
            {
                foundEndPosition = true;
                break;
            }
            else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
            {
                path.RemoveAt(path.Count - 1);
                foundEndPosition = false;
                break;
            }
        }

        if (foundEndPosition)
        {
            List<PassiveEffectArea> allPossibleActivatedPassives = new List<PassiveEffectArea>();
            for (int i = 0; i < path.Count; i++)
            {
                Vector2Int currentPosition = path[i];
                List<PassiveEffectArea> passivesOnLocation = passives[currentPosition.x, currentPosition.y];
                for(int j = 0; j < passivesOnLocation.Count; j++)
                {
                    if (!allPossibleActivatedPassives.Contains(passivesOnLocation[j]))
                    {
                        allPossibleActivatedPassives.Add(passivesOnLocation[j]);
                    }
                }
            }

            for(int i = 0; i < allPossibleActivatedPassives.Count;i++)
            {
                allPossibleActivatedPassives[i].CalculatePredictedActionConsequences(AiActionData, startingPosition, path);
            }
        }
        else
        {
            Debug.LogError("This Shouldn't be possible, predicting consequences for moving to a location and can't reach location");
        }
    }

    public override void GetMovementMap(AIActionData actionData)
    {
        CombatGameManager gameManager = actionData.unit.gameManager;
        Unit movingUnit = actionData.unit;
        int initialActionPoints = actionData.expectedCurrentActionPoints;

        actionData.unit.gameManager.map.ResetMap(true);
        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
        int startValue = (movingUnit.moveSpeedPerMoveAction * initialActionPoints);
        gameManager.map.ResetMap(false, false);
        List<DijkstraMapNode> mapNodes = actionData.unit.gameManager.map.GetNodesInMovementRange(actionData.originalPosition.x, actionData.originalPosition.y, startValue, movingUnit.moveModifier, gameManager);

        if (mapNodes.Count > 1)
        {
            actionData.canMove = true;
            int currentMoveSpeed = movingUnit.currentMoveSpeed;
            for (int i = 1; i < mapNodes.Count; i++)
            {
                DijkstraMapNode currentNode = mapNodes[i];
                int nodeValue = startValue - currentNode.value;
                int amountOfMoveActionsTaken;
                if (currentMoveSpeed > 0)
                {
                    int tempNodeValue = nodeValue - currentMoveSpeed;
                    if (tempNodeValue <= 0)
                    {
                        amountOfMoveActionsTaken = -1;
                    }
                    else
                    {
                        tempNodeValue -= 1;
                        amountOfMoveActionsTaken = tempNodeValue / (movingUnit.moveSpeedPerMoveAction);
                    }
                }
                else
                {
                    nodeValue -= 1;
                    amountOfMoveActionsTaken = (nodeValue / (movingUnit.moveSpeedPerMoveAction));
                }
                amountOfMoveActionsTaken += 1;
                int actionPointsUsed = 0;
                for (int j = 0; j < amountOfMoveActionsTaken; j++)
                {
                    actionPointsUsed += 1;
                }

                if (actionData.ignorePassiveArea[currentNode.x, currentNode.y] &&  actionPointsUsed < actionData.movementData[currentNode.x, currentNode.y])
                {
                    Vector2Int currentNodePosition = new Vector2Int(currentNode.x, currentNode.y);
                    int movementGridValue = actionData.movementData[currentNode.x, currentNode.y];
                    if(movementGridValue == int.MaxValue)
                    {
                        if(actionPointsUsed == 0)
                        {
                            actionData.hexesUnitCanMoveTo[0].Add(currentNodePosition);
                        }
                        else
                        {
                            actionData.hexesUnitCanMoveTo[actionPointsUsed - 1].Add(currentNodePosition);
                        }  
                    }
                    // if same then hexes can move to list is the same
                    else if(actionPointsUsed != movementGridValue)
                    {
                        actionData.hexesUnitCanMoveTo[movementGridValue - 1].Remove(currentNodePosition);
                        actionData.hexesUnitCanMoveTo[actionPointsUsed - 1].Add(currentNodePosition);
                    }

                    actionData.movementData[currentNode.x, currentNode.y] = actionPointsUsed;
                    actionData.movementActions[currentNode.x, currentNode.y] = new List<Action> { this };
                    actionData.startPositions[currentNode.x, currentNode.y] = new List<Vector2Int>() { new Vector2Int(movingUnit.x, movingUnit.y) };
                }
            }
        }


        List<PassiveEffectArea>[,] passives = new List<PassiveEffectArea>[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < gameManager.mapSize; i++)
        {
            for (int j = 0; j < gameManager.mapSize; j++)
            {
                passives[i, j] = new List<PassiveEffectArea>();
            }
        }

        List<List<PassiveEffectArea>> classifiedPassiveEffectArea = movingUnit.CalculuatePassiveAreas();
        bool[,] unwalkablePassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];

        for (int i = 0; i < classifiedPassiveEffectArea[0].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[0][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[0][i].passiveLocations[j];
                unwalkablePassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[0][i]);
            }
        }

        bool[,] badWalkInPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < classifiedPassiveEffectArea[1].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[1][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[1][i].passiveLocations[j];
                badWalkInPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[1][i]);
            }
        }

        bool[,] goodWalkinPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < classifiedPassiveEffectArea[2].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[2][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[2][i].passiveLocations[j];
                goodWalkinPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[2][i]);
            }
        }

        actionData.unit.gameManager.map.ResetMap(true);
        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
        gameManager.map.ResetMap(false, false);
        mapNodes = actionData.unit.gameManager.map.GetNodesInMovementRange(actionData.originalPosition.x, actionData.originalPosition.y, startValue, movingUnit.moveModifier, gameManager, badWalkInPassivesValues);

        if (mapNodes.Count > 1)
        {
            actionData.canMove = true;
            int currentMoveSpeed = movingUnit.currentMoveSpeed;
            for (int i = 1; i < mapNodes.Count; i++)
            {
                DijkstraMapNode currentNode = mapNodes[i];
                int nodeValue = startValue - currentNode.value;
                int amountOfMoveActionsTaken;
                if (currentMoveSpeed > 0)
                {
                    int tempNodeValue = nodeValue - currentMoveSpeed;
                    if (tempNodeValue <= 0)
                    {
                        amountOfMoveActionsTaken = - 1;
                    }
                    else
                    {
                        tempNodeValue -= 1;
                        amountOfMoveActionsTaken = tempNodeValue / (movingUnit.moveSpeedPerMoveAction);
                    }
                }
                else
                {
                    nodeValue -= 1;
                    amountOfMoveActionsTaken = (nodeValue / (movingUnit.moveSpeedPerMoveAction));
                }
                amountOfMoveActionsTaken += 1;
                int actionPointsUsed = 0;
                for (int j = 0; j < amountOfMoveActionsTaken; j++)
                {
                    actionPointsUsed += j + 1;
                }

                if (actionData.ignorePassiveArea[currentNode.x, currentNode.y] || actionPointsUsed < actionData.movementData[currentNode.x, currentNode.y])
                {
                    actionData.movementData[currentNode.x, currentNode.y] = actionPointsUsed;
                    actionData.movementActions[currentNode.x, currentNode.y] = new List<Action> { this };
                    actionData.startPositions[currentNode.x, currentNode.y] = new List<Vector2Int>() { new Vector2Int(movingUnit.x, movingUnit.y) };
                    actionData.ignorePassiveArea[currentNode.x, currentNode.y] = false;
                }
            }
        }
    }

    public override void SelectAction(Unit self)
    {
        /*
        if (SpecificCheckActionUsable(self) && (this.intialActionPointUsage + actionPointGrowth * self.actions[actionIndex].amountUsedDuringRound
    <= self.currentActionsPoints || self.CanMove()))
        {
            return;
        }
        */
        base.SelectAction(self);
        self.gameManager.spriteManager.ActivateMovementTargeting(self, false, self.currentMajorActionsPoints);
        self.gameManager.spriteManager.movementTargeting.OnFoundTarget += FoundTarget;
    }

    public void FoundTarget(List<Vector2Int> path, List<int> indexOfStartingMoves,  Unit movingUnit, bool foundTarget)
    {
        if (foundTarget)
        {
            if (movingUnit.gameManager.spriteManager.GetWorldPosition(path[0].x, path[0].y) == movingUnit.transform.position)
            {
                path.RemoveAt(0);
            }
            if (path.Count == 0)
            {
                movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
                movingUnit.gameManager.spriteManager.ActivateMovementTargeting(movingUnit, false, movingUnit.currentMajorActionsPoints);
                movingUnit.gameManager.spriteManager.movementTargeting.OnFoundTarget += FoundTarget;
                return;
            }

            ActionData actionData = new ActionData();
            actionData.action = this;
            actionData.actingUnit = movingUnit;
            actionData.originLocation = new Vector2Int(movingUnit.x, movingUnit.y);

            List<Vector2Int> tempPath = new List<Vector2Int>() { path[0] };
            actionData.path = tempPath;
            movingUnit.gameManager.AddActionToQueue(actionData, false, false);


            for (int i = 1; i < path.Count; i++)
            {
                actionData= new ActionData();
                actionData.action = this;
                actionData.actingUnit = movingUnit;
                actionData.originLocation = new Vector2Int(path[i - 1].x, path[i - 1].y);

                tempPath =  new List<Vector2Int>() { path[i] };
                actionData.path = tempPath;
                movingUnit.gameManager.AddActionToQueue(actionData, false, false);
            }
            movingUnit.gameManager.PlayActions();   
        }
        else
        {
            movingUnit.UseActionPoints(0);
            movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
        }
    }

    public void AnotherActionMove(List<Vector2Int> path, Unit movingUnit, bool onlyMoved)
    {
        Debug.Log("Another Action MOve");

        if (movingUnit.gameManager.spriteManager.GetWorldPosition(path[0].x, path[0].y) == movingUnit.transform.position)
        {
            path.RemoveAt(0);
        }
        if (path.Count == 0)
        {
            return;
        }

        ActionData actionData = new ActionData();
        actionData.action = this;
        actionData.actingUnit = movingUnit;
        actionData.originLocation = new Vector2Int(movingUnit.x, movingUnit.y);

        List<Vector2Int> tempPath = new List<Vector2Int>() { path[0] };
        actionData.path = tempPath;
        movingUnit.gameManager.AddActionToQueue(actionData, false, false);

        for (int i = 1; i < path.Count; i++)
        {
            actionData = new ActionData();
            actionData.action = this;
            actionData.actingUnit = movingUnit;
            actionData.originLocation = new Vector2Int(path[i - 1].x, path[i - 1].y);

            tempPath = new List<Vector2Int>() { path[i] };
            actionData.path = tempPath;
            movingUnit.gameManager.AddActionToQueue(actionData, false, false);
        }
        movingUnit.gameManager.PlayActions();
    }

    public override void ConfirmAction(ActionData actionData)
    { 
        Unit movingUnit = actionData.actingUnit;
        Vector2Int nextHexPosition = actionData.path[0];
        Vector3 newPosition = movingUnit.gameManager.spriteManager.GetWorldPosition(nextHexPosition.x, nextHexPosition.y);
        if (movingUnit.currentMoveSpeed < movingUnit.gameManager.moveCostMap[nextHexPosition.x, nextHexPosition.y])
        {
            int actionPointsUsed = this.actionPointUsage;
            movingUnit.actions[GetActionIndex(movingUnit)].actionUsedDuringRound = true;
            movingUnit.UseActionPoints(actionPointsUsed);
            movingUnit.currentMoveSpeed += movingUnit.moveSpeedPerMoveAction;
        }
        movingUnit.currentMoveSpeed -= movingUnit.gameManager.moveCostMap[nextHexPosition.x, nextHexPosition.y];
        MoveAnimation moveAnimation = (MoveAnimation)Instantiate(animation);
        moveAnimation.SetParameters(movingUnit, movingUnit.transform.position, newPosition, 
            new Vector2Int(nextHexPosition.x, nextHexPosition.y));
        movingUnit.MovePositions(movingUnit.transform.position, newPosition, true);

        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
    }
}
