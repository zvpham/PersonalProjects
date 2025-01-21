using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

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

    // This is for when there are spotted Enemy Units
    public void GetCloserToEnemyUnits(AIActionData AiActionData)
    {
        DijkstraMap map =  gameManager.map;
        map.ResetMap(true);

        AiActionData.unit.moveModifier.SetUnwalkable(gameManager, AiActionData.unit);
        map.SetGoalsNew(AiActionData.enemyUnits, gameManager, AiActionData.unit.moveModifier);

        Unit movingUnit  =  AiActionData.unit;
        int x = movingUnit.x;
        int y = movingUnit.y;
        List<Vector2Int> path = new List<Vector2Int>();
        path.Clear();
        int currentMoveSpeed = movingUnit.currentMoveSpeed + movingUnit.moveSpeedPerMoveAction;
        int startMoveSpeed = currentMoveSpeed;
        int previousNodeMoveValue = map.getGrid().GetGridObject(x, y).value;
        int startx = x;
        int starty = y;
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

        if(currentNode.value == int.MaxValue)
        {
            Debug.LogError("Test Unit Isolated on high terrain while they can't see enemyUnitss");
        }
        
        // Desired end position has a Unit on it, use modified path where friendly units are walkable
        if(gameManager.grid.GetGridObject(x, y).unit != null)
        {
            List<DijkstraMapNode> newGoals =  
                map.GetNodesThatHaveValueLessThanTarget(map.getGrid().GetGridObject(x, y).value, startMoveSpeed, AiActionData.enemyUnits);
            for (int i = 0; i < newGoals.Count; i++)
            {
                if (gameManager.grid.GetGridObject(x, i).unit != null)
                {
                    newGoals.RemoveAt(i);
                    i--;
                }
            }
            map.ResetMap();
            map.SetGoalsNew(AiActionData.enemyUnits, gameManager, AiActionData.unit.moveModifier);

            x = movingUnit.x;
            y = movingUnit.y;
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

            if (path.Count != 0)
            {

            }
            else
            {
                Debug.LogError("Path not found when attempting to approach enemyUnits for: " + movingUnit.name);
            }
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

        if (inMelee)
        {
            if (positionOnly)
            {
                int unitIndex = units.IndexOf(unit);
                futureUnitPosiitions[unitIndex] = new Vector2Int(unit.x, unit.y);
                return;
            }
            AiActionData.inMelee = true;
            actionIndex = GetHighestActionIndex(AiActionData, unit);
        }
        else
        {

            AiActionData.movementData = new int[gameManager.mapSize, gameManager.mapSize];
            AiActionData.movementActions = new List<Action>[gameManager.mapSize, gameManager.mapSize];
            AiActionData.startPositions = new List<Vector2Int>[gameManager.mapSize, gameManager.mapSize];
            AiActionData.ignorePassiveArea = new bool[gameManager.mapSize, gameManager.mapSize];
            for(int i = 0; i < gameManager.mapSize; i++)
            {
                for(int j = 0; j < gameManager.mapSize; j++)
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

            for (int i = 0; i < unit.actions.Count; i++)
            {
                if (unit.actions[i].action.actionTypes.Contains(ActionType.Movement))
                {
                    unit.actions[i].action.GetMovementMap(AiActionData);
                }
            }



            AiActionData.inMelee = false;
            List<Vector2Int> enemyUnitHexPositions = new List<Vector2Int>();
            for(int i = 0; i < visibleUnits.Count; i++)
            {
                enemyUnitHexPositions.Add(new Vector2Int(visibleUnits[i].x, visibleUnits[i].y));
            }
            AiActionData.enemyUnits = enemyUnitHexPositions;
            List<int> actionsInRange;
            switch (AIState)
            {
                case (AITurnStates.Combat):
                    actionsInRange = GetActionsInRange(AiActionData, unit);
                    if(actionsInRange.Count <= 0)
                    {
                        GetCloserToEnemyUnits(AiActionData);
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
                        GetCloserToEnemyUnits(AiActionData);
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
}
