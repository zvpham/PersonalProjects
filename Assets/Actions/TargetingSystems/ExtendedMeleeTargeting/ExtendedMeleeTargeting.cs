using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ExtendedMeleeTargeting : TargetingSystem
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
    public List<Vector2Int> targetHexPositions = new List<Vector2Int>();
    public List<Vector2Int> enemyGroundHexes;
    public List<Vector2Int> groundHexes;
    List<Vector2Int> friendlyUnits = new List<Vector2Int>();
    List<Vector2Int> validTargetPositions = new List<Vector2Int>();

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
    public int amountOfPossibleMoves;
    public int actionPointsLeft;
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

    public delegate List<AttackDataUI> CalculateAttackData(Unit movingUnit, Unit targetUnit, List<Vector2Int> movementPath);
    public CalculateAttackData calculateAttackData;

    //Path, MovingUnit, TargetUnit, FoundTarget
    public UnityAction<Unit, Unit, bool> OnFoundTarget;

    public void SetParameters(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int actionPointUseAmount, int meleeRange,
        CalculateAttackData calculateAttackData)
    {
        Debug.Log("Moving Unit: " + movingUnit.currentMajorActionsPoints + ", " + actionPointsLeft);
        this.targetFriendly = targetFriendly;
        this.movingUnit = movingUnit;
        this.gameManager = movingUnit.gameManager;
        this.actionPointsLeft = actionPointsLeft;
        this.actionPointUseAmount = actionPointUseAmount;
        this.meleeRange = meleeRange;
        this.calculateAttackData = calculateAttackData;
        map = movingUnit.gameManager.map;
        startingPosition = new Vector2Int(movingUnit.x, movingUnit.y);
        selectedTarget = false;
        unitAttemptingToMove = false;
        prevEndHexPosition = new Vector2Int(-10, 0);
        path = new List<Vector2Int>();
        this.enabled = true;

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

        SetUp(startingPosition, actionPointsLeft, movingUnit.currentMoveSpeed);
    }

    public void SetUp(Vector2Int targetPosition, int numActionPoints, int currentMoveSpeed)
    {
        ResetSetUp();

        List<Unit> units = gameManager.units;
        friendlyUnits = new List<Vector2Int>();
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].team == movingUnit.team)
            {
                friendlyUnits.Add(new Vector2Int(units[i].x, units[i].y));
            }
        }

        List<Unit> validTargets = GetValidTargets();
        validTargetPositions = new List<Vector2Int>();
        for (int i = 0; i < validTargets.Count; i++)
        {
            validTargetPositions.Add(new Vector2Int(validTargets[i].x, validTargets[i].y));
        }

        moveSpeedInitiallyAvailable = currentMoveSpeed;
        DijkstraMap map = gameManager.map;
        int x = targetPosition.x;
        int y = targetPosition.y;
        map.ResetMap(true);

        int originElevation = gameManager.spriteManager.elevationOfHexes[x, y];
        startingPosition = targetPosition;
        actionPointsLeft = numActionPoints;
        int initialActionPoints = numActionPoints - actionPointUseAmount;
        int moveAmounts = initialActionPoints;

        int startValue = currentMoveSpeed + (movingUnit.moveSpeedPerMoveAction * moveAmounts);

        groundHexes = new List<Vector2Int>();
        groundColorValues = new List<int>();
        enemyGroundHexes = new List<Vector2Int>();
        map.ResetMap();

        for (int i = 1; i <= meleeRange; i++)
        {
            List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(x, y, i);
            for (int j = 0; j < mapNodes.Count; j++)
            {
                Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                Unit targetUnit = gameManager.grid.GetGridObject(currentNodePosition.x, currentNodePosition.y).unit;
                int targetElevation = gameManager.spriteManager.elevationOfHexes[mapNodes[j].x, mapNodes[j].y];
                if (targetUnit != null && targetUnit.team != movingUnit.team &&
                    (originElevation == targetElevation || (i == 1 && Mathf.Abs(originElevation - targetElevation) <= meleeRange))
                    && movingUnit.LineOfSight(new Vector2Int(x, y), currentNodePosition))
                {
                    enemyGroundHexes.Add(currentNodePosition);
                    targetHexPositions.Add(currentNodePosition);
                }
            }
        }

        startingPosition = targetPosition;
        actionPointsLeft = numActionPoints;
        amountOfPossibleMoves = moveAmounts;
        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
        map.ResetMap(false, false);
        List<DijkstraMapNode> nodesInMovementRange = map.GetNodesInMovementRange(x, y, startValue, movingUnit.moveModifier, gameManager);
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
                        rangeBracketOfNode = -1;
                    }
                    else
                    {
                        tempNodeValue -= 1;
                        rangeBracketOfNode = tempNodeValue / (movingUnit.moveSpeedPerMoveAction);
                    }
                }
                else
                {
                    nodeValue -= 1;
                    rangeBracketOfNode = (nodeValue / (movingUnit.moveSpeedPerMoveAction));
                }
                rangeBracketOfNode += 2 - actionPointsLeft;
                Unit unitOnHex = gameManager.grid.GetGridObject(currentNodePosition).unit;
                if (unitOnHex == null)
                {
                    groundColorValues.Add(rangeBracketOfNode);
                    groundHexes.Add(currentNodePosition);
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
        for (int i = 0; i < groundColorValues.Count; i++)
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
                bool foundEndPosition = false;

                moveSpeedUsed = 0;

                //Target Unit
                Unit targetUnit = null;
                if (gameManager.grid.GetGridObject(endX, endY) != null)
                {
                    targetUnit = gameManager.grid.GetGridObject(endX, endY).unit;
                }
                if (canMove)
                {
                    EnoughActionPointsForMeleeActionOnly = false;

                    if (enemyGroundHexes.Contains(new Vector2Int(endX, endY)))
                    {
                        Debug.Log("Found End Position");
                        foundEndPosition = true;
                        selectOnTarget = true;
                    }
                    else
                    {
                        int startx = x;
                        int starty = y;
                        selectOnTarget = false;
                        int currentMoveSpeed = moveSpeedInitiallyAvailable + movingUnit.moveSpeedPerMoveAction * amountOfPossibleMoves;
                        int previousNodeMoveValue = map.getGrid().GetGridObject(x, y).value;
                        // loop through possible move amounts to find a path to end hex
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

                            if (endHex[0] == path[path.Count - 1])
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

                                if (endHex[0] == path[path.Count - 1])
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
        if (movingUnit.moveModifier.NewValidMeleeAttack(gameManager, map.getGrid().GetGridObject(startingPosition), map.getGrid().GetGridObject(endHexPosition), meleeRange))
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
                        prevEndHexPosition = endHexPosition;
                        for (int i = 0; i < path.Count; i++)
                        {
                            setPath.Add(path[i]);
                        }
                    }
                }
                // If not able to move then only check if Mouse Path is in melee Range
                else if (targetInRange && targetUnit != null)
                {
                    prevEndHexPosition = endHexPosition;
                }

                Vector2Int lineOfSightStartHex = startingPosition;
                if (setPath.Count > 0)
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
                    if (targetInRange)
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
                        moveSpeedLeft += movementActionsTaken * movingUnit.moveSpeedPerMoveAction;
                        actionPointsLeft -= movementActionsTaken;
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
                                gameManager.move.AnotherActionMove(setPath, movingUnit, false);
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
                    moveSpeedLeft += movementActionsTaken * movingUnit.moveSpeedPerMoveAction;
                    actionPointsLeft -= movementActionsTaken;

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
                                gameManager.move.AnotherActionMove(setPath, movingUnit, true);
                                Destroy(tempMovingUnit);
                            }
                        },
                        () => // Cancel Action
                        {
                            SetUp(new Vector2Int(movingUnit.x, movingUnit.y), movingUnit.currentMajorActionsPoints, movingUnit.currentMoveSpeed);
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
        actionLines = new List<List<Vector3>>();
        actionPointsLeft = movingUnit.currentMajorActionsPoints;
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
        Vector2Int endHex;

        if (EnoughActionPointsForMeleeActionOnly || selectOnTarget || path.Count == 0)
        {
            int x = startingPosition.x;
            int y = startingPosition.y;
            endHex = new Vector2Int(x, y);
        }
        else
        {
            endHex = path[path.Count - 1];
        }

        for (int i = 0; i < rangeHexes.Count; i++)
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

        for (int i = 1; i <= meleeRange; i++)
        {
            List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(endHex.x, endHex.y, i);
            for (int j = 0; j < mapNodes.Count; j++)
            {
                if (movingUnit.moveModifier.NewValidMeleeAttack(gameManager, map.getGrid().GetGridObject(endHex), mapNodes[j], meleeRange))
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
