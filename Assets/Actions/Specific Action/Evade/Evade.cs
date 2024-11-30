using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Action/Evade")]
public class Evade : StatusAction
{
    public override int CalculateWeight(AIActionData actionData)
    {
        throw new System.NotImplementedException();
    }

    public override void FindOptimalPosition(AIActionData actionData)
    {
        return;
    }

    public override bool CheckIfActionIsInRange(AIActionData actionData)
    {
        return false;
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
        actionData.unit.gameManager.map.ResetMap(true);
        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
        actionData.unit.gameManager.map.SetGoals(new List<Vector2Int>() { actionData.originalPosition }, gameManager, movingUnit.moveModifier, badWalkInPassivesValues);
        int[,] movementGridValues = actionData.unit.gameManager.map.GetGridValues();

        int moveInitialActionAmount = -1;
        int moveActionGrowth = -1;
        int amountMoved = 0;
        bool usedEvade = false;
        for (int i = 0; i < movingUnit.actions.Count; i++)
        {
            if (movingUnit.actions[i].action.GetType() == typeof(Move))
            {
                moveInitialActionAmount = movingUnit.actions[i].action.intialActionPointUsage;
                moveActionGrowth = movingUnit.actions[i].action.actionPointGrowth;
                amountMoved = movingUnit.actions[i].amountUsedDuringRound;
            }

            if (movingUnit.actions[i].action.GetType() == typeof(Evade))
            {
                usedEvade = movingUnit.actions[i].amountUsedDuringRound > 0;
            }
        }

        int actionAddAmount = this.intialActionPointUsage;
        if (usedEvade)
        {
            actionAddAmount = 0;
        }

        for (int i = 0; i < movementGridValues.GetLength(0); i++)
        {
            for (int j = 0; j < movementGridValues.GetLength(1); j++)
            {
                int actionAmount = (movementGridValues[i, j] + movingUnit.moveSpeed - 1) / movingUnit.moveSpeed;
                int totalActionAmount = 0;
                for (int k = 0; k < actionAmount; k++)
                {
                    totalActionAmount += moveInitialActionAmount + (k + amountMoved) * moveActionGrowth;
                }
                movementGridValues[i, j] = totalActionAmount + actionAddAmount;
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
