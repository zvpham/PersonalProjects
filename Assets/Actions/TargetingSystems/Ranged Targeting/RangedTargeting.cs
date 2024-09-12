using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class RangedTargeting : TargetingSystem
{
    public DijkstraMap map;
    public CombatGameManager gameManager;
    public Unit movingUnit;
    public Vector3 startingPosition;
    public Vector2Int currentlySelectedHex;
    // For End Targeting/ Mouse Up
    public Vector2Int prevEndHexPosition;
    // For Selection/ Mouse Hover (Combat UI)
    public Vector2Int prevEndHexSelectedPosition;

    public List<Vector2Int> path = new List<Vector2Int>();
    public List<Vector2Int> setPath = new List<Vector2Int>();
    public List<Vector2Int> targetHexPositions = new List<Vector2Int>();
        public List<Vector2Int> highlightedCoverHexes = new List<Vector2Int>();

    public List<Vector2Int> enemyGroundHexes;
    public List<Vector2Int> groundHexes;
    public List<int> groundColorValues;
    public List<GameObject> highlightedTargetedHexes = new List<GameObject>();
    public List<GameObject> highlightedHexes = new List<GameObject>();
    public List<GameObject> rangeHexes = new List<GameObject>();
    public List<GameObject> coverHexes = new List<GameObject>();
    public GameObject tempMovingUnit;
    public int oneActionMoveAmount;
    public int twoActionMoveAmount;
    public int meleeRange;
    public int actionPointUseAmount;
    public AttackData attackData;
    public List<EquipableAmmoSO> unitAmmo;
    public int currentAmmoIndex;

    public List<int> actionMoveAmounts;
    public int amountOfPossibleMoves;
    public int amountMoved = 0;

    public int amountActionLineIncreased = 0;
    public int IndexOfStartingActionLine = 0;
    List<List<Vector3>> actionLines = new List<List<Vector3>>();

    List<List<Vector2Int>> rangeBrackets = new List<List<Vector2Int>>();

    public bool EnoughActionPointsForMeleeActionOnly;
    public bool selectedTarget = false;
    public bool targetFriendly = false;
    public bool canStillMove;
    public bool keepCombatAttackUi = false;
    public bool ChangeCombatAttackUI = false;
    public bool selectOnTarget = false;
    public bool unitAttemptingToMove = false;

    public int actionPointsLeft;

    //Path, MovingUnit, TargetUnit, FoundTarget
    public UnityAction<Unit, Unit, bool> OnFoundTarget;

    public void SetParameters(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int actionPointUseAmount, int meleeRange,
        AttackData attackData, List<EquipableAmmoSO> unitAmmo)
    {
        this.targetFriendly = targetFriendly;
        this.movingUnit = movingUnit;
        this.gameManager = movingUnit.gameManager;
        this.actionPointsLeft = actionPointsLeft;
        this.actionPointUseAmount = actionPointUseAmount;
        this.meleeRange = meleeRange;
        this.attackData = attackData;
        this.unitAmmo = unitAmmo;
        if(unitAmmo.Count > 0 )
        {
            currentAmmoIndex = 0;
        }
        else
        {
            currentAmmoIndex = -1;
        }
        map = movingUnit.gameManager.map;
        startingPosition = movingUnit.transform.position;
        selectedTarget = false;
        unitAttemptingToMove = false;
        prevEndHexPosition = new Vector2Int(-1, -1);
        path = new List<Vector2Int>();
        this.enabled = true;
        amountMoved = movingUnit.amountMoveUsedDuringRound;

        SetUp(startingPosition, actionPointsLeft, movingUnit.moveSpeed);
    }

    public void SetUp(Vector3 targetPosition, int numActionPoints, int moveSpeed)
    {
        ResetSetUp();

        DijkstraMap map = gameManager.map;
        map.getGrid().GetXY(targetPosition, out int x, out int y);
        map.ResetMap(true);
        map.SetGoals(new List<Vector2Int>() { new Vector2Int(x, y) });
        startingPosition = targetPosition;

        actionPointsLeft = numActionPoints;

        int initialActionPoints = numActionPoints;
        initialActionPoints -= actionPointUseAmount;
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

        List<DijkstraMapNode> mapNodes;
        List<DijkstraMapNode> unresolvedMapNodes = new List<DijkstraMapNode>();
        rangeBrackets = new List<List<Vector2Int>>();
        groundHexes = new List<Vector2Int>();
        groundColorValues = new List<int>();
        enemyGroundHexes = new List<Vector2Int>();

        int meleeAdjustment = 0;
        amountOfPossibleMoves = moveAmounts;
        int currentRadius = 1;
        if (moveAmounts > 0)
        {
            canStillMove = true;
            for (int i = 1; i <= moveAmounts; i++)
            {
                if (i == moveAmounts)
                {
                    meleeAdjustment = meleeRange;
                }

                for (int j = currentRadius; j <= (moveSpeed * i) + meleeAdjustment; j++)
                {
                    mapNodes = map.getGrid().GetGridObjectsInRing(x, y, j);
                    for (int k = 0; k < mapNodes.Count; k++)
                    {
                        Vector2Int currentNodePosition = new Vector2Int(mapNodes[k].x, mapNodes[k].y);
                        Unit targetUnit = gameManager.grid.GetGridObject(currentNodePosition.x, currentNodePosition.y).unit;
                        if (targetUnit != null && targetUnit.team != movingUnit.team && map.getGrid().GetGridObject(mapNodes[k].x, mapNodes[k].y).value <=
                            moveSpeed * j + meleeRange)
                        {
                            enemyGroundHexes.Add(currentNodePosition);
                            unresolvedMapNodes.Remove(mapNodes[k]);
                            targetHexPositions.Add(currentNodePosition);
                        }
                    }
                    currentRadius += 1;
                }
            }

            map.ResetMap(true);
            currentRadius = 1;
            //Make Units Unwalkable
            for (int i = 0; i < gameManager.units.Count; i++)
            {
                if (gameManager.units[i].team != movingUnit.team)
                {
                    map.getGrid().GetXY(gameManager.units[i].transform.position, out int enemyX, out int enemyY);
                    map.SetUnwalkable(new Vector2Int(enemyX, enemyY));
                }
            }
            map.getGrid().GetXY(targetPosition, out x, out y);
            map.SetGoals(new List<Vector2Int>() { new Vector2Int(x, y) });

            for (int i = 1; i <= moveAmounts; i++)
            {
                rangeBrackets.Add(new List<Vector2Int>());
                for (int j = 0; j < unresolvedMapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(unresolvedMapNodes[j].x, unresolvedMapNodes[j].y);
                    if (map.getGrid().GetGridObject(unresolvedMapNodes[j].x, unresolvedMapNodes[j].y).value <= moveSpeed * i)
                    {
                        if (!enemyGroundHexes.Contains(currentNodePosition))
                        {
                            groundHexes.Add(currentNodePosition);
                            groundColorValues.Add(amountMoved + 1);
                        }
                        rangeBrackets[i - 1].Add(currentNodePosition);
                        unresolvedMapNodes.RemoveAt(j);
                        j--;
                    }
                }

                if (i == moveAmounts)
                {
                    meleeAdjustment = meleeRange;
                }

                for (int j = currentRadius; j <= (moveSpeed * i) + meleeAdjustment; j++)
                {
                    mapNodes = map.getGrid().GetGridObjectsInRing(x, y, j);
                    for (int k = 0; k < mapNodes.Count; k++)
                    {
                        Vector2Int currentNodePosition = new Vector2Int(mapNodes[k].x, mapNodes[k].y);
                        if (map.getGrid().GetGridObject(mapNodes[k].x, mapNodes[k].y).value <= moveSpeed * i)
                        {
                            if (!enemyGroundHexes.Contains(currentNodePosition))
                            {
                                groundHexes.Add(currentNodePosition);
                                groundColorValues.Add(amountMoved + 1);
                            }
                            rangeBrackets[i - 1].Add(currentNodePosition);
                        }
                        else
                        {
                            unresolvedMapNodes.Add(mapNodes[k]);
                        }
                    }
                    currentRadius += 1;
                }
            }
        }
        else
        {
            canStillMove = false;
            for (int i = 1; i <= meleeRange; i++)
            {
                mapNodes = map.getGrid().GetGridObjectsInRing(x, y, i);
                for (int j = 0; j < mapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                    Unit targetUnit = gameManager.grid.GetGridObject(currentNodePosition.x, currentNodePosition.y).unit;
                    if (targetUnit != null && targetUnit.team != movingUnit.team && map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= meleeRange)
                    {
                        if (groundHexes.Contains(currentNodePosition))
                        {
                            int groundIndex = groundHexes.IndexOf(currentNodePosition);
                            groundHexes.RemoveAt(groundIndex);
                            groundColorValues.RemoveAt(groundIndex);
                        }
                        enemyGroundHexes.Add(currentNodePosition);
                        targetHexPositions.Add(currentNodePosition);
                    }
                }
            }

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
            currentlySelectedHex = newHex;
            map.ResetMap();
            List<Vector2Int> endHex = new List<Vector2Int>();
            int endX = currentlySelectedHex.x;
            int endY = currentlySelectedHex.y;
            amountActionLineIncreased = 0;
            if (map.getGrid().GetGridObject(endX, endY) != null)
            {
                endHex.Add(new Vector2Int(endX, endY));
                map.SetGoals(endHex);
                map.getGrid().GetXY(startingPosition, out int x, out int y);
                path.Clear();
                bool foundEndPosition = false;
                bool findMostDirectPath = false;
                int initialAmountPathMoveIncreased = amountActionLineIncreased;
                selectOnTarget = false;

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

                if (amountOfPossibleMoves > 0)
                {
                    EnoughActionPointsForMeleeActionOnly = false;
                    
                    if (endHexIsValidTargetUnit && map.getGrid().GetGridObject(startingPosition).value <= meleeRange)
                    {
                        foundEndPosition = true;
                        selectOnTarget = true;
                    }
                    else
                    {
                        // loop through possible move amounts to find a path to end hex
                        for (int i = 0; i < amountOfPossibleMoves; i++)
                        {
                            actionMoveAmounts.Add(0);
                            amountActionLineIncreased += 1;
                            bool inRangeBracket = rangeBrackets[i].Contains(new Vector2Int(endX, endY));
                            int startx = x;
                            int starty = y;

                            // Attempt to find path Avoiding harmful Terrain
                            for (int j = 0; j < movingUnit.moveSpeed; j++)
                            {
                                DijkstraMapNode nextLowestNode = map.GetLowestNearbyNode(x, y);
                                x = nextLowestNode.x;
                                y = nextLowestNode.y;
                                path.Add(new Vector2Int(x, y));
                                actionMoveAmounts[i] = actionMoveAmounts[i] + 1;
                                
                                if (endHex[0] == path[path.Count - 1] || (endHexIsValidTargetUnit && map.getGrid().GetGridObject(path[i]).value <= meleeRange))
                                {
                                    foundEndPosition = true;
                                    break;
                                }
                                else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
                                {
                                    path.RemoveAt(path.Count - 1);
                                    foundEndPosition = false;
                                    actionMoveAmounts[i] = actionMoveAmounts[i] - 1;
                                    break;
                                }
                            }

                            if (foundEndPosition)
                            {
                                break;
                            }

                            // If it is possible to reach end position in range bracket Reset data and try again
                            if (inRangeBracket && !foundEndPosition)
                            {
                                // Reset Data From Initial attempt to find path
                                for (int j = 0; j < actionMoveAmounts[i]; j++)
                                {
                                    path.RemoveAt(path.Count - 1);
                                }
                                actionMoveAmounts[i] = 0;
                                x = startx;
                                y = starty;

                                //Try to find Path
                                for (int j = 0; j < movingUnit.moveSpeed; j++)
                                {
                                    DijkstraMapNode nextLowestNode = map.GetLowestNearbyNode(x, y);
                                    x = nextLowestNode.x;
                                    y = nextLowestNode.y;
                                    path.Add(new Vector2Int(x, y));
                                    actionMoveAmounts[i] = actionMoveAmounts[i] + 1;
                                    if (endHex[0] == path[path.Count - 1] || (endHexIsValidTargetUnit && map.getGrid().GetGridObject(path[i]).value <= meleeRange))
                                    {
                                        foundEndPosition = true;
                                        break;
                                    }
                                    else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
                                    {
                                        path.RemoveAt(path.Count - 1);
                                        foundEndPosition = false;
                                        actionMoveAmounts[i] = actionMoveAmounts[i] - 1;
                                        break;
                                    }
                                }

                                findMostDirectPath = !foundEndPosition;
                                break;
                            }
                        }
                        // Unable to find path in its rangebracket, redo entire path taking most direct route
                        if (findMostDirectPath)
                        {
                            for (int i = 0; i < amountActionLineIncreased; i++)
                            {
                                actionMoveAmounts.RemoveAt(actionMoveAmounts.Count - 1);
                            }
                            amountActionLineIncreased = initialAmountPathMoveIncreased;
                            endHex.Add(new Vector2Int(endX, endY));
                            map.SetGoals(endHex);
                            map.getGrid().GetXY(startingPosition, out x, out y);
                            path.Clear();
                            foundEndPosition = false;
                            for (int i = 0; i < amountOfPossibleMoves; i++)
                            {
                                amountActionLineIncreased += 1;
                                actionMoveAmounts.Add(0);
                                bool inRangeBracket = rangeBrackets[i].Contains(new Vector2Int(endX, endY));
                                int startx = x;
                                int starty = y;

                                // Attempt to find path Avoiding harmful Terrain
                                for (int j = 0; j < movingUnit.moveSpeed; j++)
                                {
                                    DijkstraMapNode nextLowestNode = map.GetLowestNearbyNode(x, y);
                                    x = nextLowestNode.x;
                                    y = nextLowestNode.y;
                                    path.Add(new Vector2Int(nextLowestNode.x, nextLowestNode.y));
                                    actionMoveAmounts[i] = actionMoveAmounts[i] + 1;
                                    if (endHex[0] == path[path.Count - 1] || (endHexIsValidTargetUnit && map.getGrid().GetGridObject(path[i]).value <= meleeRange))
                                    {
                                        foundEndPosition = true;
                                        break;
                                    }
                                    else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
                                    {
                                        path.RemoveAt(path.Count - 1);
                                        foundEndPosition = false;
                                        actionMoveAmounts[i] = actionMoveAmounts[i] - 1;
                                        break;
                                    }
                                }

                                if (foundEndPosition)
                                {
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
                    map.SetGoals(endHex);
                    map.getGrid().GetXY(startingPosition, out x, out y);
                    path.Clear();
                    oneActionMoveAmount = 0;
                    map.getGrid().GetXY(startingPosition, out x, out y);
                    path.Add(new Vector2Int(x, y));
                }
                else
                {
                    Debug.Log("this SHouldn't happen");
                }

                if((prevEndHexPosition.x <= -1 ||prevEndHexPosition.y <= -1) && prevEndHexSelectedPosition != endHex[0])
                {
                    for (int i = 0; i < coverHexes.Count; i++)
                    {
                        gameManager.spriteManager.DisableHighlightedHex(coverHexes[i]);
                    }
                    coverHexes = new List<GameObject>();
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
                        if(path.Count > 0)
                        {
                            highlightedCoverHexes = GetSpacesThatAreCover(path[0], endHex[0]);
                        }
                        else
                        {
                            map.getGrid().GetXY(startingPosition, out x, out y);
                            highlightedCoverHexes = GetSpacesThatAreCover(new Vector2Int(x, y), endHex[0]);
                        }

                        if (targetHexPositions.Contains(endHex[0]))
                        {
                            AttackData currentAttackData = new AttackData(attackData.minDamage, attackData.maxDamage, 
                                attackData.armorDamagePercentage, attackData.originUnit);
                            if(unitAmmo != null && unitAmmo.Count > 0)
                            {
                                currentAttackData = unitAmmo[currentAmmoIndex].ModifyAttack(currentAttackData);
                            }
                            List<AttackDataUI> attackDatas = currentAttackData.CalculateAttackData(targetUnit);
                            if(highlightedCoverHexes.Count > 0)
                            {
                                for(int j = 0; j < attackDatas.Count; j++)
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

                                for(int i = 0; i < highlightedCoverHexes.Count; i++)
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

    // On Mouse UP
    public override void EndTargeting()
    {
        int x = currentlySelectedHex.x;
        int y = currentlySelectedHex.y;

        if (x >= gameManager.grid.GetWidth() || x < 0 || y >= gameManager.grid.GetHeight() || y < 0)
        {
            return;
        }
        // Determine Latest Action Line
        for (int i = actionLines.Count - 1; i >= 0; i--)
        {
            if (actionLines[i].Count != 0)
            {
                IndexOfStartingActionLine = i + 1;
                break;
            }
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
                if (canStillMove)
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
                else if (startingPositionDistanceFromMouse <= meleeRange && targetUnit != null)
                {
                    prevEndHexPosition = endHexPosition;
                }
                //Case -  MouseHex contains a Unit that is a valid Target
                if (targetUnit != null && targetInRange)
                {
                    bool targetInMeleeRange = false;
                    if (startingPositionDistanceFromMouse <= meleeRange)
                    {
                        targetInMeleeRange = true;
                        actionPointsLeft -= actionPointUseAmount;
                    }
                    else
                    {
                        for (int i = 0; i <= rangeBrackets.Count; i++)
                        {
                            if (startingPositionDistanceFromMouse <= meleeRange + movingUnit.moveSpeed * i)
                            {
                                actionPointsLeft -= actionPointUseAmount;
                                for (int j = 0; j <= i; j++)
                                {
                                    actionPointsLeft -= amountMoved + (j + 1);
                                }
                                amountMoved += i;
                            }
                        }
                    }
                    keepCombatAttackUi = true;
                    // Case target is within Range and Hasn't attempted to move
                    if (targetInMeleeRange && !unitAttemptingToMove)
                    {
                        SetUp(startingPosition, 0, movingUnit.moveSpeed);
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
                        SetUp(map.getGrid().GetWorldPosition(setPath[setPath.Count - 1]), 0, movingUnit.moveSpeed);
                        gameManager.spriteManager.ActivateActionConfirmationMenu(
                            () => // Confirm Action
                            {
                                selectedTarget = true;
                                gameManager.move.AnotherActionMove(setPath, IndexOfStartingActionLine, movingUnit, false);
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
                else if (prevEndHexPosition == endHexPosition && canStillMove)
                {
                    endPathHex = setPath[setPath.Count - 1];
                    for (int i = 0; i < rangeBrackets.Count; i++)
                    {
                        if (rangeBrackets[i].Contains(endPathHex))
                        {
                            for (int j = 0; j <= i; j++)
                            {
                                actionPointsLeft -= amountMoved + (j + 1);
                            }
                            amountMoved += i + 1;
                        }
                    }
                    Destroy(tempMovingUnit);
                    tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, 1, movingUnit.unitProfile);
                    SetUp(map.getGrid().GetWorldPosition(setPath[setPath.Count - 1]), actionPointsLeft, movingUnit.moveSpeed);
                    unitAttemptingToMove = true;
                    gameManager.spriteManager.ActivateActionConfirmationMenu(
                        () => // Confirm Action
                        {
                            //gameManager.move.AnotherActionMove(setPath, amountMoved movingUnit);
                            if (setPath.Count == 1 && movingUnit.gameManager.map.getGrid().GetWorldPosition(setPath[0].x, setPath[0].y) == movingUnit.transform.position)
                            {
                                OnFoundTarget?.Invoke(movingUnit, null, false);
                                gameManager.playerTurn.SelectUnit(endHexPosition);
                            }
                            else
                            {
                                gameManager.move.AnotherActionMove(setPath, amountMoved, movingUnit, true);
                                Destroy(tempMovingUnit);
                            }
                        },
                        () => // Cancel Action
                        {
                            SetUp(movingUnit.transform.position, movingUnit.currentActionsPoints, movingUnit.moveSpeed);
                            ResetTargeting();
                        });
                }
            }
        }
    }

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
            if (gridPosition != null && gridPosition.unit != null)
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
        path = new List<Vector2Int>();
        setPath = new List<Vector2Int>();
        actionMoveAmounts = new List<int>();
        actionLines = new List<List<Vector3>>();
        amountActionLineIncreased = 0;
        IndexOfStartingActionLine = 0;
        actionPointsLeft = movingUnit.currentActionsPoints;
        amountMoved = movingUnit.amountMoveUsedDuringRound;
        gameManager.spriteManager.ResetCombatAttackUI();
        gameManager.spriteManager.ClearLines();
        SetUp(movingUnit.transform.position, actionPointsLeft, movingUnit.moveSpeed);
    }

    public override void DeactivateTargetingSystem()
    {
        ResetTargeting();
        ResetSetUp();
        for (int i = 0; i < coverHexes.Count; i++)
        {
            gameManager.spriteManager.DisableHighlightedHex(coverHexes[i]);
        }
        coverHexes = new List<GameObject>();
        map.ResetMap(true);
        OnFoundTarget = null;
    }

    public void DrawLine()
    {
        if (actionPointsLeft <= 0)
        {
            return;
        }

        if (IndexOfStartingActionLine > 0)
        {
            for (int i = 0; i < amountActionLineIncreased; i++)
            {
                if (i > IndexOfStartingActionLine)
                {
                    actionLines.RemoveAt(actionLines.Count - 1);
                }
            }
        }

        for (int i = IndexOfStartingActionLine; i < actionLines.Count; i++)
        {
            actionLines[i] = new List<Vector3>();
            gameManager.spriteManager.DrawLine(actionLines[i], i);
        }

        int currentPathindex = 0;
        for (int i = IndexOfStartingActionLine; i < IndexOfStartingActionLine + amountActionLineIncreased; i++)
        {
            if (actionLines.Count <= i)
            {
                actionLines.Add(new List<Vector3>());
            }

            if (i == 0)
            {
                actionLines[i].Add(movingUnit.transform.position);
            }
            else
            {
                actionLines[i].Add(actionLines[i - 1][actionLines[i - 1].Count - 1]);
            }

            for (int j = 0; j < actionMoveAmounts[i - IndexOfStartingActionLine]; j++)
            {
                actionLines[i].Add(map.getGrid().GetWorldPosition(path[currentPathindex].x, path[currentPathindex].y));
                currentPathindex += 1;
            }
            gameManager.spriteManager.DrawLine(actionLines[i], i);
        }

        for (int i = 0; i < amountActionLineIncreased; i++)
        {
            actionMoveAmounts.RemoveAt(actionMoveAmounts.Count - 1);
        }

        // Draw meleeRange
        Vector2Int endHex;

        if (EnoughActionPointsForMeleeActionOnly || selectOnTarget)
        {
            map.getGrid().GetXY(startingPosition, out int x, out int y);
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

        for (int i = 1; i <= meleeRange; i++)
        {
            List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(endHex.x, endHex.y, i);
            for (int j = 0; j < mapNodes.Count; j++)
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
