using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIRangedMovement
{
    // This is for when there are spotted Enemy Units
    public static void RangedMoveToOptimalPositionCombat(AIActionData AiActionData)
    {
        Unit currentUnit = AiActionData.unit;
        CombatGameManager gameManager = currentUnit.gameManager;
        int highestActionWeight = -1;
        int actionIndex = -1;
        for (int i = 0; i < currentUnit.actions.Count; i++)
        {
            int testActionWeight = currentUnit.actions[i].action.CalculateEnvironmentWeight(AiActionData);
            if (currentUnit.actions[i].action.actionTypes.Contains(ActionType.Movement))
            {
                continue;
            }

            if (testActionWeight > highestActionWeight)
            {
                actionIndex = i;
                highestActionWeight = testActionWeight;
            }
        }

        Debug.LogWarning("Remove This for final version, remove when you have enemyTeamStartingPositinons Set");
        Vector2Int endHex;
        if (AiActionData.enemyTeamStartingPositions == null || AiActionData.enemyTeamStartingPositions.Count == 0)
        {
            if (AiActionData.enemyUnits.Count == 0)
            {
                Debug.LogError("no EnemynUnits Left");
                return;
            }
            endHex = AiActionData.enemyUnits[0];
        }
        else
        {
            endHex = AiActionData.enemyTeamStartingPositions[0];
        }   

        AiActionData.desiredEndPosition = new Vector2Int(-1, -1);
        Action currentAction = currentUnit.actions[actionIndex].action;
        Debug.Log("Current Action: " + currentAction);
        currentAction.FindOptimalPosition(AiActionData);
        if (AiActionData.desiredEndPosition == new Vector2Int(-1, -1))
        {
            AiActionData.unit.EndTurnAction();
        }
        else
        {
            int moveActionIndex = AiActionData.unit.GetMoveActionIndex();
            Move moveAction = (Move)AiActionData.unit.actions[moveActionIndex].action;
            Debug.Log("final Position: " + AiActionData.desiredEndPosition);
            moveAction.MoveIsOnlyActionAIAction(AiActionData);
        }
    }

    public static void RangedMoveToOptimalPositionSkirmish(AIActionData AiActionData)
    {
        //Find Highest Action weight
        Unit movingUnit = AiActionData.unit;
        CombatGameManager gameManager = movingUnit.gameManager;
        int highestActionWeight = -1;
        int actionIndex = -1;
        for (int i = 0; i < movingUnit.actions.Count; i++)
        {
            int testActionWeight = movingUnit.actions[i].action.CalculateEnvironmentWeight(AiActionData);
            if (movingUnit.actions[i].action.actionTypes.Contains(ActionType.Movement))
            {
                continue;
            }

            if (testActionWeight > highestActionWeight)
            {
                actionIndex = i;
                highestActionWeight = testActionWeight;
            }
        }

        Debug.LogWarning("Remove This for final version, remove when you have enemyTeamStartingPositinons Set");
        Vector2Int endHex;
        if (AiActionData.enemyTeamStartingPositions == null || AiActionData.enemyTeamStartingPositions.Count == 0)
        {
            if (AiActionData.enemyUnits.Count == 0)
            {
                Debug.LogError("no EnemynUnits Left");
                return;
            }
            endHex = AiActionData.enemyUnits[0];
        }
        else
        {
            endHex = AiActionData.enemyTeamStartingPositions[0];
        }

        // Find Desired End Position Based On Highest Action
        AiActionData.desiredEndPosition = new Vector2Int(-1, -1);
        Action currentAction = movingUnit.actions[actionIndex].action;
        //Debug.Log("Current Action: " + currentAction);
        currentAction.FindOptimalPosition(AiActionData);
        //IF Can't Find Position End Turn
        if (AiActionData.desiredEndPosition == new Vector2Int(-1, -1))
        {
            Debug.LogWarning("Couldn't Find Position");
            AiActionData.unit.EndTurnAction();
        }
        // Otherwise do 
        else
        {
            //Debug.Log("Found End Position: " + AiActionData.desiredEndPosition + ", " + AiActionData.highestActionWeight);
            //Get highest MoveSpeed From One Action
            DijkstraMap map = gameManager.map;
            int currMaxSpeed = movingUnit.currentMoveSpeed;
            if (movingUnit.currentMajorActionsPoints > 0)
            {
                currMaxSpeed += movingUnit.moveSpeedPerMoveAction;
            }

            // Get All Nodes in one Move Range From Walking
            Vector2Int unitPosition =  new Vector2Int(movingUnit.x, movingUnit.y);
            gameManager.map.ResetMap(true, false);
            movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
            List<DijkstraMapNode> nodesInMoverange = gameManager.map.GetNodesInMovementRange(movingUnit.x, movingUnit.y, currMaxSpeed, movingUnit.moveModifier, gameManager);
            //Debug.Log("Nodes In Range: " + nodesInMoverange.Count);

            // Get grid of move Values 0 at moving Unit Position and INcrease by Move Grid (This is To Ensure that End Move position is Walkable In One Move Action)
            map.ResetMap(false);
            map.SetGoalsNew(new List<Vector2Int>() { unitPosition }, gameManager, AiActionData.unit.moveModifier);
            int[,] gridOfMoveableHexes = map.GetGridValues();
            movingUnit.unitMovedThisTurn = true;

            //Get grid of move Values 0 at moving Desired End Position and INcrease by Move Grid
            gameManager.map.ResetMap(true);
            movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
            gameManager.map.SetGoalsNew(new List<Vector2Int>() { AiActionData.desiredEndPosition }, gameManager, movingUnit.moveModifier);

            //Find Best Move Position in One or less action Move
            bool foundPosition = false;
            int highestValue = int.MinValue;
            int originalUnitNodeValue = gameManager.map.getGrid().GetGridObject(new Vector2Int(movingUnit.x, movingUnit.y)).value;
            DijkstraMapNode targetDestination = null;
            GridHex<GridPosition> unitGrid = gameManager.grid;
            //Debug.Log("Current Max Move Speed: " + currMaxSpeed);
            for (int i = 0; i < nodesInMoverange.Count; i++)
            {
                DijkstraMapNode currentNode = nodesInMoverange[i];
                int modifiedValue = originalUnitNodeValue - currentNode.value - currMaxSpeed;

                //Debug.Log("test 10: " + currentNode.x + ", " + currentNode.y + ", " + gridOfMoveableHexes[currentNode.x, currentNode.y]);
                if (modifiedValue > highestValue && (unitGrid.GetGridObject(currentNode.x, currentNode.y).unit == null ||
                    unitGrid.GetGridObject(currentNode.x, currentNode.y).unit == movingUnit) && gridOfMoveableHexes[currentNode.x, currentNode.y] <= currMaxSpeed)
                {
                    targetDestination = currentNode;
                    highestValue = modifiedValue;
                }
            }

            //if Target Position is Found Move To End Position
            if (targetDestination != null)
            {
                AiActionData.desiredEndPosition = new Vector2Int(targetDestination.x, targetDestination.y);
                List<Action> movementActions = AiActionData.movementActions[targetDestination.x, targetDestination.y];
                //Debug.Log("targetDestination: " + targetDestination.x + ", " + targetDestination.y + ", " + movementActions.Count);
                for (int i = 0; i < movementActions.Count; i++)
                {
                    movementActions[i].AIUseAction(AiActionData);
                }
                gameManager.PlayActions();
                foundPosition = true;
            }

            //If targetposition isn't found  end Trn
            if (!foundPosition)
            {
                movingUnit.EndTurnAction();
            }
        }
    }
}
