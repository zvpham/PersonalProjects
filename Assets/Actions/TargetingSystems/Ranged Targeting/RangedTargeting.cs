using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static SpriteManager;

public class RangedTargeting : TargetingSystem
{
    public DijkstraMap map;
    public CombatGameManager gameManager;
    public Unit movingUnit;
    public Action action;
    public Vector2Int startingPosition;
    public Vector2Int currentlySelectedHex;
    // For End Targeting/ Mouse Up
    public Vector2Int prevEndHexPosition;
    // For Selection/ Mouse Hover (Combat UI)
    public Vector2Int prevEndHexSelectedPosition;

    public List<Vector2Int> friendlyUnits;
    public List<Vector2Int> validTargetPositions;
    public List<Vector2Int> path = new List<Vector2Int>();
    public List<Vector2Int> setPath = new List<Vector2Int>();
    public List<Vector2Int> effectiveTargetHexPositions = new List<Vector2Int>();
    public List<Vector2Int> maxRangeTargetHexPositions = new List<Vector2Int>();
    public List<Vector2Int> highlightedCoverHexes = new List<Vector2Int>();
    public List<Unit> targets = new List<Unit>();

    public List<PassiveEffectArea>[,] passives;
    List<PassiveEffectArea> passiveEffectAreas = new List<PassiveEffectArea>();
    List<Tuple<Passive, Vector2Int>> passiveSprites = new List<Tuple<Passive, Vector2Int>>();
    public bool[,] unwalkablePassivesValues;
    public bool[,] badWalkInPassivesValues;
    public bool[,] goodWalkinPassivesValues;

    public List<Vector2Int> enemyGroundHexes;
    public List<Vector2Int> groundHexes;
    public List<int> groundColorValues;
    public List<GameObject> highlightedTargetedHexes = new List<GameObject>();
    public List<GameObject> highlightedHexes = new List<GameObject>();
    public List<GameObject> effectiveTargetHexes = new List<GameObject>();
    public List<GameObject> maxRangeTargetHexes = new List<GameObject>();
    public List<GameObject> coverHexes = new List<GameObject>();
    public List<SpriteHolder> targetingPassiveSpriteHolder;
    public GameObject tempMovingUnit;
    //public AttackData attackData;
    public List<EquipableAmmoSO> unitAmmo;
    public int currentAmmoIndex;

    public int numTargets = 1;
    public int effectiveRange;
    public int maxRange;
    public int actionPointUseAmount;
    public int amountOfPossibleMoves;
    public int actionPointsLeft;
    public int moveSpeedUsed = 0;
    public int moveSpeedInitiallyAvailable = 0;
    List<List<Vector3>> actionLines = new List<List<Vector3>>();

    public bool EnoughActionPointsForMeleeActionOnly;
    public bool selectedTarget = false;
    public bool targetFriendly = false;
    public bool canSelectSameTarget = false;
    public bool doesDamage = false;
    public bool canMove;
    public bool keepCombatAttackUi = false;
    public bool ChangeCombatAttackUI = false;
    public bool selectOnTarget = false;
    public bool unitAttemptingToMove = false;

    AttackData attackData;

    //MovingUnit, TargetUnit, FoundTarget, CurrentAmmoIndex
    public UnityAction<Unit, List<Unit>, bool, int> OnFoundTarget;
    public delegate bool IsValidtarget(Unit target);

    public void SetParameters(RangedTargetingData rangedTargetingData)
    {
        this.targetFriendly = rangedTargetingData.targetFriendly;
        this.movingUnit = rangedTargetingData.movingUnit;
        this.gameManager = rangedTargetingData.movingUnit.gameManager;
        this.actionPointsLeft = rangedTargetingData.actionPointsLeft;
        this.actionPointUseAmount = rangedTargetingData.actionPointUseAmount;
        this.effectiveRange = rangedTargetingData.effectiveRange;
        this.numTargets = rangedTargetingData.numTargets;
        this.maxRange = rangedTargetingData.maxRange;
        this.doesDamage = rangedTargetingData.doesDamage;
        Debug.Log("Moving Unit: " + movingUnit.currentMajorActionsPoints + ", " + actionPointsLeft);

        if (effectiveRange > maxRange)
        {
            this.maxRange = effectiveRange;
        }

        this.attackData = rangedTargetingData.attackData;


        int damage = 0;
        for (int i = 0; i < attackData.allDamage.Count; i++)
        {
            damage += attackData.allDamage[i].maxDamage;
        }
        Debug.Log("Start Test Damage: " + damage  + ", Damage Count: " + attackData.allDamage.Count);

        map = movingUnit.gameManager.map;
        startingPosition = new Vector2Int(movingUnit.x, movingUnit.y);
        selectedTarget = false;
        unitAttemptingToMove = false;
        prevEndHexPosition = new Vector2Int(-1, -1);
        path = new List<Vector2Int>();
        this.enabled = true;

        this.unitAmmo = rangedTargetingData.unitAmmo;
        if (unitAmmo == null || unitAmmo.Count < 0)
        {
            currentAmmoIndex = -1;
        }
        else
        {
            currentAmmoIndex = 0;
        }

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
        Debug.Log("Setup");
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

        for (int i = 1; i <= maxRange; i++)
        {
            List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(x, y, i);
            for (int j = 0; j < mapNodes.Count; j++)
            {
                Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                Unit targetUnit = gameManager.grid.GetGridObject(currentNodePosition.x, currentNodePosition.y).unit;
                int targetElevation = gameManager.spriteManager.elevationOfHexes[mapNodes[j].x, mapNodes[j].y];
                if (targetUnit != null && validTargets.Contains(targetUnit))
                {
                    enemyGroundHexes.Add(currentNodePosition);
                    effectiveTargetHexPositions.Add(currentNodePosition);
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

        for (int i = 0; i < effectiveTargetHexes.Count; i++)
        {
            gameManager.spriteManager.DisableTargetHex(effectiveTargetHexes[i]);
        }
        effectiveTargetHexes.Clear();
        effectiveTargetHexPositions.Clear();

        for (int i = 0; i < maxRangeTargetHexes.Count; i++)
        {
            gameManager.spriteManager.DisableMaxRangeTargetHex(maxRangeTargetHexes[i]);
        }
        maxRangeTargetHexes.Clear();
        maxRangeTargetHexPositions.Clear();
    }

    //Mouse Hover
    public override void SelectNewPosition(Vector2Int newHex)
    {
        if (!selectedTarget)
        {   
            currentlySelectedHex = newHex;
            map.ResetMap();
            List<Vector2Int> endHex = new List<Vector2Int>();
            int endX = currentlySelectedHex.x;
            int endY = currentlySelectedHex.y;
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
                        Debug.Log("Target Hex HAs Enemy");
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
                    map.SetGoalsNew(endHex, gameManager, movingUnit.moveModifier);
                    path.Clear();
                    x = startingPosition.x;
                    y = startingPosition.y;
                    path.Add(new Vector2Int(x, y));
                }
                else
                {
                    //Debug.Log("this SHouldn't happen");
                }

                //Create Combat Attack UI and Cover
                if (ChangeCombatAttackUI || (prevEndHexSelectedPosition != endHex[0] && !keepCombatAttackUi))
                {
                    ChangeCombatAttackUI = false;
                    for (int i = 0; i < coverHexes.Count; i++)
                    {
                        gameManager.spriteManager.DisableHighlightedHex(coverHexes[i]);
                    }
                    coverHexes = new List<GameObject>();

                    gameManager.spriteManager.ResetCombatAttackUI();
                    prevEndHexSelectedPosition = endHex[0];
                    if (targetUnit != null)
                    {
                        if (path.Count > 0)
                        {
                            highlightedCoverHexes = GetSpacesThatAreCover(path[0], endHex[0]);
                        }
                        else
                        {
                            highlightedCoverHexes = GetSpacesThatAreCover(startingPosition, endHex[0]);
                        }

                        if (effectiveTargetHexPositions.Contains(endHex[0]))
                        {
                            AttackData currentAttackData = new AttackData(attackData);

                            if (unitAmmo != null && unitAmmo.Count > 0)
                            {
                                unitAmmo[currentAmmoIndex].ModifyAttack(currentAttackData);
                            }

                            List<AttackDataUI> attackDatas = currentAttackData.CalculateAttackData(targetUnit, doesDamage);
                            Debug.Log("amount Of AttackDatas: " + attackDatas.Count);
                            if (highlightedCoverHexes.Count > 0 && doesDamage)
                            {
                                for (int j = 0; j < attackDatas.Count; j++)
                                {
                                    if (attackDatas[j].attackDataType == attackDataType.Main)
                                    {
                                        AttackDataUI attackData = attackDatas[j];
                                        attackData.min = attackData.min / 2;
                                        attackData.max = attackData.max / 2;
                                        attackData.data = attackData.min + " - " + attackData.max;
                                    }
                                }
                                AttackDataUI coverAttackData = new AttackDataUI();
                                coverAttackData.attackDataType = attackDataType.Modifier;
                                coverAttackData.attackState = attackState.Benediction;
                                coverAttackData.data = "Cover Penalty - 50%";
                                attackDatas.Add(coverAttackData);

                                for (int i = 0; i < highlightedCoverHexes.Count; i++)
                                {
                                    GameObject newHighlightedHex = gameManager.spriteManager.UseOpenHighlightedHex();
                                    Vector2Int currentNodePosition = highlightedCoverHexes[i];
                                    newHighlightedHex.transform.position = gameManager.spriteManager.GetWorldPosition(currentNodePosition);
                                    newHighlightedHex.GetComponent<SpriteRenderer>().color = gameManager.resourceManager.highlightedHexColors[6];
                                    newHighlightedHex.GetComponent<SpriteRenderer>().sortingOrder = gameManager.spriteManager.terrain[currentNodePosition.x,
                                        currentNodePosition.y].sprite.sortingOrder + 2;
                                    coverHexes.Add(newHighlightedHex);
                                }
                            }
                            
                           
                            gameManager.spriteManager.ActivateCombatAttackUI(targetUnit, attackDatas, targetUnit.transform.position);
                        }
                    }
                }
                DrawLine();
            }
        }
    }

    public override void EndTargeting()
    {
        int x = currentlySelectedHex.x;
        int y = currentlySelectedHex.y;

        if (x >= gameManager.grid.GetWidth() || x < 0 || y >= gameManager.grid.GetHeight() || y < 0)
        {
            return;
        }

        bool targetInRange = false;
        Unit targetUnit = gameManager.grid.GetGridObject(currentlySelectedHex.x, currentlySelectedHex.y).unit;
        map.ResetMap(true, false);
        map.SetGoalForNodesInTargetRange(new List<Vector2Int>() { startingPosition }, maxRange);
        int startingPositionDistanceFromMouse = map.getGrid().GetGridObject(currentlySelectedHex).value;
        if (startingPositionDistanceFromMouse >= 0)
        {
            targetInRange = true;
        }

        //Debug.Log("Target In Range: " +  targetInRange + ", " +  startingPosition + ", " +  maxRange + ", " + currentlySelectedHex + ", " + startingPositionDistanceFromMouse);
        // Case -  Player Clicks on previously selected Hex
        // Confirms action
        if (prevEndHexPosition.x >= 0 && currentlySelectedHex == prevEndHexPosition && (targetUnit == null || targets.Count >= numTargets))
        {
            gameManager.spriteManager.ConfirmAction();
        }
        //Case - Player Did not Click on previously selected Hex
        else
        {
            Vector2Int endPathHex;
            prevEndHexPosition = currentlySelectedHex;

            // if still able to move set PRevious Path Normally
            if (canMove)
            {
                if (actionPointsLeft > 0)
                {
                    for (int i = 0; i < path.Count; i++)
                    {
                        setPath.Add(path[i]);
                    }
                }
            }

            // Select a Player Controlled Unit when you aren't using a friendly ability like heal
            if (!targetFriendly && gameManager.playerTurn.CheckSpaceForFriendlyUnit(currentlySelectedHex))
            {
                OnFoundTarget?.Invoke(movingUnit, null, false, currentAmmoIndex);
                gameManager.playerTurn.SelectUnit(currentlySelectedHex);
            }
            //Select Valid Target
            else if (targetUnit != null && targetInRange && (canSelectSameTarget || !targets.Contains(targetUnit)))
            {
                Debug.Log("Selected VAlid taret");
                // check to see if max targets have been reached if it has do nothing
                if (targets.Count < numTargets)
                {
                    actionPointsLeft -= actionPointUseAmount;
                    int moveSpeedLeft = moveSpeedInitiallyAvailable - moveSpeedUsed;
                    targets.Add(targetUnit);
                    keepCombatAttackUi = true;

                    if (!targetInRange)
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

                    // Case target is within Range Of StartingPosition
                    if (targetInRange && !unitAttemptingToMove)
                    {
                        Debug.Log("Target in Range of Starting Position Confirmed");
                        SetUp(startingPosition, 0, movingUnit.currentMoveSpeed);
                        gameManager.spriteManager.ActivateActionConfirmationMenu(
                            () => // Confirm Action
                            {
                                selectedTarget = true;
                                OnFoundTarget?.Invoke(movingUnit, targets, true, currentAmmoIndex);
                                Destroy(tempMovingUnit);
                            },
                            () => // Cancel Action
                            {
                                ResetTargeting();
                            });
                    }
                    // Case - Target Unit is in movement + Range
                    else
                    {
                        Debug.Log("Target in Range with move confirmed");
                        endPathHex = setPath[setPath.Count - 1];
                        Destroy(tempMovingUnit);
                        tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, 1, movingUnit.unitProfile);
                        SetUp(setPath[setPath.Count - 1], 0, moveSpeedLeft);
                        gameManager.spriteManager.ActivateActionConfirmationMenu(
                            () => // Confirm Action
                            {
                                selectedTarget = true;
                                gameManager.move.AnotherActionMove(setPath, movingUnit, false);
                                OnFoundTarget?.Invoke(movingUnit, targets, true, currentAmmoIndex);
                                Destroy(tempMovingUnit);
                            },
                            () => // Cancel Action
                            {
                                ResetTargeting();
                            });
                    }
                }
            }
            // Select Empty Tile
            else if (canMove)
            {
                Debug.Log("Moving confirmed");
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
                            OnFoundTarget?.Invoke(movingUnit, null, false, currentAmmoIndex);
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

    /*
    // On Mouse UP
    public override void EndTargeting()
    {
        int x = currentlySelectedHex.x;
        int y = currentlySelectedHex.y;

        if (x >= gameManager.grid.GetWidth() || x < 0 || y >= gameManager.grid.GetHeight() || y < 0)
        {
            return;
        }

        bool targetInRange = false;
        map.ResetMap(true, false);
        map.SetGoalForNodesInTargetRange(new List<Vector2Int>() { startingPosition }, maxRange);
        int startingPositionDistanceFromMouse = map.getGrid().GetGridObject(currentlySelectedHex).value;
        if (startingPositionDistanceFromMouse <= maxRange)
        {
            targetInRange = true;
        }

        // Case -  Player Clicks on previously selected Hex
        // Confirms action
        if (prevEndHexPosition.x >= 0 && currentlySelectedHex == prevEndHexPosition)
        {
            gameManager.spriteManager.ConfirmAction();
        }
        //Case - Player clicks on hex that has not been previously selected
        else
        {
            Vector2Int endPathHex;
            // Select a Player Controlled Unit when you aren't using a friendly ability like heal
            if (!targetFriendly && gameManager.playerTurn.CheckSpaceForFriendlyUnit(currentlySelectedHex))
            {
                OnFoundTarget?.Invoke(movingUnit, null, false, currentAmmoIndex);
                gameManager.playerTurn.SelectUnit(currentlySelectedHex);
            }
            else
            {
                Unit targetUnit = gameManager.grid.GetGridObject(currentlySelectedHex.x, currentlySelectedHex.y).unit;
                // if still able to move set PRevious Path Normally
                if (canMove)
                {
                    if (actionPointsLeft > 0)
                    {
                        prevEndHexPosition = currentlySelectedHex;
                        for (int i = 0; i < path.Count; i++)
                        {
                            setPath.Add(path[i]);
                        }
                    }
                }
                // If not able to move then only check if Mouse Path is in melee Range
                else if (startingPositionDistanceFromMouse <= effectiveRange && targetUnit != null)
                {
                    prevEndHexPosition = currentlySelectedHex;
                }

                Vector2Int lineOfSightStartHex = startingPosition;
                if (setPath.Count > 0)
                {
                    lineOfSightStartHex = setPath[setPath.Count - 1];
                }

                //Case -  MouseHex contains a Unit that is a valid Target
                if (targetUnit != null && targetInRange)
                {
                    bool targetInMeleeRange = false;
                    actionPointsLeft -= actionPointUseAmount;
                    int moveSpeedLeft = moveSpeedInitiallyAvailable - moveSpeedUsed;
                    if (startingPositionDistanceFromMouse <= effectiveRange)
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
                                OnFoundTarget?.Invoke(movingUnit, new List<Unit>() { targetUnit }, true, currentAmmoIndex);
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
                                OnFoundTarget?.Invoke(movingUnit, new List<Unit>() { targetUnit}, true, currentAmmoIndex);
                                Destroy(tempMovingUnit);
                            },
                            () => // Cancel Action
                            {
                                ResetTargeting();
                            });
                    }
                }
                // Case - Mouse Hex doesn't contain a Unit and Moving Unit Can Still move
                else if (prevEndHexPosition == currentlySelectedHex && canMove)
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
                                OnFoundTarget?.Invoke(movingUnit, null, false, currentAmmoIndex);
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
    */

    public List<Vector2Int>  GetSpacesThatAreCover(Vector2Int shootingPosition, Vector2Int targetPosition)
    {
        List<Vector2Int> spacesThatAreCover = new List<Vector2Int>();
        List<Vector2Int> potentialSpacesThatAreCover = new List<Vector2Int>();
        Vector3 startPosition = map.getGrid().GetWorldPosition(shootingPosition);
        Vector3 endPosition = map.getGrid().GetWorldPosition(targetPosition);
        float angle = (Mathf.Atan2(endPosition.y - startPosition.y, endPosition.x - startPosition.x) * Mathf.Rad2Deg);
        int adjustedAngle = Mathf.RoundToInt(angle);

        List<int> potentialCoverAngles =  new List<int>();
        map.getGrid().GetXY(endPosition, out int x, out int y);
        List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(x, y, 1);
        for (int k = 0; k < mapNodes.Count; k++)
        {
            Vector2Int currentNodePosition = new Vector2Int(mapNodes[k].x, mapNodes[k].y);
            GridPosition gridPosition = gameManager.grid.GetGridObject(currentNodePosition);
            if (gameManager.spriteManager.elevationOfHexes[currentNodePosition.x, currentNodePosition.y] > gameManager.spriteManager.elevationOfHexes[targetPosition.x, targetPosition.y] ||(gridPosition != null && gridPosition.unit != null))
            {
                Vector3 nodePosition = map.getGrid().GetWorldPosition(currentNodePosition);
                angle = (Mathf.Atan2(endPosition.y - nodePosition.y, endPosition.x - nodePosition.x) * Mathf.Rad2Deg);
                potentialCoverAngles.Add(Mathf.RoundToInt(angle));
                potentialSpacesThatAreCover.Add(currentNodePosition);
            }
        }

        for (int i = 0; i < potentialCoverAngles.Count; i++)
        {
            if(adjustedAngle == potentialCoverAngles[i])
            {
                spacesThatAreCover.Add(potentialSpacesThatAreCover[i]);
                break;
            }
            else if(Mathf.Abs(Mathf.DeltaAngle(adjustedAngle, potentialCoverAngles[i])) < 60)
            {
                spacesThatAreCover.Add((potentialSpacesThatAreCover[i]));
            }
        }
        return spacesThatAreCover;
    }

    public void ResetTargeting()
    {
        Debug.Log("Reset");
        Destroy(tempMovingUnit);
        tempMovingUnit = null;
        selectedTarget = false;
        keepCombatAttackUi = false;
        unitAttemptingToMove = false;
        prevEndHexPosition = new Vector2Int(-1, -1);
        path.Clear();
        setPath.Clear();
        actionLines.Clear();
        actionPointsLeft = movingUnit.currentMajorActionsPoints;
        targets.Clear();
        //amountMoved = movingUnit.actions[0].amountUsedDuringRound;
        gameManager.spriteManager.ResetCombatAttackUI();
        gameManager.spriteManager.ClearLines();
        SetUp(new Vector2Int(movingUnit.x, movingUnit.y), actionPointsLeft, movingUnit.moveSpeed);
    }

    public override void DeactivateTargetingSystem()
    {
        Debug.Log("Deactivate Ranged Targeting System");
        ResetTargeting();
        ResetSetUp();
        for (int i = 0; i < coverHexes.Count; i++)
        {
            gameManager.spriteManager.DisableHighlightedHex(coverHexes[i]);
        }
        coverHexes = new List<GameObject>();
        map.ResetMap(true);
        OnFoundTarget = null;
        movingUnit.midAction = false;
    }

    public void DrawLine()
    {
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

        for (int i = 0; i < effectiveTargetHexes.Count; i++)
        {
            gameManager.spriteManager.DisableTargetHex(effectiveTargetHexes[i]);
        }
        effectiveTargetHexes.Clear();

        for (int i = 0; i < maxRangeTargetHexes.Count; i++)
        {
            gameManager.spriteManager.DisableMaxRangeTargetHex(maxRangeTargetHexes[i]);
        }
        maxRangeTargetHexes.Clear();

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

        for (int i = 1; i <= effectiveRange; i++)
        {
            List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(endHex.x, endHex.y, i);
            for (int j = 0; j < mapNodes.Count; j++)
            {
                Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                GameObject newTargetHex = gameManager.spriteManager.UseOpenTargetHex();
                newTargetHex.transform.position = gameManager.spriteManager.GetWorldPosition(currentNodePosition);
                newTargetHex.GetComponent<SpriteRenderer>().sortingOrder = gameManager.spriteManager.terrain[currentNodePosition.x,
                    currentNodePosition.y].sprite.sortingOrder + 2;
                effectiveTargetHexes.Add(newTargetHex);
            }
        }

        if(maxRange > effectiveRange)
        {
            for (int i = effectiveRange + 1; i <= maxRange; i++)
            {
                List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(endHex.x, endHex.y, i);
                for (int j = 0; j < mapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                    GameObject newTargetHex = gameManager.spriteManager.UseOpenMaxRangeTargetHex();
                    newTargetHex.transform.position = gameManager.spriteManager.GetWorldPosition(currentNodePosition);
                    newTargetHex.GetComponent<SpriteRenderer>().sortingOrder = gameManager.spriteManager.terrain[currentNodePosition.x,
                        currentNodePosition.y].sprite.sortingOrder + 2;
                    maxRangeTargetHexes.Add(newTargetHex);
                }
            }
        }

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
        if(currentAmmoIndex == -1)
        {
            return;
        }

        for(int i = 0; i < unitAmmo.Count; i++)
        {
            currentAmmoIndex += 1;
            if(currentAmmoIndex >= unitAmmo.Count)
            {
                currentAmmoIndex = 0;
            }
            if (unitAmmo[currentAmmoIndex] != null)
            {
                ChangeCombatAttackUI = true;
                SelectNewPosition(currentlySelectedHex);
                return;
            }
        }

    }

    public override void PreviousItem()
    {
        if (currentAmmoIndex == -1)
        {
            return;
        }

        for (int i = 0; i < unitAmmo.Count; i++)
        {
            currentAmmoIndex -= 1;
            if (currentAmmoIndex <= -1)
            {
                currentAmmoIndex = unitAmmo.Count - 1;
            }
            if (unitAmmo[currentAmmoIndex] != null)
            {
                ChangeCombatAttackUI = true;
                SelectNewPosition(currentlySelectedHex);
                return;
            }
        }
    }
}
