using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows;

public class MeleeTargeting : TargetingSystem
{
    public DijkstraMap map;
    public CombatGameManager gameManager;
    public Unit movingUnit;
    public Vector2Int startingPosition;
    public Vector2Int currentlySelectedhex;
    // For End Targeting/ Mouse Up
    public Vector2Int prevEndHexPosition;    
    // For Selection/ Mouse Hover (Combat UI)
    public Vector2Int prevEndHexSelectedPosition;
    public List<Vector2Int> path = new List<Vector2Int>();
    public List<Vector2Int> setPath = new List<Vector2Int>();
    public List<Vector2Int> targetHexPositions =  new List<Vector2Int>();
    public List<Vector2Int> enemyGroundHexes;
    public List<Vector2Int> groundHexes;
    List<Vector2Int> friendlyUnits = new List<Vector2Int>();
    List<Vector2Int> validTargetPositions = new List<Vector2Int>();

    public List<int> startOfNewMoveIndexes = new List<int>();
    public List<int> setStartOfNewMoveIndexes = new List<int>();

    public List<PassiveEffectArea>[,] passives;
    List<PassiveEffectArea> passiveEffectAreas = new List<PassiveEffectArea>();
    List<Tuple<Passive, Vector2Int>> passiveSprites = new List<Tuple<Passive, Vector2Int>>();
    public bool[,] unwalkablePassivesValues;
    public bool[,] badWalkInPassivesValues;
    public bool[,] goodWalkinPassivesValues;

    public List<int> groundColorValues;
    public List<GameObject> highlightedTargetedHexes = new List<GameObject>();
    public List<GameObject> highlightedHexes = new List<GameObject>();
    public List<GameObject> rangeHexes = new List<GameObject>();
    public List<SpriteHolder> targetingPassiveSpriteHolder = new List<SpriteHolder>();
    public GameObject tempMovingUnit;

    public int meleeRange;
    public int actionPointUseAmount;


    public List<int> actionMoveAmounts;
    public int amountOfPossibleMoves;
    public int amountMoved = 0;
    public int moveSpeedUsed = 0;
    public int moveSpeedInitiallyAvailable = 0;

    List<List<Vector3>> actionLines = new List<List<Vector3>>();

    public bool EnoughActionPointsForMeleeActionOnly;
    public bool selectedTarget = false;
    public bool targetFriendly = false;
    public bool canMove;
    public bool keepCombatAttackUi = false;
    public bool selectOnTarget;
    public bool unitAttemptingToMove = false;

    public int actionPointsLeft;

    public delegate List<AttackDataUI> CalculateAttackData(Unit movingUnit, Unit targetUnit, List<Vector2Int> movementPath);
    public CalculateAttackData calculateAttackData;

    //Path, MovingUnit, TargetUnit, FoundTarget
    public UnityAction<Unit, Unit, bool> OnFoundTarget;

    public void SetParameters(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int actionPointUseAmount, int meleeRange,
        CalculateAttackData calculateAttackData)
    {
        this.targetFriendly = targetFriendly;
        this.movingUnit = movingUnit;
        this.gameManager = movingUnit.gameManager;  
        this.actionPointsLeft = actionPointsLeft;
        this.actionPointUseAmount = actionPointUseAmount;
        if(meleeRange != 1)
        {
            Debug.LogError("This SHould only be used for melee Range 1 action");
        }
        this.meleeRange = 1;
        this.calculateAttackData = calculateAttackData;
        map = movingUnit.gameManager.map;
        startingPosition = new Vector2Int(movingUnit.x, movingUnit.y);
        selectedTarget = false;
        unitAttemptingToMove = false;
        prevEndHexPosition = new Vector2Int(-10, 0);
        path = new List<Vector2Int>();
        this.enabled = true;
        amountMoved = movingUnit.actions[0].amountUsedDuringRound;

        passives = new List<PassiveEffectArea>[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < gameManager.mapSize; i++)
        {
            for (int j = 0; j < gameManager.mapSize; j++)
            {
                passives[i, j] = new List<PassiveEffectArea>();
            }
        }

        List<List<PassiveEffectArea>> classifiedPassiveEffectArea = movingUnit.CalculuatePassiveAreas();
        unwalkablePassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];

        for (int i = 0; i < classifiedPassiveEffectArea[0].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[0][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[0][i].passiveLocations[j];
                unwalkablePassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[0][i]);
            }
        }

        badWalkInPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < classifiedPassiveEffectArea[1].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[1][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[1][i].passiveLocations[j];
                badWalkInPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[1][i]);
            }
        }

        goodWalkinPassivesValues = new bool[gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < classifiedPassiveEffectArea[2].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[2][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[2][i].passiveLocations[j];
                goodWalkinPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[2][i]);
            }
        }
        List<Unit> units = gameManager.units;
        friendlyUnits = new List<Vector2Int>();
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].team == movingUnit.team)
            {
                friendlyUnits.Add(new Vector2Int(units[i].x, units[i].y));
            }
        }

        SetUp(startingPosition, actionPointsLeft, movingUnit.currentMoveSpeed);
    }

    public void SetUp(Vector2Int targetPosition, int numActionPoints, int currentMoveSpeed)
    {

        ResetSetUp();

        List<Unit> validTargets = GetValidTargets();
        validTargetPositions =  new List<Vector2Int>();
        for(int i = 0; i < validTargets.Count; i++)
        {
            validTargetPositions.Add(new Vector2Int(validTargets[i].x, validTargets[i].y));
        }

        moveSpeedInitiallyAvailable = currentMoveSpeed;
        DijkstraMap map = gameManager.map;
        int x = targetPosition.x;
        int y = targetPosition.y;
        map.ResetMap(true);
        map.SetGoalsMelee(new List<Vector2Int>() { new Vector2Int(x, y) }, friendlyUnits, validTargetPositions,
            gameManager, movingUnit.moveModifier, meleeRange);

        startingPosition = targetPosition;
        actionPointsLeft = numActionPoints;
        int initialActionPoints = numActionPoints - actionPointUseAmount;
        int usableActionPoints = initialActionPoints;
        int moveAmounts = 0;

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

        int startValue = currentMoveSpeed + (movingUnit.moveSpeedPerMoveAction * moveAmounts);

        Debug.Log("iNITAIL aAction points: " + initialActionPoints + ", " + currentMoveSpeed);
        //canMove = CanUnitMove(movingUnit, initialActionPoints, amountMoved, currentMoveSpeed);
        groundHexes = new List<Vector2Int>();
        groundColorValues = new List<int>();
        enemyGroundHexes = new List<Vector2Int>();

        List<DijkstraMapNode> nodesInMeleeRange =  map.GetNodesInMeleeRange(targetPosition, startValue, null, validTargetPositions, 
            gameManager, movingUnit.moveModifier, meleeRange);

        Debug.Log(nodesInMeleeRange.Count);
        if (nodesInMeleeRange.Count > 1)
        {
            for (int i = 1; i < nodesInMeleeRange.Count; i++) // Start at one to avoid moving Unit
            {
                DijkstraMapNode currentNode = nodesInMeleeRange[i];

                Vector2Int currentNodePosition = new Vector2Int(currentNode.x, currentNode.y);
                Unit targetUnit = gameManager.grid.GetGridObject(currentNodePosition.x, currentNodePosition.y).unit;
                if (targetUnit != null && (targetUnit.team != movingUnit.team && !targetFriendly || targetUnit.team == movingUnit.team && 
                    targetFriendly))
                {
                    enemyGroundHexes.Add(currentNodePosition);
                    targetHexPositions.Add(currentNodePosition);
                }

            }
        }

        amountOfPossibleMoves = moveAmounts;
        if (canMove)
        {
            startingPosition = targetPosition;
            actionPointsLeft = numActionPoints;
            amountOfPossibleMoves = moveAmounts;

            List<DijkstraMapNode> nodesInMovementRange = map.GetNodesInMovementRange(x, y, startValue, movingUnit.moveModifier, gameManager);
            movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
            if (nodesInMovementRange.Count > 1)
            {
                for (int i = 0; i < nodesInMovementRange.Count; i++)
                {
                    DijkstraMapNode currentNode = nodesInMovementRange[i];
                    int nodeValue = startValue - currentNode.value;
                    Vector2Int currentNodePosition = new Vector2Int(currentNode.x, currentNode.y);
                    if (enemyGroundHexes.Contains(currentNodePosition))
                    {
                        continue;
                    }
                    int rangeBracketOfNode;
                    if (currentMoveSpeed > 0)
                    {
                        int tempNodeValue = nodeValue - currentMoveSpeed;
                        if (tempNodeValue <= 0)
                        {
                            Debug.Log("rangeOfBracketNode 1: " + (amountMoved - 1));
                            rangeBracketOfNode = amountMoved - 1;
                        }
                        else
                        {
                            tempNodeValue -= 1;
                            Debug.Log("rangeOfBracketNode 2: " + tempNodeValue);
                            rangeBracketOfNode = tempNodeValue / (movingUnit.moveSpeedPerMoveAction) + amountMoved;
                        }
                    }
                    else
                    {
                        nodeValue -= 1;
                        Debug.Log("rangeOfBracketNode 3: " + nodeValue);
                        rangeBracketOfNode = (nodeValue / (movingUnit.moveSpeedPerMoveAction)) + amountMoved;
                    }
                    groundHexes.Add(currentNodePosition);
                    Debug.Log("rangeOfBracketNode: " + rangeBracketOfNode);
                    groundColorValues.Add(rangeBracketOfNode);
                }
            }
        }
        else
        {
            //Make Units Unwalkable
            for (int i = 0; i < gameManager.units.Count; i++)
            {
                if (gameManager.units[i].team != movingUnit.team)
                {
                    map.getGrid().GetXY(gameManager.units[i].transform.position, out x, out y);
                    map.SetUnwalkable(new Vector2Int(x, y));
                }
            }
        }
        canMove = groundHexes.Count > 0;
        PlaceGroundHexes();
    }

    public List<Unit> GetValidTargets()
    {
        List<Unit> units = gameManager.units;
        List<Unit> validTargets = new List<Unit>();
        if (targetFriendly)
        {
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].team == movingUnit.team)
                {
                    validTargets.Add(units[i]);
                }
            }
        }
        else
        {
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i].team != movingUnit.team)
                {
                    validTargets.Add(units[i]);
                }
            }
        }
        return validTargets;
    }

    public void PlaceGroundHexes()
    {
        for(int i = 0; i < groundColorValues.Count; i++)
        {
            GameObject newHighlightedHex = gameManager.spriteManager.UseOpenHighlightedHex();
            Vector2Int currentNodePosition = groundHexes[i];
            newHighlightedHex.transform.position = gameManager.spriteManager.GetWorldPosition(currentNodePosition);
            newHighlightedHex.GetComponent<SpriteRenderer>().color = gameManager.resourceManager.highlightedHexColors[groundColorValues[i]];
            newHighlightedHex.GetComponent<SpriteRenderer>().sortingOrder = gameManager.spriteManager.terrain[currentNodePosition.x,
                currentNodePosition.y].sprite.sortingOrder + 1;
            highlightedTargetedHexes.Add(newHighlightedHex);
        }

        for (int i = 0; i < enemyGroundHexes.Count; i++)
        {
            GameObject newHighlightedHex = gameManager.spriteManager.UseOpenHighlightedHex();
            Vector2Int currentNodePosition = enemyGroundHexes[i];
            newHighlightedHex.transform.position = gameManager.spriteManager.GetWorldPosition(currentNodePosition);
            newHighlightedHex.GetComponent<SpriteRenderer>().color = gameManager.resourceManager.highlightedHexColors[3];
            newHighlightedHex.GetComponent<SpriteRenderer>().sortingOrder = gameManager.spriteManager.terrain[currentNodePosition.x,
                currentNodePosition.y].sprite.sortingOrder + 1;
            highlightedTargetedHexes.Add(newHighlightedHex);
        }
    }


    // Clear Highlighted Hexes
    public void ResetSetUp()
    {
        for (int i = 0; i < highlightedHexes.Count; i++)
        {
            gameManager.spriteManager.DisableHighlightedHex(highlightedHexes[i]);
        }
        highlightedHexes = new List<GameObject>();

        for (int i = 0; i < highlightedTargetedHexes.Count; i++)
        {
            gameManager.spriteManager.DisableHighlightedHex(highlightedTargetedHexes[i]);
        }
        highlightedTargetedHexes = new List<GameObject>();

        for (int i = 0; i < rangeHexes.Count; i++)
        {
            gameManager.spriteManager.DisableTargetHex(rangeHexes[i]);
        }
        rangeHexes.Clear();
        targetHexPositions.Clear();
    }

    //Mouse Hover
    public override void SelectNewPosition(Vector2Int newHex)
    {
        if (!selectedTarget)
        {
            currentlySelectedhex = newHex;
            map.ResetMap();
            List<Vector2Int> endHex = new List<Vector2Int>();
            int endX = currentlySelectedhex.x;
            int endY = currentlySelectedhex.y;

            if (map.getGrid().GetGridObject(endX, endY) != null)
            {
                endHex.Add(new Vector2Int(endX, endY));
                map.SetGoalsNew(endHex, gameManager, movingUnit.moveModifier, badWalkInPassivesValues);
                int x = startingPosition.x;
                int y = startingPosition.y;
                path.Clear();
                startOfNewMoveIndexes = new List<int>();
                bool foundEndPosition = false;

                int currentMoveSpeed = moveSpeedInitiallyAvailable + movingUnit.moveSpeedPerMoveAction * amountOfPossibleMoves;
                int previousNodeMoveValue = map.getGrid().GetGridObject(x, y).value;
                int startx = x;
                int starty = y;
                moveSpeedUsed = 0;


                //Target Unit
                bool endHexIsValidTargetUnit = false;

                Unit targetUnit = null;
                if (gameManager.grid.GetGridObject(endX, endY) != null)
                {
                    targetUnit = gameManager.grid.GetGridObject(endX, endY).unit;
                }

                if (targetUnit != null && (targetUnit.team != Team.Player || (targetFriendly && targetUnit.team == Team.Player)))
                {
                    endHexIsValidTargetUnit = true;
                }

                if (canMove)
                {
                    EnoughActionPointsForMeleeActionOnly = false;
                    if (endHexIsValidTargetUnit && IsHexInRange(movingUnit, startingPosition, endHex[0], meleeRange) &&
                        movingUnit.LineOfSight(startingPosition, new Vector2Int(endX, endY)))
                    {
                        Debug.Log("Found End Position");
                        foundEndPosition = true;
                        selectOnTarget = true;
                    }
                    else
                    {
                        selectOnTarget = false;
                        while (true)
                        {
                            DijkstraMapNode currentNode;
                            currentNode = map.GetLowestNearbyNode(x, y, endHex[0], movingUnit.moveModifier, gameManager);
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

                            if (endHex[0] == path[path.Count - 1] || (endHexIsValidTargetUnit &&
                                    IsHexInRange(movingUnit, path[path.Count - 1], endHex[0], meleeRange)))
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

                        if (!foundEndPosition)
                        {
                            currentMoveSpeed += moveSpeedUsed;
                            moveSpeedUsed = 0;
                            map.ResetMap(true);
                            movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
                            map.SetGoalsNew(endHex, gameManager, movingUnit.moveModifier);
                            x = startx;
                            y = starty;
                            previousNodeMoveValue = map.getGrid().GetGridObject(x, y).value;
                            path.Clear();
                            while (true)
                            {
                                DijkstraMapNode currentNode;
                                currentNode = map.GetLowestNearbyNode(x, y, endHex[0], movingUnit.moveModifier, gameManager);
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

                                if (endHex[0] == path[path.Count - 1] || (endHexIsValidTargetUnit &&
                                    IsHexInRange(movingUnit, path[path.Count - 1], endHex[0], meleeRange)))
                                {
                                    break;
                                }
                                else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
                                {
                                    path.RemoveAt(path.Count - 1);
                                    break;
                                }
                            }
                        }
                    }
                }
                // Case - Only have enough action points to perform action
                else if (actionPointsLeft >= actionPointUseAmount)
                {
                    EnoughActionPointsForMeleeActionOnly = true;
                    endHex.Add(new Vector2Int(endX, endY));
                    path.Clear();
                    x = startingPosition.x;
                    y = startingPosition.y;
                    path.Add(new Vector2Int(x, y));
                }
                else
                {
                    Debug.Log("this SHouldn't happen");
                }


                if (prevEndHexSelectedPosition != endHex[0] && !keepCombatAttackUi)
                {
                    gameManager.spriteManager.ResetCombatAttackUI();
                    prevEndHexSelectedPosition = endHex[0];
                    if (targetUnit != null && targetHexPositions.Contains(endHex[0]))
                    {
                        List<AttackDataUI> attackDatas = calculateAttackData(movingUnit, targetUnit, path);
                        gameManager.spriteManager.ActivateCombatAttackUI(targetUnit, attackDatas, targetUnit.transform.position);
                    }
                }
                DrawLine();
            }
        }
    }

    // On Mouse UP
    public override void EndTargeting()
    {
        int x = currentlySelectedhex.x;
        int y = currentlySelectedhex.y;
        if (x >= gameManager.grid.GetWidth() || x < 0 || y >= gameManager.grid.GetHeight() || y < 0)
        {
            return;
        }
        Vector2Int endHexPosition = new Vector2Int(x, y);

        bool targetInRange = false;
        int startingPositionDistanceFromMouse = map.getGrid().GetGridObject(startingPosition).value;
        if (startingPositionDistanceFromMouse <= meleeRange + movingUnit.moveSpeed * amountOfPossibleMoves)
        {
            targetInRange = true;
        }

        // Case -  Player Clicks on previously selected Hex
        // Confirms action
        if (prevEndHexPosition.x >= 0 && endHexPosition == prevEndHexPosition)
        {
                gameManager.spriteManager.ConfirmAction();
        }
        //Case - Player clicks on hex that has not been previously selected
        else
        {
            Vector2Int endPathHex;
            // Select Player Unit when you aren't using a friendly ability like heal
            if (!targetFriendly && gameManager.playerTurn.CheckSpaceForFriendlyUnit(endHexPosition))
            {
                OnFoundTarget?.Invoke(movingUnit, null, false);
                gameManager.playerTurn.SelectUnit(endHexPosition);
            }
            else
            {
                Unit targetUnit = gameManager.grid.GetGridObject(endHexPosition.x, endHexPosition.y).unit;
                // if still able to move set PRevious Path Normally
                if (canMove)
                {
                    if (actionPointsLeft > 0)
                    {
                        for (int i = 0; i < startOfNewMoveIndexes.Count; i++)
                        {
                            setStartOfNewMoveIndexes.Add(startOfNewMoveIndexes[i]);
                        }

                        prevEndHexPosition = endHexPosition;
                        for (int i = 0; i < path.Count; i++)
                        {
                            setPath.Add(path[i]);
                        }
                    }
                }
                // If not able to move then only check if Mouse Path is in melee Range
                else if (startingPositionDistanceFromMouse <= meleeRange && targetUnit != null)
                {
                    prevEndHexPosition = endHexPosition;
                }

                Vector2Int lineOfSightStartHex = startingPosition;
                if(setPath.Count > 0)
                {
                    lineOfSightStartHex = setPath[setPath.Count - 1];
                }

                //Case -  MouseHex contains a Unit that is a valid Target
                if (targetUnit != null && targetInRange && 
                    movingUnit.LineOfSight(lineOfSightStartHex, new Vector2Int(targetUnit.x, targetUnit.y)))
                {
                    bool targetInMeleeRange = false;
                    actionPointsLeft -= actionPointUseAmount;
                    int moveSpeedLeft = moveSpeedInitiallyAvailable - moveSpeedUsed;
                    if (startingPositionDistanceFromMouse <= meleeRange)
                    {
                        targetInMeleeRange = true;
                    }
                    else
                    {
                        // Calculate Action points used
                        int movementActionsTaken;
                        if (moveSpeedLeft >= 0)
                        {
                            movementActionsTaken = 0;
                        }
                        else
                        {
                            movementActionsTaken = (((moveSpeedLeft + 1) / movingUnit.moveSpeedPerMoveAction) - 1) * -1;
                        }

                        int actionPointsUsed = 0;
                        int previousAmountMoved = amountMoved;
                        for (int i = 0; i < movementActionsTaken; i++)
                        {
                            actionPointsUsed += amountMoved + 1;
                            moveSpeedLeft += movingUnit.moveSpeedPerMoveAction;
                            amountMoved += 1;
                        }
                        actionPointsLeft -= actionPointsUsed;
                    }
                    keepCombatAttackUi = true;
                    // Case target is within meleeRange
                    if (targetInMeleeRange && !unitAttemptingToMove)
                    {
                        SetUp(startingPosition, 0, movingUnit.currentMoveSpeed);
                        gameManager.spriteManager.ActivateActionConfirmationMenu(
                            () => // Confirm Action
                            {
                                selectedTarget = true;
                                OnFoundTarget?.Invoke(movingUnit, targetUnit, true);
                                Destroy(tempMovingUnit);
                            },
                            () => // Cancel Action
                            {
                                ResetTargeting();
                            });
                    }
                    // Case - Target Unit is in movement +  Melee Range
                    else
                    {
                        endPathHex = setPath[setPath.Count - 1];
                        Destroy(tempMovingUnit);
                        tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, 1, movingUnit.unitProfile);
                        SetUp(setPath[setPath.Count - 1], 0, moveSpeedLeft);
                        gameManager.spriteManager.ActivateActionConfirmationMenu(
                            () => // Confirm Action
                            {
                                selectedTarget = true;
                                gameManager.move.AnotherActionMove(setPath, setStartOfNewMoveIndexes, movingUnit, false);
                                OnFoundTarget?.Invoke(movingUnit, targetUnit, true);
                                Destroy(tempMovingUnit);
                            },
                            () => // Cancel Action
                            {
                                ResetTargeting();
                            });
                    }
                }   
                // Case - Mouse Hex doesn't contain a Unit and Moving Unit Can Still move
                else if (prevEndHexPosition == endHexPosition && canMove)
                {
                    endPathHex = setPath[setPath.Count - 1];

                    // Calculate Action points used
                    int moveSpeedLeft = moveSpeedInitiallyAvailable - moveSpeedUsed;
                    int movementActionsTaken;
                    if (moveSpeedLeft >= 0)
                    {
                        movementActionsTaken = 0;
                    }
                    else
                    {
                        movementActionsTaken = (((moveSpeedLeft + 1) / movingUnit.moveSpeedPerMoveAction) - 1) * -1;
                    }

                    int actionPointsUsed = 0;
                    int previousAmountMoved = amountMoved;
                    for (int i = 0; i < movementActionsTaken; i++)
                    {
                        actionPointsUsed += amountMoved + 1;
                        moveSpeedLeft += movingUnit.moveSpeedPerMoveAction;
                        amountMoved += 1;
                    }
                    actionPointsLeft -= actionPointsUsed;

                    Destroy(tempMovingUnit);
                    tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, 1, movingUnit.unitProfile);
                    SetUp(setPath[setPath.Count - 1], actionPointsLeft, moveSpeedLeft);
                    unitAttemptingToMove = true;
                    gameManager.spriteManager.ActivateActionConfirmationMenu(
                        () => // Confirm Action
                        {
                            //gameManager.move.AnotherActionMove(setPath, amountMoved movingUnit);
                            if (setPath.Count == 1 && movingUnit.gameManager.map.getGrid().GetWorldPosition(setPath[0].x, setPath[0].y) == movingUnit.transform.position)
                            {
                                OnFoundTarget?.Invoke(movingUnit, null, false);
                                gameManager.playerTurn.SelectUnit(movingUnit);
                            }
                            else
                            {
                                gameManager.move.AnotherActionMove(setPath, setStartOfNewMoveIndexes, movingUnit, true);
                                Destroy(tempMovingUnit);
                            }
                        },
                        () => // Cancel Action
                        {
                            SetUp(new Vector2Int(movingUnit.x, movingUnit.y), movingUnit.currentActionsPoints, movingUnit.currentMoveSpeed);
                            ResetTargeting();
                        });
                }
            }
        }
    }

    public void ResetTargeting()
    {
        Destroy(tempMovingUnit);
        tempMovingUnit = null;
        selectedTarget = false;
        keepCombatAttackUi = false;
        prevEndHexPosition = new Vector2Int(-1, -1);
        path = new List<Vector2Int>();
        setPath = new List<Vector2Int>();
        actionMoveAmounts = new List<int>();
        setStartOfNewMoveIndexes = new List<int>();
        actionLines = new List<List<Vector3>>();
        actionPointsLeft = movingUnit.currentActionsPoints;
        amountMoved = movingUnit.actions[0].amountUsedDuringRound;
        gameManager.spriteManager.ResetCombatAttackUI();
        gameManager.spriteManager.ClearLines();
        for (int i = 0; i < targetingPassiveSpriteHolder.Count; i++)
        {
            gameManager.spriteManager.DisableTargetingSpriteHolder(targetingPassiveSpriteHolder[i]);
        }
        targetingPassiveSpriteHolder.Clear();
        SetUp(new Vector2Int(movingUnit.x, movingUnit.y), actionPointsLeft, movingUnit.currentMoveSpeed);
    }

    public override void DeactivateTargetingSystem()
    {
        ResetTargeting();
        ResetSetUp();
        map.ResetMap(true);
        OnFoundTarget = null;
    }

    public void DrawLine()
    {
        if (actionPointsLeft <= 0)
        {
            return;
        }

        actionLines = new List<List<Vector3>>() { new List<Vector3>() };
        actionLines[0].Add(movingUnit.transform.position);
        for (int i = 0; i < setPath.Count; i++)
        {
            actionLines[0].Add(gameManager.spriteManager.GetWorldPosition(setPath[i].x, setPath[i].y));
        }

        for (int i = 0; i < path.Count; i++)
        {
            actionLines[0].Add(gameManager.spriteManager.GetWorldPosition(path[i].x, path[i].y));
        }

        gameManager.spriteManager.DrawLine(actionLines[0], 0);

        // Draw meleeRange
        Vector2Int endHex = Vector2Int.zero;
        bool noPath = false;

        if (EnoughActionPointsForMeleeActionOnly || selectOnTarget)
        {
            int x = startingPosition.x;
            int y = startingPosition.y;
            endHex = new Vector2Int(x, y);
        }
        else
        {
            if(path.Count == 0)
            {
                noPath = true;
            }
            else
            {
                endHex = path[path.Count - 1];
            }
        }

        for(int i = 0; i < rangeHexes.Count; i++)
        {
            gameManager.spriteManager.DisableTargetHex(rangeHexes[i]);
        }
        rangeHexes.Clear();

        passiveEffectAreas = new List<PassiveEffectArea>();
        passiveSprites = new List<Tuple<Passive, Vector2Int>>();

        for (int i = 0; i < targetingPassiveSpriteHolder.Count; i++)
        {
            gameManager.spriteManager.DisableTargetingSpriteHolder(targetingPassiveSpriteHolder[i]);
        }
        targetingPassiveSpriteHolder.Clear();
        if (noPath)
        {
            Debug.Log("Return");
            return;
        }

        CheckPassives(new List<Vector2Int>() { new Vector2Int(movingUnit.x, movingUnit.y) });
        CheckPassives(setPath);
        CheckPassives(path);

        //Hex Position, num Passives
        Dictionary<Vector2Int, int> passivesGrid = new Dictionary<Vector2Int, int>();
        for (int i = 0; i < passiveSprites.Count; i++)
        {
            SpriteHolder tempTargetPassiveSprite = gameManager.spriteManager.UseOpenTargetingSpriteHolder();
            if (passivesGrid.ContainsKey(passiveSprites[i].Item2))
            {
                passivesGrid[passiveSprites[i].Item2] += 1;
            }
            else
            {
                passivesGrid.Add(passiveSprites[i].Item2, 1);
            }
            tempTargetPassiveSprite.transform.position = gameManager.spriteManager.GetWorldPosition(passiveSprites[i].Item2) + new Vector3(-0.6f + (0.3f * passivesGrid[passiveSprites[i].Item2]), -0.4f, 0);
            tempTargetPassiveSprite.spriteRenderer.sortingOrder = gameManager.spriteManager.terrain[passiveSprites[i].Item2.x, passiveSprites[i].Item2.y].sprite.sortingOrder + 10;
            tempTargetPassiveSprite.spriteRenderer.sprite = passiveSprites[i].Item1.UISkillImage;
            targetingPassiveSpriteHolder.Add(tempTargetPassiveSprite);
        }

        gameManager.map.ResetMap(true);
        map.SetGoalsMelee(new List<Vector2Int>() { endHex }, friendlyUnits, validTargetPositions, gameManager, movingUnit.moveModifier, meleeRange);
        for (int i = 1; i <= meleeRange; i++)
        {
            List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(endHex.x, endHex.y, i);
            for (int j = 0; j < mapNodes.Count; j++)
            {
                if (movingUnit.moveModifier.ValidMeleeAttack(gameManager, map.getGrid().GetGridObject(endHex), mapNodes[j], meleeRange))
                {
                    Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                    GameObject newTargetHex = gameManager.spriteManager.UseOpenTargetHex();
                    newTargetHex.transform.position = gameManager.spriteManager.GetWorldPosition(currentNodePosition);
                    newTargetHex.GetComponent<SpriteRenderer>().sortingOrder = gameManager.spriteManager.terrain[currentNodePosition.x,
                        currentNodePosition.y].sprite.sortingOrder + 2;
                    rangeHexes.Add(newTargetHex);
                }
            }
        }
    }

    public void CheckPassives(List<Vector2Int> moveLocations)
    {
        for (int i = 0; i < moveLocations.Count; i++)
        {
            Vector2Int pathLocation = moveLocations[i];
            List<PassiveEffectArea> passivesOnLocation = passives[pathLocation.x, pathLocation.y];
            List<Passive> passivesUsed = new List<Passive>();
            for (int j = 0; j < passivesOnLocation.Count; j++)
            {
                if (!passiveEffectAreas.Contains(passivesOnLocation[j]) && !passivesUsed.Contains(passivesOnLocation[j].passive.passive))
                {
                    passiveEffectAreas.Add(passivesOnLocation[j]);
                    Tuple<Passive, Vector2Int> tempPassiveSprite = passivesOnLocation[j].GetTargetingData(new Vector2Int(movingUnit.x, movingUnit.y), path,
                        setPath, passivesOnLocation[j].passiveLocations);
                    if (tempPassiveSprite.Item2.x != -1)
                    {
                        passiveSprites.Add(tempPassiveSprite);
                    }
                    passivesUsed.Add(passivesOnLocation[j].passive.passive);
                }
            }
        }
    }

    public override void NextItem()
    {

    }

    public override void PreviousItem()
    {

    }
}
