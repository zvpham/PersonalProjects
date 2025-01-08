using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Evade")]
public class Evade : StatusAction
{
    public override int CalculateWeight(AIActionData AIActionData)
    {
        throw new System.NotImplementedException();
    }

    public override void FindOptimalPosition(AIActionData AIActionData)
    {
        return;
    }

    public override bool CheckIfActionIsInRange(AIActionData AIActionData)
    {
        return false;
    }

    public override void AIUseAction(AIActionData AIActionData)
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
                if (actionPointsUsed < actionData.movementData[currentNode.x, currentNode.y])
                {
                    Vector2Int startHex = new Vector2Int(movingUnit.x, movingUnit.y);
                    actionData.movementData[currentNode.x, currentNode.y] = actionPointsUsed;
                    actionData.movementActions[currentNode.x, currentNode.y] = new List<Action>() { this, movingUnit.actions[0].action};
                    actionData.startPositions[currentNode.x, currentNode.y] = new List<Vector2Int>() { new Vector2Int(movingUnit.x, movingUnit.y), new Vector2Int(movingUnit.x, movingUnit.y) };
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
