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

        int actionIndex = GetActionIndex(movingUnit);
        int amountMoved = movingUnit.actions[actionIndex].amountUsedDuringRound;

        int moveAmounts = 0;
        int usableActionPoints = movingUnit.currentActionsPoints;
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

        int startValue = movingUnit.currentMoveSpeed + (movingUnit.moveSpeedPerMoveAction * moveAmounts);
        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
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

    public override void AIUseAction(AIActionData AIActionData, bool finalAction = false)
    {
        Vector2Int endPosition = AIActionData.desiredEndPosition;
        Debug.Log("EndPosition: " + endPosition + " , " + AIActionData.movementActions[endPosition.x, endPosition.y].Count);
        int movementActionIndex = AIActionData.movementActions[endPosition.x, endPosition.y].IndexOf(this);
        CombatGameManager gameManager = AIActionData.unit.gameManager;
        Unit movingUnit = AIActionData.unit;
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
        AIActionData.unit.gameManager.map.SetGoalsNew(new List<Vector2Int>() { currentEndPosition }, gameManager, movingUnit.moveModifier, badWalkInPassivesValues);
        DijkstraMap map = gameManager.map;
        int x = startingPosition.x;
        int y = startingPosition.y;
        int previousNodeMoveValue = map.getGrid().GetGridObject(x, y).value;

        int initialActionPoints = AIActionData.expectedCurrentActionPoints;
        int amountOfPossibleMoves = 0;
        int actionIndex = GetActionIndex(movingUnit);
        int amountMoved =  movingUnit.actions[actionIndex].amountUsedDuringRound;
        while (initialActionPoints > 0)
        {
            if (initialActionPoints >= amountMoved + amountOfPossibleMoves + 1)
            {
                amountOfPossibleMoves += 1;
                initialActionPoints -= amountMoved + amountOfPossibleMoves;
            }
            else
            {
                break;
            }
        }

        int currentMoveSpeed = AIActionData.expectedInitialMoveSpeed + movingUnit.moveSpeedPerMoveAction * amountOfPossibleMoves;
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


    public override int[,] GetMovementMap(AIActionData actionData)
    {
        CombatGameManager gameManager = actionData.unit.gameManager;
        Unit movingUnit = actionData.unit;

        int amountMoved = -1;
        for (int i = 0; i < movingUnit.actions.Count; i++)
        {
            if (movingUnit.actions[i].action.GetType() == typeof(Move))
            {
                amountMoved = movingUnit.actions[i].amountUsedDuringRound;
            }
        }

        int initialActionPoints = actionData.expectedCurrentActionPoints;
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
        List<DijkstraMapNode> mapNodes = actionData.unit.gameManager.map.GetNodesInMovementRange(actionData.originalPosition.x, actionData.originalPosition.y, startValue, movingUnit.moveModifier, gameManager);
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

                if (actionData.ignorePassiveArea[currentNode.x, currentNode.y] &&  actionPointsUsed < actionData.movementData[currentNode.x, currentNode.y])
                {
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
        mapNodes = actionData.unit.gameManager.map.GetNodesInMovementRange(actionData.originalPosition.x, actionData.originalPosition.y, startValue, movingUnit.moveModifier, gameManager, badWalkInPassivesValues);
        movementGridValues = actionData.unit.gameManager.map.GetGridValues();

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

                if (actionData.ignorePassiveArea[currentNode.x, currentNode.y] || actionPointsUsed < actionData.movementData[currentNode.x, currentNode.y])
                {
                    actionData.movementData[currentNode.x, currentNode.y] = actionPointsUsed;
                    actionData.movementActions[currentNode.x, currentNode.y] = new List<Action> { this };
                    actionData.startPositions[currentNode.x, currentNode.y] = new List<Vector2Int>() { new Vector2Int(movingUnit.x, movingUnit.y) };
                    actionData.ignorePassiveArea[currentNode.x, currentNode.y] = false;
                }
            }
        }
        return movementGridValues;
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
        self.gameManager.spriteManager.ActivateMovementTargeting(self, false, self.currentActionsPoints);
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
                movingUnit.gameManager.spriteManager.ActivateMovementTargeting(movingUnit, false, movingUnit.currentActionsPoints);
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

    public void AnotherActionMove(List<Vector2Int> path, List<int> indexOfStartingMoves, Unit movingUnit, bool onlyMoved)
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
            int actionPointsUsed = this.intialActionPointUsage + this.actionPointGrowth * movingUnit.actions[0].amountUsedDuringRound;
            movingUnit.actions[0].amountUsedDuringRound += 1;
            movingUnit.UseActionPoints(actionPointsUsed);
            movingUnit.currentMoveSpeed += movingUnit.moveSpeedPerMoveAction;
        }
        movingUnit.currentMoveSpeed -= movingUnit.gameManager.moveCostMap[nextHexPosition.x, nextHexPosition.y];
        MoveAnimation moveAnimation = (MoveAnimation)Instantiate(animation);
        moveAnimation.SetParameters(movingUnit.gameManager, movingUnit.transform.position, newPosition, 
            new Vector2Int(nextHexPosition.x, nextHexPosition.y));
        movingUnit.MovePositions(movingUnit.transform.position, newPosition, true);

        movingUnit.gameManager.spriteManager.DeactiveTargetingSystem();
    }
}
