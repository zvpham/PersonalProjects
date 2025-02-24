using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class AITurn : MonoBehaviour
{
    public static List<UnitType> meleeTypes = new List<UnitType>() { UnitType.Chaff, UnitType.FrontLine, UnitType.FrontLine2, UnitType.MeleeSupport };


    public AITurnStates AIState;
    public List<UnitSuperClass> unitSuperClasses;
    public List<Unit> units; // ALL Units in AITURN sorted by Initiative
    public List<Vector2Int> futureUnitPosiitions;
    public List<Vector2Int> enemyTeamStartPositions;
    public List<Vector2Int> lastSeenEnemyUnitPositions;

    public List<Unit> frontlineUnits;
    public List<Unit> backlineUnits;


    // Enemy Units Visible to AiTeam
    public List<Unit> visibleUnits = new List<Unit>();

    public int totalRangedValue;
    public int numMelee;
    public CombatGameManager gameManager;
    //For randomization
    private int seed;


    public List<List<UnitSuperClass>> LoadEnemyPositions (MissionUnitPlacementName unitFormation, List<UnitSuperClass> units)
    {
        switch(unitFormation)
        {
            case (MissionUnitPlacementName.LineBattleWestStart):
                return CreateLineFormation(units);
        }
        Debug.LogError("Sorting AIUnits into battle positions failed: " + gameObject.name);
        return null;
    }

    private int UseSeed()
    {
        seed += 1;
        return seed;
    }

    public List<List<UnitSuperClass>> CreateLineFormation(List<UnitSuperClass> units)
    {
        List<UnitSuperClass> frontLine = new List<UnitSuperClass>();
        List<UnitSuperClass> backLine =  new List<UnitSuperClass>();
        List<UnitSuperClass> flankers = new List<UnitSuperClass>();
        List<UnitSuperClass> frontLine2 = new List<UnitSuperClass>();
        List<UnitSuperClass> meleeSupports = new List<UnitSuperClass>();
        // Add Frontline to Frontline and (Ranged and RangedSupport) to backline
        for(int i = 0; i < units.Count; i++)
        {
            switch(units[i].unitType)
            {
                case (UnitType.FrontLine):
                    frontLine.Add(units[i]);
                    break;
                case (UnitType.Ranged):
                    backLine.Add(units[i]);
                    break;
                case (UnitType.RangedSupport):
                    backLine.Add(units[i]);
                    break;
                case (UnitType.Flanker):
                    flankers.Add(units[i]);
                    break;
                case (UnitType.FrontLine2):
                    frontLine2.Add(units[i]);
                    break;
                case (UnitType.MeleeSupport):
                    meleeSupports.Add(units[i]);
                    break;
            }
        }

        seed = System.DateTime.Now.Millisecond;
        Random.InitState(UseSeed());
        //Insert MeleeSupports into frontline or backline depending on if backline has too many more units than frontline
        for (int i = 0; i < meleeSupports.Count; i++)
        {
            if (backLine.Count > frontLine.Count + flankers.Count + frontLine2.Count + 2)
            {
                int positionIndex = Random.Range(1, frontLine.Count);
                frontLine.Insert(positionIndex, meleeSupports[i]);
            }
            else
            {
                int positionIndex = Random.Range(1, backLine.Count);
                backLine.Insert(positionIndex, meleeSupports[i]);
            }
        }

        //Insert Frontline2 into frontline or backline depending on if backline has too many more units than frontline
        for (int i = 0; i < frontLine2.Count; i++)
        {
            if (backLine.Count > frontLine.Count + flankers.Count)
            {
                int positionIndex = Random.Range(1, frontLine.Count);
                frontLine.Insert(positionIndex, frontLine2[i]);
            }
            else
            {
                int positionIndex = Random.Range(1, backLine.Count);
                backLine.Insert(positionIndex, frontLine2[i]);
            }
        }

        int flankerIndex = 0;
        for(int i = 0; i < flankers.Count; i++)
        {
            if(flankerIndex % 2 == 0)
            {
                frontLine.Insert(0, flankers[i]);
            }
            else
            {
                frontLine.Add(flankers[i]);
            }
        }
        List<List<UnitSuperClass>> battleLines =  new List<List<UnitSuperClass>>();
        battleLines.Add(frontLine); 
        battleLines.Add(backLine);
        return battleLines;
    }

    public int CalculateRangedValue()
    {
        totalRangedValue = 0;
        for (int i = 0; i < unitSuperClasses.Count; i++)
        {
            if (unitSuperClasses[i].unitType == UnitType.Ranged)
            {
                totalRangedValue += 2 + unitSuperClasses[i].powerLevel;
            }
        }
        return totalRangedValue;
    }

    public int CalculateMeleeValue()
    {
        numMelee = 0;
        List<UnitType> meleeTypes = new List<UnitType>() { UnitType.Chaff, UnitType.Flanker, UnitType.MeleeSupport,
        UnitType.FrontLine, UnitType.FrontLine};
        for(int i = 0; i <= unitSuperClasses.Count;i++)
        {
            if (meleeTypes.Contains(unitSuperClasses[i].unitType))
            {
                if(unitSuperClasses[i].GetType() == typeof(UnitGroup))
                {
                    UnitGroup tempGroup = (UnitGroup) unitSuperClasses[i];
                    numMelee += tempGroup.units.Count;
                }
                else
                {
                    numMelee++;
                }  
            }
        }
        return numMelee;
    }

    public void ReadyCombat()
    {
        totalRangedValue = CalculateRangedValue();
    }

    public void SortUnitsByInitiative()
    {
        List<IInititiave> initiatives =  new List<IInititiave>();
        List<int> initiativeAmount =  new List<int>();
        for (int i = 0; i < unitSuperClasses.Count; i++)
        {
            if (unitSuperClasses[i].GetType() == typeof(UnitGroup))
            {
                UnitGroup tempGroup = (UnitGroup)unitSuperClasses[i];
                initiatives.Add(tempGroup);
                initiativeAmount.Add(tempGroup.CalculateInititive());
            }
            else
            {
                Unit tempUnit = (Unit)unitSuperClasses[i];
                initiatives.Add(tempUnit);
                initiativeAmount.Add(tempUnit.CalculateInititive());
            }
        }
        initiatives[0].Quicksort(initiativeAmount, initiatives, 0, initiatives.Count - 1);

        for (int i = 0; i < initiatives.Count; i++)
        {
            if (initiatives[i].GetType() == typeof(UnitGroup))
            {
                UnitGroup tempGroup = (UnitGroup)unitSuperClasses[i];
                for(int j = 0; j < tempGroup.units.Count; j++)
                {
                    units.Add(tempGroup.units[j]);
                }
            }
            else
            {
                Unit tempUnit = (Unit)unitSuperClasses[i];
                units.Add(tempUnit);
            }
        }
    }

    public void SortUnitsByType()
    {
        for(int i = 0; i < unitSuperClasses.Count; i++)
        {
            if (meleeTypes.Contains(unitSuperClasses[i].unitType))
            {
                if (unitSuperClasses[i].GetType() == typeof(UnitGroup))
                {
                    UnitGroup tempGroup = (UnitGroup)unitSuperClasses[i];
                    for (int j = 0; j < tempGroup.units.Count; j++)
                    {
                        frontlineUnits.Add(tempGroup.units[j]);
                    }
                }
                else
                {
                    Unit tempUnit = (Unit)unitSuperClasses[i];
                    frontlineUnits.Add(tempUnit);
                }
            }
        }
    }

    public void StartAITurn(List<Unit> unitGroup)
    {
        Debug.LogWarning("Remove This (Manually sets AIState to Combat)");
        AIState = AITurnStates.Combat;
        visibleUnits = gameManager.playerTurn.playerUnits;
        CalculateEnemyState();
        for(int i = 0; i < unitGroup.Count; i++)
        {
            
        }

        for(int i = 0; i < unitGroup.Count; i++)
        {
            switch(unitGroup[i].unitType)
            {
                case (UnitType.Chaff):
                    ChaffAI(unitGroup[i], false);
                    break;
                case (UnitType.Flanker):
                    ChaffAI(unitGroup[i], false);
                    break;
                case (UnitType.FrontLine):
                    FrontLineAI(unitGroup[i], false);
                    break;
                case (UnitType.FrontLine2):
                    ChaffAI(unitGroup[i], false);
                    break;
                case (UnitType.MeleeSupport):
                    ChaffAI(unitGroup[i], false);
                    break;
                case (UnitType.RangedSupport):
                    ChaffAI(unitGroup[i], false);
                    break;
                case (UnitType.Ranged):
                    ChaffAI(unitGroup[i], false);
                    break;
                case (UnitType.Mixed):
                    ChaffAI(unitGroup[i], false);
                    break;
            }
        }
    }

    public void CalculateFutureUnitPositions(Unit unit)
    {
        int unitIndex =  units.IndexOf(unit);
        for(int i = unitIndex; i < units.Count; i++)
        {
            Unit currentUnit = units[i];
            switch(currentUnit.unitType)
            {
                case UnitType.Chaff:
                    ChaffAI(currentUnit, true);
                    break;
                case UnitType.FrontLine:
                    FrontLineAI(currentUnit, true);
                    break;
                case UnitType.FrontLine2:
                    FrontLineAI(currentUnit, true);
                    break;
                case UnitType.Flanker:
                    break;
                case UnitType.Mixed:
                    break;
                case UnitType.MeleeSupport:
                    break;
                case UnitType.RangedSupport:
                    break;
                case UnitType.Ranged:
                    break;
            }
        }
    }

    public AITurnStates CalculateEnemyState()
    {
        if(AIState == AITurnStates.Combat)
        {
            return AIState;
        }
        else if(totalRangedValue  - gameManager.playerTurn.totalRangedValue > 5)
        {
            AIState = AITurnStates.SuperiorRanged;
        }
        else if (gameManager.playerTurn.totalRangedValue - totalRangedValue > 5)
        {
            AIState = AITurnStates.Agressive;
        }
        else
        {
            AIState = AITurnStates.Skirmish;
        }  

        return AIState;
    }
    
    // -------------------------------
    // General Unit Movement
    // ----------------------------

    //Tuple<int, int> <action index, action Priority>
    private static int MovementActionCompareByPriority(Tuple<int, int> actionOne, Tuple<int, int> actionTwo)
    {
        return actionOne.Item2.CompareTo(actionTwo.Item2);
    }

    // This is for when there are spotted Enemy Units
    public void MeleeRangeOneGetCloserToEnemyUnits(AIActionData AiActionData)
    {
        Unit movingUnit = AiActionData.unit;
        List<Tuple<int>> moveActionIndexes = new List<Tuple<int>>();
        for(int i = 0; i < movingUnit.actions.Count; i++)
        {
            if (movingUnit.actions[i].action.actionTypes.Contains(ActionType.Movement))
            {
                moveActionIndexes.Add(new Tuple<int> (i));
            }
        }

        int lowestMoveValue = int.MaxValue;
        int lowestMoveActionIndex = -1;
        List<Action> moveActionsCalculated = new List<Action>();
        List<Vector2Int> unitStartPositions = new List<Vector2Int>();
        bool ignorePassives = false;

        for (int i = 0; i < moveActionIndexes.Count; i++)
        {
            Action currentMoveAction = movingUnit.actions[moveActionIndexes[i].Item1].action;

            Tuple<int, Vector2Int, List<Action>, List<Vector2Int>> moveValue = currentMoveAction.MoveUnit(AiActionData);
            if(moveValue.Item1 < lowestMoveValue)
            {
                lowestMoveActionIndex = i;
                lowestMoveValue = moveValue.Item1;
                AiActionData.desiredEndPosition = moveValue.Item2;
                moveActionsCalculated = moveValue.Item3;
                unitStartPositions = moveValue.Item4;
            }
        }

        for (int i = 0; i < moveActionIndexes.Count; i++)
        {
            Action currentMoveAction = movingUnit.actions[moveActionIndexes[i].Item1].action;

            Tuple<int, Vector2Int, List<Action>, List<Vector2Int>> moveValue = currentMoveAction.MoveUnit(AiActionData, true);
            if (moveValue.Item1 < lowestMoveValue)
            {
                ignorePassives = true;
                lowestMoveActionIndex = i;
                lowestMoveValue = moveValue.Item1;
                AiActionData.desiredEndPosition = moveValue.Item2;
                moveActionsCalculated = moveValue.Item3;
                unitStartPositions = moveValue.Item4;
            }
        }
        Vector2Int endPosition = AiActionData.desiredEndPosition;
        AiActionData.movementActions[endPosition.x, endPosition.y] = moveActionsCalculated;
        AiActionData.startPositions[endPosition.x, endPosition.y] = unitStartPositions;
        AiActionData.ignorePassiveArea[endPosition.x, endPosition.y] = ignorePassives;

        if (lowestMoveActionIndex == -1)
        {
            movingUnit.EndTurnAction();
        }
        else
        {
            for (int i = 0; i < moveActionsCalculated.Count; i++)
            {
                moveActionsCalculated[i].AIUseAction(AiActionData);
            }
            gameManager.PlayActions();
        }
    }

    public void ChaffAI(Unit unit, bool positionOnly)
    {
        switch (AIState)
        {
            case (AITurnStates.Combat):     
                break;
            case (AITurnStates.Agressive):
                break;
            case (AITurnStates.Skirmish):
                break;
            case (AITurnStates.Convoy):
                break;
            case (AITurnStates.SuperiorRanged):
                break;
            case (AITurnStates.Unaware):
                break;
        }
    }

    public void FrontLineAI(Unit unit, bool positionOnly)
    {
        visibleUnits = unit.gameManager.playerTurn.playerUnits;
        bool inMelee = false;
        Vector2Int originalPosition = new Vector2Int(unit.x, unit.y);
        unit.gameManager.map.ResetMap(true);
        DijkstraMapNode currentunitNode = unit.gameManager.map.getGrid().GetGridObject(originalPosition);
        List<Vector2Int> mapNodes = unit.gameManager.map.getGrid().GetGridPositionsInRing(originalPosition.x, originalPosition.y, 1);
        for (int k = 0; k < mapNodes.Count; k++)
        {
            Vector2Int surroundingNodePosition = new Vector2Int(mapNodes[k].x, mapNodes[k].y);
            DijkstraMapNode surroundingUnitNode = unit.gameManager.map.getGrid().GetGridObject(surroundingNodePosition);
            Unit tempUnit = gameManager.grid.GetGridObject(surroundingNodePosition).unit;
            if (tempUnit != null && tempUnit.team != unit.team && unit.moveModifier.NewValidMeleeAttack(unit.gameManager, currentunitNode, surroundingUnitNode, 1))
            {
                inMelee = true;
                break;
            }
        }
        AIActionData AiActionData =  new AIActionData();
        AiActionData.unit = unit;
        AiActionData.originalPosition = originalPosition;
        AiActionData.AIState = AIState;
        AiActionData.expectedCurrentActionPoints = unit.currentActionsPoints;
        int actionIndex = -1;

        List<UnitActionData> movementActionData = new List<UnitActionData>();
        AiActionData.movementData = new int[gameManager.mapSize, gameManager.mapSize];
        AiActionData.movementActions = new List<Action>[gameManager.mapSize, gameManager.mapSize];
        AiActionData.startPositions = new List<Vector2Int>[gameManager.mapSize, gameManager.mapSize];
        AiActionData.ignorePassiveArea = new bool[gameManager.mapSize, gameManager.mapSize];

        List<Vector2Int> enemyUnitHexPositions = new List<Vector2Int>();
        for (int i = 0; i < visibleUnits.Count; i++)
        {
            enemyUnitHexPositions.Add(new Vector2Int(visibleUnits[i].x, visibleUnits[i].y));
        }
        AiActionData.enemyUnits = enemyUnitHexPositions;

        for (int i = 0; i < gameManager.mapSize; i++)
        {
            for (int j = 0; j < gameManager.mapSize; j++)
            {
                AiActionData.ignorePassiveArea[j, i] = true;
            }
        }

        for (int i = 0; i < gameManager.mapSize; i++)
        {
            for (int j = 0; j < gameManager.mapSize; j++)
            {
                AiActionData.movementData[i, j] = int.MaxValue;
            }
        }
        if (inMelee)
        {
            AiActionData.movementData[unit.x, unit.y] = 0;
            AiActionData.movementActions[unit.x, unit.y] = new List<Action>();
            Debug.Log("In Melee");
            if (positionOnly)
            {
                int unitIndex = units.IndexOf(unit);
                futureUnitPosiitions[unitIndex] = new Vector2Int(unit.x, unit.y);
                return;
            }
            AiActionData.inMelee = true;
            actionIndex = GetHighestActionIndex(AiActionData, unit);
            if (actionIndex == -1)
            {
                Debug.LogWarning("No Action found for highestIndex");
                unit.EndTurnAction();
            }
            else
            {
                unit.actions[actionIndex].action.AIUseAction(AiActionData);
            }
        }
        else
        {

            for (int i = 0; i < unit.actions.Count; i++)
            {
                if (unit.actions[i].action.actionTypes.Contains(ActionType.Movement))
                {
                    unit.actions[i].action.GetMovementMap(AiActionData);
                }
            }

            AiActionData.movementData[unit.x, unit.y] = 0;
            AiActionData.movementActions[unit.x, unit.y] = new List<Action>();

            AiActionData.inMelee = false;
            List<int> actionsInRange;

            switch (AIState)
            {
                case (AITurnStates.Combat):
                    actionsInRange = GetActionsInRange(AiActionData, unit);
                    if(actionsInRange.Count <= 0)
                    {
                        MeleeRangeOneGetCloserToEnemyUnits(AiActionData);
                    }
                    else
                    {
                        actionIndex = GetHighestActionIndex(AiActionData, unit, actionsInRange);
                        if(actionIndex == -1)
                        {
                            Debug.LogError("No Action found for highestIndex");
                        }
                        else
                        {
                            unit.actions[actionIndex].action.AIUseAction(AiActionData);
                        }
                    }
                    break;
                case (AITurnStates.Agressive):
                    actionsInRange = GetActionsInRange(AiActionData, unit);
                    if (actionsInRange.Count <= 0)
                    {
                        MeleeRangeOneGetCloserToEnemyUnits(AiActionData);
                    }
                    else
                    {
                        actionIndex = GetHighestActionIndex(AiActionData, unit, actionsInRange);
                        unit.actions[actionIndex].action.AIUseAction(AiActionData);
                    }
                    break;
                case (AITurnStates.Skirmish):
                    actionIndex = GetHighestActionIndex(AiActionData, unit);
                    break;
                case (AITurnStates.Convoy):
                    actionIndex = GetHighestActionIndex(AiActionData, unit);
                    break;
                case (AITurnStates.SuperiorRanged):
                    actionIndex = GetHighestActionIndex(AiActionData, unit);
                    break;
                case (AITurnStates.Unaware):
                    actionIndex = GetHighestActionIndex(AiActionData, unit);
                    break;
            }
        }
    }

    public int GetHighestActionIndex(AIActionData actionData, Unit unit)
    {
        int actionIndex = -1;
        int highestActionWeight = -1;
        for (int i = 0; i < unit.actions.Count; i++)
        {
            int actionWieght = unit.actions[i].action.CalculateWeight(actionData);
            if (highestActionWeight < actionWieght)
            {
                actionIndex = i;
                highestActionWeight = actionIndex;
            }
        }
        return actionIndex;
    }

    public int GetHighestActionIndex(AIActionData actionData, Unit unit, List<int> actionsInRange)
    {
        Debug.Log("test 3: " + actionData.movementData.GetLength(0) + ", " + actionData.movementData.GetLength(1));
        int actionIndex = -1;
        int highestActionWeight = 0;
        for (int i = 0; i < actionsInRange.Count; i++)
        {
            int actionWieght = unit.actions[actionsInRange[i]].action.CalculateWeight(actionData);
            Debug.Log(unit.actions[actionsInRange[i]].action + " action weight: " + actionWieght);      
            if (highestActionWeight < actionWieght)
            {
                actionIndex = i;
                highestActionWeight = actionIndex;
            }
        }

        if(actionIndex != -1)
        {
            return actionsInRange[actionIndex];
        }

        return -1;
    }

    public List<int> GetActionsInRange(AIActionData actionData, Unit unit)
    {
        List<int> actionsInRange = new List<int>();
        for (int i = 0; i < unit.actions.Count; i++)
        {

            if (unit.actions[i].action.actionTypes.Contains(ActionType.Movement))
            {
                continue;
            }

            if (unit.actions[i].action.CheckIfActionIsInRange(actionData))
            {
                actionsInRange.Add(i);
            }
        }
        return actionsInRange;
    }

    public void UnitDeath(Unit unit)
    {

    }
}
