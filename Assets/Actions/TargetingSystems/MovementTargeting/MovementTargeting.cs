using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class MovementTargeting : TargetingSystem
{
    public DijkstraMap map;
    public CombatGameManager gameManager;
    public Unit movingUnit;
    public Vector3 startingPosition;
    public Vector2Int prevEndHexPosition;
    public Vector2Int currentlySelectedHex;
    public List<Vector2Int> path = new List<Vector2Int>();
    public List<Vector2Int> setPath = new List<Vector2Int>();

    public List<int> groundColorValues = new List<int>();
    public List<Vector2Int> groundHexes = new List<Vector2Int>();
    public List<GameObject> highlightedTargetedHexes = new List<GameObject>();

    public List<PassiveEffectArea>[,] passives;
    List<PassiveEffectArea> passiveEffectAreas = new List<PassiveEffectArea>();
    List<Tuple<Passive, Vector2Int>> passiveSprites = new List<Tuple<Passive, Vector2Int>>();
    public bool[,] unwalkablePassivesValues;
    public bool[,] badWalkInPassivesValues;
    public bool[,] goodWalkinPassivesValues;

    public GameObject tempMovingUnit;

    public int actionPointsLeft;
    public List<int> actionMoveAmounts;
    public int moveSpeedInitiallyAvailable = 0;
    public int moveSpeedUsed = 0;

    public int amountActionLineIncreased = 0;
    public int IndexOfStartingActionLine = 0;
    List<List<Vector3>> actionLines = new List<List<Vector3>>();

    public List<GameObject> highlightedHexes;
    public List<SpriteHolder> targetingPassiveSpriteHolder = new List<SpriteHolder>();

    public bool targetFriendly = false;
    public bool canMove = false;

    public UnityAction<List<Vector2Int>, List<int>,  Unit,  bool> OnFoundTarget;

    public void SetParameters(Unit movingUnit, bool targetFriendly, int actionPointsLeft)
    {
        this.targetFriendly = targetFriendly;
        this.movingUnit = movingUnit;
        this.gameManager = movingUnit.gameManager;
        this.actionPointsLeft = actionPointsLeft;
        map = movingUnit.gameManager.map;
        startingPosition = movingUnit.transform.position;
        prevEndHexPosition = new Vector2Int(-10, 0);
        path = new List<Vector2Int>();
        this.enabled = true;
        highlightedHexes = new List<GameObject>();
        SetUp(startingPosition, actionPointsLeft, movingUnit.currentMoveSpeed);

        passives = new List<PassiveEffectArea>[gameManager.mapSize, gameManager.mapSize];
        for(int i = 0; i < gameManager.mapSize; i++)
        {
            for(int j = 0; j < gameManager.mapSize; j++)
            {
                passives[i, j] = new List<PassiveEffectArea>();
            }
        }

        List<List<PassiveEffectArea>> classifiedPassiveEffectArea =  movingUnit.CalculuatePassiveAreas();
        unwalkablePassivesValues = new bool [gameManager.mapSize, gameManager.mapSize];

        for(int i = 0; i < classifiedPassiveEffectArea[0].Count; i++)
        {
            for(int j = 0; j < classifiedPassiveEffectArea[0][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[0][i].passiveLocations[j];
                unwalkablePassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[0][i]);
            }
        }

        badWalkInPassivesValues = new bool [gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < classifiedPassiveEffectArea[1].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[1][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[1][i].passiveLocations[j];
                badWalkInPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[1][i]);
            }
        }

        goodWalkinPassivesValues = new bool [gameManager.mapSize, gameManager.mapSize];
        for (int i = 0; i < classifiedPassiveEffectArea[2].Count; i++)
        {
            for (int j = 0; j < classifiedPassiveEffectArea[2][i].passiveLocations.Count; j++)
            {
                Vector2Int passiveLocation = classifiedPassiveEffectArea[2][i].passiveLocations[j];
                goodWalkinPassivesValues[passiveLocation.x, passiveLocation.y] = true;
                passives[passiveLocation.x, passiveLocation.y].Add(classifiedPassiveEffectArea[2][i]);
            }
        }

    }

    public void SetUp(Vector3 targetPosition, int numActionPoints, int currentMoveSpeed)
    {
        ResetSetUp();

        moveSpeedInitiallyAvailable = currentMoveSpeed;
        DijkstraMap map = gameManager.map;
        map.getGrid().GetXY(targetPosition, out int x, out int y);
        map.ResetMap(true);

        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);

        int startValue = currentMoveSpeed + (movingUnit.moveSpeedPerMoveAction * numActionPoints);
        List<DijkstraMapNode> nodesInMovementRange =  map.GetNodesInMovementRange(x, y, startValue, movingUnit.moveModifier, gameManager);

        startingPosition = targetPosition;
        actionPointsLeft = numActionPoints;

        if (nodesInMovementRange.Count > 1)
        {
            for (int i = 0; i < nodesInMovementRange.Count; i++)
            {
                DijkstraMapNode currentNode = nodesInMovementRange[i];
                int nodeValue = startValue - currentNode.value;
                Vector2Int currentNodePosition = new Vector2Int(currentNode.x, currentNode.y);
                int rangeBracketOfNode;
                if (currentMoveSpeed > 0)
                {
                    int tempNodeValue = nodeValue - currentMoveSpeed;
                    if(tempNodeValue <= 0)
                    {
                        rangeBracketOfNode = - 1;
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
                if(unitOnHex == null)
                {
                    groundColorValues.Add(rangeBracketOfNode);
                    groundHexes.Add(currentNodePosition);
                }
            }
            PlaceGroundHexes();
        }
        canMove = groundHexes.Count > 0;
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
            highlightedHexes.Add(newHighlightedHex);
        }
    }

    // Clear Highlighted Hexes
    public void ResetSetUp()
    {
        for(int i = 0; i < highlightedHexes.Count; i++)
        {
            gameManager.spriteManager.DisableHighlightedHex(highlightedHexes[i]);
        }
        highlightedHexes = new List<GameObject>();
        groundHexes = new List<Vector2Int>();
        groundColorValues = new List<int>();
    }

    // Select New Position when Mouse Hovers over a new Hex
    public override void SelectNewPosition(Vector2Int newHex)
    {
        this.currentlySelectedHex = newHex;
        map.ResetMap();
        List<Vector2Int> endHex = new List<Vector2Int>();
        amountActionLineIncreased = 0;
        int endX = currentlySelectedHex.x;
        int endY = currentlySelectedHex.y;
        if (map.getGrid().GetGridObject(endX, endY) != null)
        {
            endHex.Add(new Vector2Int(endX, endY));
            map.SetGoalsNew(endHex, gameManager, movingUnit.moveModifier, badWalkInPassivesValues);
            map.getGrid().GetXY(startingPosition, out int x, out int y);
            path.Clear();
            bool foundEndPosition = false;
            int currentMoveSpeed = moveSpeedInitiallyAvailable + movingUnit.moveSpeedPerMoveAction * actionPointsLeft;
            int previousNodeMoveValue = map.getGrid().GetGridObject(x, y).value;
            int startx = x;
            int starty = y;
            moveSpeedUsed = 0;
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
                path.Add(new Vector2Int(x,y));

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
                    // This is for going to target Hex where an unwalkable space is
                    // Removes duplicate path and ends loop
                    else if (path.Count >= 2 && path[path.Count - 1] == path[path.Count - 2])
                    {
                        path.RemoveAt(path.Count - 1);
                        break;
                    }
                }
            }
            DrawLine();
        }  
    }
    public override void EndTargeting()
    {
        int x = currentlySelectedHex.x;
        int y = currentlySelectedHex.y;

        if (x >= gameManager.grid.GetWidth() || x < 0 || y >= gameManager.grid.GetHeight() || y < 0 || path.Count == 0)
        {
            return;
        }

        Vector2Int endHexPosition = new Vector2Int(x, y);
         
        // Case -  Player Clicks on previously selected Hex
        // Confirms action
        if (prevEndHexPosition.x >= 0 && endHexPosition == prevEndHexPosition)
        {
                gameManager.spriteManager.ConfirmAction();
        }
        //Case - Player clicks on hex that has not been previously selected
        else
        {
            // Select Player Unit when you aren't using a friendly ability like heal
            if (!targetFriendly && gameManager.playerTurn.CheckSpaceForFriendlyUnit(endHexPosition))
            {
                OnFoundTarget?.Invoke(null, null, movingUnit, false);
                gameManager.playerTurn.SelectUnit(endHexPosition);
            }
            else
            {

                //if there is still action Points left Add temp Path to Set Path
                if (canMove)
                {
                    prevEndHexPosition = endHexPosition;
                    for (int i = 0; i < path.Count; i++)
                    {
                        setPath.Add(path[i]);
                    }
                }
                
                Vector2Int endPathHex = setPath[setPath.Count - 1];

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
                tempMovingUnit = null;
                tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, 1, movingUnit.unitProfile);
                SetUp(map.getGrid().GetWorldPosition(setPath[setPath.Count - 1]), actionPointsLeft, moveSpeedLeft);
                gameManager.spriteManager.ActivateActionConfirmationMenu(
                    () => // Confirm Action
                    {
                        if(setPath.Count == 1 && movingUnit.gameManager.map.getGrid().GetWorldPosition(setPath[0].x, setPath[0].y) == movingUnit.transform.position)
                        {
                            OnFoundTarget?.Invoke(null, null, movingUnit, false);
                            gameManager.playerTurn.SelectUnit(endHexPosition);
                        }
                        else
                        {
                            OnFoundTarget?.Invoke(setPath, null, movingUnit, true);
                            Destroy(tempMovingUnit);
                        }
                    },
                    () => // Cancel Action
                    {
                        ResetTargeting();
                        SetUp(movingUnit.transform.position, movingUnit.currentMajorActionsPoints, movingUnit.currentMoveSpeed);
                    });
                
            }
        }
    }

    public void ResetTargeting()
    {
        Destroy(tempMovingUnit);
        tempMovingUnit = null;
        setPath = new List<Vector2Int>();
        path = new List<Vector2Int>();
        prevEndHexPosition = new Vector2Int(-1, -1);
        actionMoveAmounts = new List<int>();
        actionLines = new List<List<Vector3>>();
        amountActionLineIncreased = 0;
        IndexOfStartingActionLine = 0;
        gameManager.spriteManager.ClearLines();
        for (int i = 0; i < targetingPassiveSpriteHolder.Count; i++)
        {
            gameManager.spriteManager.DisableTargetingSpriteHolder(targetingPassiveSpriteHolder[i]);
        }
        targetingPassiveSpriteHolder.Clear();
    }

    public override void DeactivateTargetingSystem()
    {
        ResetSetUp();
        ResetTargeting();
        map.ResetMap(true);
        OnFoundTarget = null;
    }

    public void DrawLine()
    {
        if (!canMove)
        {
            return;
        }
        
        actionLines =  new List<List<Vector3>>() { new List<Vector3>() };
        actionLines[0].Add(movingUnit.transform.position);
        for(int i = 0; i < setPath.Count; i++)
        {
            actionLines[0].Add(gameManager.spriteManager.GetWorldPosition(setPath[i].x, setPath[i].y));
        }

        for (int i = 0; i < path.Count; i++)
        {
            actionLines[0].Add(gameManager.spriteManager.GetWorldPosition(path[i].x, path[i].y));
        }

        gameManager.spriteManager.DrawLine(actionLines[0], 0);

        passiveEffectAreas = new List<PassiveEffectArea>();
        passiveSprites = new List<Tuple<Passive, Vector2Int>>();

        for(int i = 0; i < targetingPassiveSpriteHolder.Count; i++)
        {
            gameManager.spriteManager.DisableTargetingSpriteHolder(targetingPassiveSpriteHolder[i]);
        }
        targetingPassiveSpriteHolder.Clear();

        CheckPassives(new List<Vector2Int>() { new Vector2Int(movingUnit.x, movingUnit.y) });
        CheckPassives(setPath);
        CheckPassives(path);

        //Hex Position, num Passives
        Dictionary<Vector2Int, int> passivesGrid =  new Dictionary<Vector2Int, int>();
        for(int i = 0; i < passiveSprites.Count; i++)
        {
            SpriteHolder tempTargetPassiveSprite =  gameManager.spriteManager.UseOpenTargetingSpriteHolder();
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
