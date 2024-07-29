using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class MeleeTargeting : TargetingSystem
{
    public DijkstraMap map;
    public CombatGameManager gameManager;
    public Unit movingUnit;
    public Vector3 startingPosition;
    public Vector3 endPosition;
    // For End Targeting/ Mouse Up
    public Vector2Int prevEndHexPosition;    
    // For Selection/ Mouse Hover (Combat UI)
    public Vector2Int prevEndHexSelectedPosition;
    public List<Vector2Int> path = new List<Vector2Int>();
    public List<Vector2Int> setPath = new List<Vector2Int>();
    public List<Vector2Int> highlightedHexes = new List<Vector2Int>();
    public List<Vector2Int> highlightedPositions = new List<Vector2Int>();
    public List<Vector2Int> highlightedTargetedPositions = new List<Vector2Int>();
    public GameObject tempMovingUnit;
    public int oneActionMoveAmount;
    public int twoActionMoveAmount;
    public int meleeRange;
    public int actionPointUseAmount;
    public int amountMoved = 0;
    public int possibleMoveAmounts;

    List<List<Vector2Int>> rangeBrackets = new List<List<Vector2Int>>();
    public List<Vector2Int> firstRangeBracket = new List<Vector2Int>();
    public List<Vector2Int> secondRangeBracket = new List<Vector2Int>();

    public bool EnoughActionPointsForMeleeActionOnly;
    public bool selectedTarget = false;
    public bool targetFriendly = false;
    public bool canStillMove;

    public int actionPointsLeft;

    public delegate List<AttackDataUI> CalculateAttackData(Unit targetUnit, List<Vector2Int> movementPath);
    public CalculateAttackData calculateAttackData;

    //Path, MovingUnit, TargetUnit, FoundTarget
    public UnityAction<List<Vector2Int>, Unit, Unit, bool> OnFoundTarget;

    public void SetParameters(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int actionPointUseAmount, int meleeRange,
        CalculateAttackData calculateAttackData)
    {
        this.targetFriendly = targetFriendly;
        this.movingUnit = movingUnit;
        this.gameManager = movingUnit.gameManager;  
        this.actionPointsLeft = actionPointsLeft;
        this.actionPointUseAmount = actionPointUseAmount;
        this.meleeRange = meleeRange;
        this.calculateAttackData = calculateAttackData;
        map = movingUnit.gameManager.map;
        startingPosition = movingUnit.transform.position;
        selectedTarget = false;
        prevEndHexPosition = new Vector2Int(-10, 0);
        path = new List<Vector2Int>();
        this.enabled = true;
        amountMoved = movingUnit.amountMoveUsedDuringRound;

        SetUp(startingPosition, actionPointsLeft, movingUnit.moveSpeed);
    }

    public void SetUp(Vector3 targetPosition, int numActionPoints, int moveSpeed)
    {
        ResetSetUp();
        Debug.Log("Set UP");
        DijkstraMap map = gameManager.map;
        map.getGrid().GetXY(targetPosition, out int x, out int y);
        map.ResetMap(true);
        map.SetGoals(new List<Vector2Int>() { new Vector2Int(x, y) });
        startingPosition = targetPosition;
        firstRangeBracket.Clear();
        secondRangeBracket.Clear();

        int initialActionPoints = numActionPoints;
        initialActionPoints -= actionPointUseAmount;
        int moveAmounts = 0;
        while (initialActionPoints > 0)
        {
            if (initialActionPoints >= actionPointUseAmount + movingUnit.amountMoveUsedDuringRound + moveAmounts + 1)
            {
                moveAmounts += 1;
                initialActionPoints -= movingUnit.amountMoveUsedDuringRound + moveAmounts;
            }
            else
            {
                break;
            }
        }
        possibleMoveAmounts = moveAmounts;

        List<DijkstraMapNode> mapNodes = new List<DijkstraMapNode>();
        List<DijkstraMapNode> unresolvedMapNodes = new List<DijkstraMapNode>();
        rangeBrackets = new List<List<Vector2Int>>();

        int currentRadius = 1;
        if(moveAmounts > 0)
        {
            canStillMove = true;
            for (int i = 1; i <= moveAmounts; i++)
            {

                rangeBrackets.Add(new List<Vector2Int>());
                for (int j = 0; j < unresolvedMapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(unresolvedMapNodes[j].x, unresolvedMapNodes[j].y);
                    if (map.getGrid().GetGridObject(unresolvedMapNodes[j].x, unresolvedMapNodes[j].y).value <= moveSpeed * i)
                    {
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, i + 1);
                        rangeBrackets[i - 1].Add(currentNodePosition);
                        highlightedPositions.Add(currentNodePosition);
                        unresolvedMapNodes.RemoveAt(j);
                        j--;
                    }
                }

                for (int j = currentRadius; j <= meleeRange + moveSpeed * i; j++)
                {
                    mapNodes = map.getGrid().GetGridObjectsInRing(x, y, j);
                    for (int k = 0; k < mapNodes.Count; k++)
                    {
                        Vector2Int currentNodePosition = new Vector2Int(mapNodes[k].x, mapNodes[k].y);
                        if (map.getGrid().GetGridObject(mapNodes[k].x, mapNodes[k].y).value <= moveSpeed * i)
                        {
                            gameManager.spriteManager.ChangeTile(currentNodePosition, 1, i + 1);
                            rangeBrackets[i - 1].Add(currentNodePosition);
                            firstRangeBracket.Add(currentNodePosition);
                            highlightedPositions.Add(currentNodePosition);
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
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 5);
                        highlightedTargetedPositions.Add(currentNodePosition);
                    }
                }
            }
        }

        if (moveAmounts == 2)
        {
            canStillMove = true;
            for (int i = 1; i <= moveSpeed * 2 + meleeRange; i++)
            {
                mapNodes = map.getGrid().GetGridObjectsInRing(x, y, i);
                for (int j = 0; j < mapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                    if (map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= moveSpeed)
                    {
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 2);
                        firstRangeBracket.Add(currentNodePosition);
                        highlightedPositions.Add(currentNodePosition);
                    }
                    else if (map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= moveSpeed * 2)
                    {
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 4);
                        secondRangeBracket.Add(currentNodePosition);
                        highlightedPositions.Add(currentNodePosition);
                    }

                    Unit targetUnit = gameManager.grid.GetGridObject(currentNodePosition.x, currentNodePosition.y).unit;
                    if (targetUnit != null && targetUnit.team != movingUnit.team && map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= moveSpeed * 2 + meleeRange)
                    {
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 5);
                        highlightedTargetedPositions.Add(currentNodePosition);
                    }
                }
            }
        }
        else if (moveAmounts == 1)
        {
            canStillMove = true;
            for (int i = 1; i <= moveSpeed + meleeRange; i++)
            {
                mapNodes = map.getGrid().GetGridObjectsInRing(x, y, i);
                for (int j = 0; j < mapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                    if (map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= moveSpeed)
                    {
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 4);
                        firstRangeBracket.Add(currentNodePosition);
                        highlightedPositions.Add(currentNodePosition);
                    }

                    Unit targetUnit = gameManager.grid.GetGridObject(currentNodePosition.x, currentNodePosition.y).unit;
                    if (targetUnit != null && targetUnit.team != movingUnit.team && map.getGrid().GetGridObject(mapNodes[j].x, mapNodes[j].y).value <= moveSpeed + meleeRange)
                    {
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 5);
                        highlightedTargetedPositions.Add(currentNodePosition);
                    }
                }
            }
        }
        else if(moveAmounts == 0)
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
                        gameManager.spriteManager.ChangeTile(currentNodePosition, 1, 5);
                        highlightedTargetedPositions.Add(currentNodePosition);
                    }
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

    // Clear Highlighted Hexes
    public void ResetSetUp()
    {
        for (int i = 0; i < highlightedPositions.Count; i++)
        {
            gameManager.spriteManager.ChangeTile(highlightedPositions[i], 1, 0);
        }
        highlightedPositions = new List<Vector2Int>();

        for (int i = 0; i < highlightedTargetedPositions.Count; i++)
        {
            gameManager.spriteManager.ChangeTile(highlightedTargetedPositions[i], 1, 0);
        }

        for (int i = 0; i < highlightedHexes.Count; i++)
        {
            gameManager.spriteManager.ChangeTile(highlightedHexes[i], 2, 0);
        }

        highlightedTargetedPositions = new List<Vector2Int>();
    }

    //Mouse Hover
    public override void SelectNewPosition(Vector3 newPosition)
    {
        if (!selectedTarget)
        {
            endPosition = newPosition;
            map.ResetMap();
            List<Vector2Int> endHex = new List<Vector2Int>();
            map.getGrid().GetXY(newPosition, out int endX, out int endY);

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

            if (map.getGrid().GetGridObject(endX, endY) != null)
            {

                // (One Move Action) if there is enough action points to perform action and move draw line to end Hex
                if (actionPointsLeft >= actionPointUseAmount + amountMoved + meleeRange)
                {
                    EnoughActionPointsForMeleeActionOnly = false;
                    endHex.Add(new Vector2Int(endX, endY));
                    map.SetGoals(endHex);
                    map.getGrid().GetXY(startingPosition, out int x, out int y);
                    path.Clear();
                    oneActionMoveAmount = 0;
                    bool foundEndPosition = false;

                    // Attempt to Reach End Location and avoid hazards and walk on beneficial terrain
                    for (int i = 0; i < movingUnit.moveSpeed; i++)
                    {
                        DijkstraMapNode nextLowestNode = map.GetLowestNearbyNode(x, y);
                        x = nextLowestNode.x;
                        y = nextLowestNode.y;
                        path.Add(new Vector2Int(x, y));
                        oneActionMoveAmount++;
                        //End Path if End(Mouse) Hex is the same as the path or target enters meleeRange
                        if (endHex[0] == path[i] || (endHexIsValidTargetUnit && map.getGrid().GetGridObject(path[i]).value <= meleeRange))
                        {
                            foundEndPosition = true;
                            break;
                        }
                    }
                    
                    // Attempt to reach end location allowing player to walk on hazardous terrain
                    if (!foundEndPosition)
                    {
                        map.ResetMap();
                        map.SetGoals(endHex);
                        map.getGrid().GetXY(startingPosition, out x, out y);
                        path.Clear();
                        oneActionMoveAmount = 0;

                        for (int i = 0; i < movingUnit.moveSpeed; i++)
                        {
                            DijkstraMapNode nextLowestNode = map.GetLowestNearbyNode(x, y);
                            x = nextLowestNode.x;
                            y = nextLowestNode.y;
                            path.Add(new Vector2Int(x, y));
                            oneActionMoveAmount++;
                            //End Path if End(Mouse) Hex is the same as the path or target enters meleeRange
                            if (endHex[0] == path[i] || (endHexIsValidTargetUnit && map.getGrid().GetGridObject(path[i]).value <= meleeRange))
                            {
                                break;
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
                    map.getGrid().GetXY(startingPosition, out int x, out int y);
                    path.Clear();
                    oneActionMoveAmount = 0;
                    map.getGrid().GetXY(startingPosition, out x, out y);
                    path.Add(new Vector2Int(x, y));
                }
                else
                {
                    Debug.Log("this SHouldn't happen");
                }

                if(prevEndHexSelectedPosition != endHex[0])
                {
                    gameManager.spriteManager.ResetCombatAttackUI();
                    prevEndHexSelectedPosition = endHex[0];
                    if (targetUnit != null)
                    {
                        if (highlightedTargetedPositions.Contains(endHex[0]))
                        {
                            List<AttackDataUI> attackDatas = calculateAttackData(targetUnit, path);
                            gameManager.spriteManager.ActivateCombatAttackUI(targetUnit, attackDatas, targetUnit.transform.position);
                        }
                        else
                        {

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
        gameManager.grid.GetXY(endPosition, out int x, out int y);

        if (x >= gameManager.grid.GetWidth() || x < 0 || y >= gameManager.grid.GetHeight() || y < 0)
        {
            return;
        }

        Vector2Int endHexPosition = new Vector2Int(x, y);

        bool targetInRange =  false;
        int startingPositionDistanceFromMouse = map.getGrid().GetGridObject(startingPosition).value;
        if (startingPositionDistanceFromMouse <= meleeRange + movingUnit.moveSpeed * possibleMoveAmounts)
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
            // Case - Select Player Unit when you aren't using a friendly ability like heal
            if (!targetFriendly && gameManager.playerTurn.CheckSpaceForFriendlyUnit(endHexPosition))
            {
                OnFoundTarget?.Invoke(null, movingUnit, null, false);
                gameManager.playerTurn.SelectUnit(endHexPosition);
            }
            // Case - Select hex that doesn't have a player Unit or you are using an action that targets friendlies
            else
            {
                Vector2Int endPathHex = endHexPosition;
                Unit targetUnit = gameManager.grid.GetGridObject(endPathHex.x, endPathHex.y).unit;
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
                else if(startingPositionDistanceFromMouse <= meleeRange && targetUnit != null) 
                {
                    prevEndHexPosition = endHexPosition;
                }

                //Case -  MouseHex contains a Unit that is a valid Target
                if (targetUnit != null && targetInRange)
                {
                    bool targetInMeleeRange = false;
                    if(startingPositionDistanceFromMouse <= meleeRange)
                    {
                        targetInMeleeRange = true;
                        actionPointsLeft -= actionPointUseAmount;
                    }
                    else if (startingPositionDistanceFromMouse <= meleeRange + movingUnit.moveSpeed)
                    {
                        actionPointsLeft -= actionPointUseAmount;
                        actionPointsLeft -= amountMoved + 1;
                        amountMoved += 1;
                    }
                    else if(startingPositionDistanceFromMouse <= meleeRange + movingUnit.moveSpeed * 2)
                    {
                        actionPointsLeft -= actionPointUseAmount;
                        actionPointsLeft -= amountMoved + 1;
                        actionPointsLeft -= amountMoved + 2;
                        amountMoved += 2;
                    }

                    // Case target is within meleeRange
                    if (targetInMeleeRange)
                    {
                        SetUp(startingPosition, 0, movingUnit.moveSpeed);
                        gameManager.spriteManager.ActivateActionConfirmationMenu(
                            () => // Confirm Action
                            {
                                selectedTarget = true;
                                //gameManager.move.AnotherActionMove(setPath, movingUnit);
                                OnFoundTarget?.Invoke(setPath, movingUnit, targetUnit, true);
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
                        tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, 1, movingUnit.unitProfile);
                        SetUp(map.getGrid().GetWorldPosition(setPath[setPath.Count - 1]), 0, movingUnit.moveSpeed);
                        gameManager.spriteManager.ActivateActionConfirmationMenu(
                            () => // Confirm Action
                            {
                                selectedTarget = true;
                                //gameManager.move.AnotherActionMove(setPath, movingUnit);
                                OnFoundTarget?.Invoke(setPath, movingUnit, targetUnit, true);
                                Destroy(tempMovingUnit);
                            },
                            () => // Cancel Action
                            {
                                ResetTargeting();
                            });
                    }
                }
                // Case - Mouse Hex doesn't contain a Unit and Moving Unit Can Still move
                else if (prevEndHexPosition == endPathHex && canStillMove)
                {
                    if (firstRangeBracket.Contains(endHexPosition))
                    {
                        actionPointsLeft -= amountMoved + 1;
                        amountMoved += 1;
                    }
                    else if (secondRangeBracket.Contains(endHexPosition))
                    {
                        actionPointsLeft -= amountMoved + 1;
                        actionPointsLeft -= amountMoved + 2;
                        amountMoved += 2;
                    }

                    Destroy(tempMovingUnit);
                    tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, 1, movingUnit.unitProfile);
                    SetUp(map.getGrid().GetWorldPosition(setPath[setPath.Count - 1]), actionPointsLeft, movingUnit.moveSpeed);
                    gameManager.spriteManager.ActivateActionConfirmationMenu(
                        () => // Confirm Action
                        {
                            //gameManager.move.AnotherActionMove(setPath, amountMoved movingUnit);
                            Destroy(tempMovingUnit);
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

    public void ResetTargeting()
    {
        Debug.Log("Reset");
        Destroy(tempMovingUnit);
        tempMovingUnit = null;
        selectedTarget = false;
        prevEndHexPosition = new Vector2Int(-1, -1);
        path = new List<Vector2Int>();
        setPath = new List<Vector2Int>();
        
        gameManager.spriteManager.ClearLines();

        actionPointsLeft = movingUnit.currentActionsPoints;
        amountMoved = movingUnit.amountMoveUsedDuringRound;
        SetUp(movingUnit.transform.position, actionPointsLeft, movingUnit.moveSpeed);
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
        List<Vector3> oneActionLine = new List<Vector3>();
        List<Vector3> twoActionLine = new List<Vector3>();

        // For One Action Line
        // There is Already a set Path
        if (setPath.Count > 0)
        {
            oneActionLine = new List<Vector3>() { movingUnit.transform.position };
            for (int i = 0; i < setPath.Count; i++)
            {
                oneActionLine.Add(map.getGrid().GetWorldPosition(setPath[i].x, setPath[i].y));
            }
        }
        //No SetPath
        else
        {
            oneActionLine = new List<Vector3>() { movingUnit.transform.position };
            for (int i = 0; i < oneActionMoveAmount; i++)
            {
                oneActionLine.Add(map.getGrid().GetWorldPosition(path[i].x, path[i].y));
            }
        }

        //Two Action Line
        //If there is a setPath
        if (setPath.Count > 0)
        {
            twoActionLine.Add(oneActionLine[oneActionLine.Count - 1]);
            for (int i = 0; i < twoActionMoveAmount; i++)
            {
                twoActionLine.Add(map.getGrid().GetWorldPosition(path[i].x, path[i].y));
            }
        }
        // No Set Path
        else
        {
            twoActionLine.Add(oneActionLine[oneActionLine.Count - 1]);
            for (int i = 0; i < twoActionMoveAmount; i++)
            {
                twoActionLine.Add(map.getGrid().GetWorldPosition(path[i + oneActionMoveAmount].x, path[i + oneActionMoveAmount].y));
            }
        }

        Vector2Int endPositionHex = path[path.Count - 1];
        Unit targetUnit = gameManager.grid.GetGridObject(endPositionHex.x, endPositionHex.y).unit;
        if (targetUnit != null)
        {
            oneActionLine.RemoveAt(oneActionLine.Count - 1);
        }

        for (int i = 0; i < highlightedHexes.Count; i++)
        {
            gameManager.spriteManager.ChangeTile(highlightedHexes[i], 2, 0);
        }

        // Draw meleeRange
        Vector2Int endHex;

        if (EnoughActionPointsForMeleeActionOnly)
        {
            endHex = path[0];
        }
        else
        {
            endHex = path[path.Count - 1];
        }
        highlightedHexes.Clear();
        for (int i = 1; i <= meleeRange; i++)
        {
            List<DijkstraMapNode> mapNodes = map.getGrid().GetGridObjectsInRing(endHex.x, endHex.y, i);
            for (int j = 0; j < mapNodes.Count; j++)
            {
                Vector2Int currentNodePosition = new Vector2Int(mapNodes[j].x, mapNodes[j].y);
                gameManager.spriteManager.ChangeTile(currentNodePosition, 2, 3);
                highlightedHexes.Add(currentNodePosition);
            }
        }

        gameManager.spriteManager.DrawLine(oneActionLine, twoActionLine);
        
    }
}
