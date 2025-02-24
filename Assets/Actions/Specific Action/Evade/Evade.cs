using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Evade")]
public class Evade : StatusAction
{
    public override int CalculateWeight(AIActionData AiActionData)
    {
        return -2;
    }

    public override void FindOptimalPosition(AIActionData AiActionData)
    {
        return;
    }

    public override bool CheckIfActionIsInRange(AIActionData AiActionData)
    {
        return false;
    }

    public override bool CanMove(AIActionData AiActionData)
    {
        Unit movingUnit = AiActionData.unit;
        int x = movingUnit.x;
        int y = movingUnit.y;
        Debug.Log("Start Position: " + x + ", " + y);   
        int actionIndex = GetActionIndex(movingUnit);
        if (movingUnit.currentActionsPoints <= this.intialActionPointUsage || movingUnit.actions[actionIndex].amountUsedDuringRound > 0)
        {
            return false;
        }

        int moveActionIndex = -1;
        for (int i = 0; i < movingUnit.actions.Count; i++)
        {
            if (movingUnit.actions[i].action.GetType() == typeof(Move))
            {
                moveActionIndex = i;
                break;
            }
        }
        int amountMoved = movingUnit.actions[moveActionIndex].amountUsedDuringRound;

        int moveAmounts = 0;
        int usableActionPoints = movingUnit.currentActionsPoints - intialActionPointUsage;
        while (usableActionPoints > 0)
        {
            if (usableActionPoints >= amountMoved + moveAmounts + 1)
            {
                moveAmounts += 1;
                usableActionPoints -= amountMoved + moveAmounts;
            }
            else
            {
                break;
            }
        }
        CombatGameManager gameManager = movingUnit.gameManager;
        List<PassiveEffectArea>[,] passives = new List<PassiveEffectArea>[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < gameManager.mapSize; i++)
        {
            for (int j = 0; j < gameManager.mapSize; j++)
            {
                passives[i, j] = new List<PassiveEffectArea>();
            }
        }

        List<List<PassiveEffectArea>> classifiedPassiveEffectArea = movingUnit.CalculuatePassiveAreas(status);
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

        int startValue = movingUnit.currentMoveSpeed + (movingUnit.moveSpeedPerMoveAction * moveAmounts);
        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
        List<DijkstraMapNode> nodesInMovementRange = gameManager.map.GetNodesInMovementRange(x, y, startValue, movingUnit.moveModifier, gameManager, badWalkInPassivesValues);
        GridHex<GridPosition> grid = gameManager.grid;
        List<DijkstraMapNode> emptyNodes = new List<DijkstraMapNode>();
        for (int i = 0; i < nodesInMovementRange.Count; i++)
        {
            x = nodesInMovementRange[i].x;
            y = nodesInMovementRange[i].y;

            Debug.Log( grid.GetGridObject(x, y));
            if (grid.GetGridObject(x, y).unit == null)
            {
                emptyNodes.Add(nodesInMovementRange[i]);
            }
        }
        return emptyNodes.Count > 0;    
    }

    public override Tuple<int, Vector2Int, List<Action>, List<Vector2Int>> MoveUnit(AIActionData AiActionData, bool IgnorePassives = false)
    {
        Unit movingUnit = AiActionData.unit;
        int actionIndex = GetActionIndex(movingUnit);
        if (movingUnit.currentActionsPoints <= this.intialActionPointUsage || movingUnit.actions[actionIndex].amountUsedDuringRound > 0)
        {
            return new Tuple<int, Vector2Int, List<Action>, List<Vector2Int>>(int.MaxValue, Vector2Int.zero, null, new List<Vector2Int>());
        }
        CombatGameManager gameManager = movingUnit.gameManager;
        DijkstraMap map =gameManager.map;
        List<PassiveEffectArea>[,] passives = new List<PassiveEffectArea>[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < gameManager.mapSize; i++)
        {
            for (int j = 0; j < gameManager.mapSize; j++)
            {
                passives[i, j] = new List<PassiveEffectArea>();
            }
        }
        
        List<List<PassiveEffectArea>> classifiedPassiveEffectArea = movingUnit.CalculuatePassiveAreas(status);
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

        movingUnit.moveModifier.SetUnwalkable(gameManager, AiActionData.unit);

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

        map.SetGoalsNew(goals, gameManager, AiActionData.unit.moveModifier, badWalkInPassivesValues);

        int x = movingUnit.x;
        int y = movingUnit.y;
        List<Vector2Int> path = new List<Vector2Int>();
        path.Clear();

        int moveActionIndex = -1;
        for (int i = 0; i < movingUnit.actions.Count; i++)
        {
            if (movingUnit.actions[i].action.GetType() == typeof(Move))
            {
                moveActionIndex = i;
                break;
            }
        }

        int initialActionPoints = movingUnit.currentActionsPoints - this.intialActionPointUsage;
        int amountMoved = movingUnit.actions[moveActionIndex].amountUsedDuringRound;
        int moveAmounts = 0;
        while (initialActionPoints > 0)
        {
            if (initialActionPoints >= amountMoved + moveAmounts + 1)
            {
                moveAmounts += 1;
                initialActionPoints -= amountMoved + moveAmounts;
            }
            else
            {
                break;
            }
        }

        int currentMoveSpeed = movingUnit.currentMoveSpeed + (movingUnit.moveSpeedPerMoveAction * moveAmounts);
        int startMoveSpeed = currentMoveSpeed;
        int previousNodeMoveValue = map.getGrid().GetGridObject(x, y).value;
        int moveSpeedUsed = 0;
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
            moveSpeedUsed += moveSpeedDifference;
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
            if (currentNode.value == int.MaxValue)
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
        Debug.Log(movingUnit + ", " + gameManager.grid.GetGridObject(x, y).unit + ", (" + x + ", " + y + ")");
        if (gameManager.grid.GetGridObject(x, y).unit != null)
        {
            Debug.Log("Start Move Speed, " + startMoveSpeed);
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

            Debug.Log("Node Found: " + currentNodeGoal + ", original node: " + movingUnit.x + ", " + movingUnit.y);
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
                    Debug.LogWarning("Break 1: " + x + ", " + y + ", " + currentNode.value + ", " + currentMoveSpeed + ", " + moveSpeedDifference);
                    break;
                }
                currentMoveSpeed -= moveSpeedDifference;
                previousNodeMoveValue = currentNodeMoveValue;
                moveSpeedUsed += moveSpeedDifference;
                path.Add(new Vector2Int(x, y));

                if (AiActionData.enemyUnits.Contains(path[path.Count - 1]))
                {
                    Debug.LogWarning("Break 2: " + x + ", " + y);
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

        Debug.Log("Path Count: " + path.Count);

        if (path.Count != 0)
        {
            /*
            Move unitmove = null;
            for (int i = 0; i < movingUnit.actions.Count; i++)
            {
                if (movingUnit.actions[i].action.GetType() == typeof(Move))
                {
                    unitmove = (Move)movingUnit.actions[i].action;
                }
            }

            ActionData evadaAction = new ActionData();
            evadaAction.action = this;
            evadaAction.actingUnit = movingUnit;
            gameManager.AddActionToQueue(evadaAction, false, false);

            ActionData actionData = new ActionData();
            actionData.action = unitmove;
            actionData.actingUnit = movingUnit;
            actionData.originLocation = new Vector2Int(movingUnit.x, movingUnit.y);

            List<Vector2Int> tempPath = new List<Vector2Int>() { path[0] };
            actionData.path = tempPath;
            movingUnit.gameManager.AddActionToQueue(actionData, false, false);


            for (int i = 1; i < path.Count; i++)
            {
                actionData = new ActionData();
                actionData.action = unitmove;
                actionData.actingUnit = movingUnit;
                actionData.originLocation = new Vector2Int(path[i - 1].x, path[i - 1].y);

                tempPath = new List<Vector2Int>() { path[i] };
                actionData.path = tempPath;
                movingUnit.gameManager.AddActionToQueue(actionData, false, false);
            }

            movingUnit.gameManager.PlayActions();
            */
            int indexOfMoveAction = movingUnit.GetMoveActionIndex();
            return new Tuple<int, Vector2Int, List<Action>, List<Vector2Int>>(map.getGrid().GetGridObject(path[path.Count - 1]).value, path[path.Count - 1],
                new List<Action> { this, movingUnit.actions[indexOfMoveAction].action }, new List<Vector2Int>() { new Vector2Int(movingUnit.x, movingUnit.y) }) ;
        }
        else
        {
            return new Tuple<int, Vector2Int, List<Action>, List<Vector2Int>>(int.MaxValue, Vector2Int.zero, null, new List<Vector2Int>());
        }
    }

    public override void AIUseAction(AIActionData AIActionData, bool finalAction = false)
    {
        AIActionData.expectedCurrentActionPoints -= intialActionPointUsage;
        Unit self = AIActionData.unit;
        ActionData newData = new ActionData();
        newData.action = this;
        newData.actingUnit = self;
        self.gameManager.AddActionToQueue(newData, false, false);
        self.gameManager.PlayActions();
    }

    public override int[,] GetMovementMap(AIActionData actionData)
    {
        CombatGameManager gameManager = actionData.unit.gameManager;
        Unit movingUnit = actionData.unit;

        List<PassiveEffectArea>[,] passives = new List<PassiveEffectArea>[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < gameManager.mapSize; i++)
        {
            for (int j = 0; j < gameManager.mapSize; j++)
            {
                passives[i, j] = new List<PassiveEffectArea>();
            }
        }

        List<List<PassiveEffectArea>> classifiedPassiveEffectArea = movingUnit.CalculuatePassiveAreas(status);
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

        int amountMoved = -1;
        for (int i = 0; i < movingUnit.actions.Count; i++)
        {
            if (movingUnit.actions[i].action.GetType() == typeof(Move))
            {
                amountMoved = movingUnit.actions[i].amountUsedDuringRound;
            }
        }

        int initialActionPoints = actionData.expectedCurrentActionPoints - intialActionPointUsage;
        int moveAmounts = 0;
        while (initialActionPoints > 0)
        {
            if (initialActionPoints >= amountMoved + moveAmounts + 1)
            {
                moveAmounts += 1;
                initialActionPoints -= amountMoved + moveAmounts;
            }
            else
            {
                break;
            }
        }

        actionData.unit.gameManager.map.ResetMap(true);
        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
        int startValue = (movingUnit.moveSpeedPerMoveAction * moveAmounts);
        List<DijkstraMapNode> mapNodes = actionData.unit.gameManager.map.GetNodesInMovementRange(actionData.originalPosition.x, actionData.originalPosition.y, startValue, movingUnit.moveModifier, gameManager, badWalkInPassivesValues);
        int[,] movementGridValues = actionData.unit.gameManager.map.GetGridValues();

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
                        amountOfMoveActionsTaken = amountMoved - 1;
                    }
                    else
                    {
                        tempNodeValue -= 1;
                        amountOfMoveActionsTaken = tempNodeValue / (movingUnit.moveSpeedPerMoveAction) + amountMoved;
                    }
                }
                else
                {
                    nodeValue -= 1;
                    amountOfMoveActionsTaken = (nodeValue / (movingUnit.moveSpeedPerMoveAction)) + amountMoved;
                }
                amountOfMoveActionsTaken += 1;
                int actionPointsUsed = 0;
                for (int j = 0; j < amountOfMoveActionsTaken; j++)
                {
                    actionPointsUsed += j + 1;
                }

                actionPointsUsed += intialActionPointUsage;
                if (actionData.ignorePassiveArea[currentNode.x, currentNode.y] ||  actionPointsUsed < actionData.movementData[currentNode.x, currentNode.y])
                {
                    Vector2Int startHex = new Vector2Int(movingUnit.x, movingUnit.y);
                    actionData.movementData[currentNode.x, currentNode.y] = actionPointsUsed;
                    actionData.movementActions[currentNode.x, currentNode.y] = new List<Action>() { this, movingUnit.actions[0].action};
                    actionData.startPositions[currentNode.x, currentNode.y] = new List<Vector2Int>() { new Vector2Int(movingUnit.x, movingUnit.y), new Vector2Int(movingUnit.x, movingUnit.y) };
                    actionData.ignorePassiveArea[currentNode.x, currentNode.y] = false;
                }
            }
        }

        return movementGridValues;
    }


    public override void SelectAction(Unit self)
    {
        base.SelectAction(self);
        self.gameManager.spriteManager.ActivateActionConfirmationMenu(
            () =>
            {
                ActionData newData = new ActionData();
                newData.action = this;
                newData.actingUnit = self;
                self.gameManager.AddActionToQueue(newData, false, false);
                self.gameManager.PlayActions();
            },
            () =>
            {

            });
    }

    public override void ConfirmAction(ActionData actionData)
    {
        Debug.Log("Confirm Action");
        Evade action = (Evade) actionData.action;
        action.status.AddStatus(actionData.actingUnit, 1);
        UseActionPreset(actionData.actingUnit);
    }

}
