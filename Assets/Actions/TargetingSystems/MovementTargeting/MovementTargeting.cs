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

    public List<PassiveEffectArea>[,] passives;
    List<PassiveEffectArea> passiveEffectAreas = new List<PassiveEffectArea>();
    List<Tuple<Passive, Vector2Int>> passiveSprites = new List<Tuple<Passive, Vector2Int>>();

    public bool[,] unwalkablePassivesValues;
    public bool[,] badWalkInPassivesValues;
    public bool[,] goodWalkinPassivesValues;
    public List<int> startOfNewMoveIndexes = new List<int>();
    public List<int> setStartOfNewMoveIndexes = new List<int>();
    public GameObject tempMovingUnit;

    public List<int> actionMoveAmounts;
    public int amountMoved = 0;
    public int amountOfPossibleMoves;

    public int amountActionLineIncreased = 0;
    public int IndexOfStartingActionLine = 0;
    List<List<Vector3>> actionLines = new List<List<Vector3>>();

    public List<List<Vector2Int>> rangeBrackets =  new List<List<Vector2Int>>();
    public List<GameObject> highlightedHexes;
    public List<SpriteHolder> targetingPassiveSpriteHolder = new List<SpriteHolder>();

    public bool targetFriendly = false;

    public int actionPointsLeft;

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
        amountMoved = movingUnit.amountMoveUsedDuringRound;
        highlightedHexes = new List<GameObject>();
        SetUp(startingPosition, actionPointsLeft, movingUnit.moveSpeed);

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

    public void SetUp(Vector3 targetPosition, int numActionPoints, int moveSpeed)
    {
        ResetSetUp();

        DijkstraMap map = gameManager.map;
        map.getGrid().GetXY(targetPosition, out int x, out int y);
        map.ResetMap(true);

        movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);


        map.SetGoals(new List<Vector2Int>() { new Vector2Int(x, y) }, gameManager, movingUnit.moveModifier);
        startingPosition = targetPosition;

        actionPointsLeft = numActionPoints;

        int initialActionPoints = numActionPoints;
        int moveAmounts = 0;
        while(initialActionPoints > 0)
        {
            if(initialActionPoints >= amountMoved + moveAmounts +  1)
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

        amountOfPossibleMoves = moveAmounts;
        int currentRadius = 1;
        if (moveAmounts > 0)
        {
            for (int i = 1; i <= moveAmounts; i++)
            {

                rangeBrackets.Add(new List<Vector2Int>());
                for (int j = 0; j < unresolvedMapNodes.Count; j++)
                {
                    Vector2Int currentNodePosition = new Vector2Int(unresolvedMapNodes[j].x, unresolvedMapNodes[j].y);
                    if (map.getGrid().GetGridObject(unresolvedMapNodes[j].x, unresolvedMapNodes[j].y).value <= moveSpeed * i)
                    {
                        GameObject newHighlightedHex = gameManager.spriteManager.UseOpenHighlightedHex();
                        newHighlightedHex.transform.position = gameManager.spriteManager.GetWorldPosition(currentNodePosition);
                        newHighlightedHex.GetComponent<SpriteRenderer>().color = gameManager.resourceManager.highlightedHexColors[amountMoved + i];
                        newHighlightedHex.GetComponent<SpriteRenderer>().sortingOrder = gameManager.spriteManager.terrain[currentNodePosition.x,
                            currentNodePosition.y].sprite.sortingOrder + 1;
                        highlightedHexes.Add(newHighlightedHex);
                        rangeBrackets[i - 1].Add(currentNodePosition);
                        unresolvedMapNodes.RemoveAt(j);
                        j--;
                    }
                }

                for (int j = currentRadius; j <= moveSpeed * i; j++)
                {
                    mapNodes = map.getGrid().GetGridObjectsInRing(x, y, j);
                    for (int k = 0; k < mapNodes.Count; k++)
                    {
                        Vector2Int currentNodePosition = new Vector2Int(mapNodes[k].x, mapNodes[k].y);
                        if (map.getGrid().GetGridObject(mapNodes[k].x, mapNodes[k].y).value <= moveSpeed * i)
                        {
                            GameObject newHighlightedHex = gameManager.spriteManager.UseOpenHighlightedHex();
                            newHighlightedHex.transform.position = gameManager.spriteManager.GetWorldPosition(currentNodePosition);
                            newHighlightedHex.GetComponent<SpriteRenderer>().color = gameManager.resourceManager.highlightedHexColors[amountMoved + i];
                            newHighlightedHex.GetComponent<SpriteRenderer>().sortingOrder = gameManager.spriteManager.terrain[currentNodePosition.x,
                                currentNodePosition.y].sprite.sortingOrder + 1;
                            highlightedHexes.Add(newHighlightedHex);
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
    }

    // Clear Highlighted Hexes
    public void ResetSetUp()
    {
        for(int i = 0; i < highlightedHexes.Count; i++)
        {
            gameManager.spriteManager.DisableHighlightedHex(highlightedHexes[i]);
        }
        highlightedHexes = new List<GameObject>();
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
            map.SetGoals(endHex, gameManager, movingUnit.moveModifier, badWalkInPassivesValues);
            map.getGrid().GetXY(startingPosition, out int x, out int y);
            path.Clear();
            startOfNewMoveIndexes = new List<int>();
            bool foundEndPosition = false;
            bool findMostDirectPath = false;
            int initialAmountPathMoveIncreased = amountActionLineIncreased;

            // loop through possible move amounts to find a path to end hex
            for (int i = 0; i < amountOfPossibleMoves; i++)
            {
                startOfNewMoveIndexes.Add(path.Count);
                actionMoveAmounts.Add(0);
                amountActionLineIncreased += 1;
                bool inRangeBracket = rangeBrackets[i].Contains(new Vector2Int(endX, endY));
                int startx = x;
                int starty = y;
                DijkstraMapNode currentNode;

                // Attempt to find path Avoiding harmful Terrain
                for (int j = 0; j < movingUnit.moveSpeed; j++)
                {
                    currentNode = map.GetLowestNearbyNode(x, y, movingUnit.moveModifier, gameManager);
                    x = currentNode.x;
                    y = currentNode.y;
                    if (currentNode.value == int.MaxValue)
                    {
                        break;
                    }
                    path.Add(new Vector2Int(x, y));
                    actionMoveAmounts[i] = actionMoveAmounts[i] + 1;
                    if (endHex[0] == path[path.Count - 1])
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

                // If it is possible to reach end position in range bracket Reset data No Passives and try again, ignore badMoveinPlaces
                if (inRangeBracket && !foundEndPosition)
                {
                    // Reset Data From Initial attempt to find path
                    actionMoveAmounts[i] = 0;
                    for (int j = 0; j < movingUnit.moveSpeed; j++)
                    {
                        if(path.Count == 0)
                        {
                            break;
                        }
                        path.RemoveAt(path.Count - 1);
                    }
                    x = startx; 
                    y = starty;

                    //Try to find Path
                    map.ResetMap(true);
                    movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
                    map.SetGoals(endHex, gameManager, movingUnit.moveModifier);
                    for (int j = 0; j < movingUnit.moveSpeed; j++)
                    {
                        currentNode = map.GetLowestNearbyNode(x, y, movingUnit.moveModifier, gameManager);
                        x = currentNode.x;  
                        y = currentNode.y;
                        path.Add(new Vector2Int(x, y));
                        actionMoveAmounts[i] = actionMoveAmounts[i] + 1;
                        if (endHex[0] == path[path.Count - 1])
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
                map.ResetMap(true);
                movingUnit.moveModifier.SetUnwalkable(gameManager, movingUnit);
                map.SetGoals(endHex, gameManager, movingUnit.moveModifier);
                map.getGrid().GetXY(startingPosition, out x, out y);
                path.Clear();
                startOfNewMoveIndexes = new List<int>();
                foundEndPosition = false;
                for (int i = 0; i < amountOfPossibleMoves; i++)
                {
                    amountActionLineIncreased += 1;
                    actionMoveAmounts.Add(0);
                    startOfNewMoveIndexes.Add(path.Count);
                    // Attempt to find path Avoiding harmful Terrain
                    for (int j = 0; j < movingUnit.moveSpeed; j++)
                    {
                        DijkstraMapNode currentNode = map.GetLowestNearbyNode(x, y, movingUnit.moveModifier, gameManager);
                        x = currentNode.x;
                        y = currentNode.y;
                        path.Add(new Vector2Int(currentNode.x, currentNode.y));
                        actionMoveAmounts[i] = actionMoveAmounts[i] + 1;
                        if (endHex[0] == path[path.Count - 1])
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
        // Determine Latest Action Line
        for(int i = actionLines.Count - 1; i >= 0; i--)
        {
            if (actionLines[i].Count != 0)
            {
                IndexOfStartingActionLine = i + 1;
                break;
            } 
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
                if (actionPointsLeft > 0)
                {
                    for(int i = 0; i< startOfNewMoveIndexes.Count; i++)
                    {
                        setStartOfNewMoveIndexes.Add(startOfNewMoveIndexes[i]);
                    }
                    prevEndHexPosition = endHexPosition;
                    for (int i = 0; i < path.Count; i++)
                    {
                        setPath.Add(path[i]);
                    }
                }
                
                Vector2Int endPathHex = setPath[setPath.Count - 1];

                for(int i = 0; i < rangeBrackets.Count; i++)
                {
                    if (rangeBrackets[i].Contains(endPathHex))
                    {
                        for(int j = 0; j <= i; j++)
                        {
                            actionPointsLeft -= amountMoved + (j + 1);
                        }
                        amountMoved += i + 1;
                    }
                }

                Destroy(tempMovingUnit);
                tempMovingUnit = null;
                tempMovingUnit = gameManager.spriteManager.CreateTempSpriteHolder(endPathHex, 1, movingUnit.unitProfile);
                SetUp(map.getGrid().GetWorldPosition(setPath[setPath.Count - 1]), actionPointsLeft, movingUnit.moveSpeed);
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
                            OnFoundTarget?.Invoke(setPath, setStartOfNewMoveIndexes, movingUnit, true);
                            Destroy(tempMovingUnit);
                        }
                    },
                    () => // Cancel Action
                    {
                        ResetTargeting();
                        SetUp(movingUnit.transform.position, movingUnit.currentActionsPoints, movingUnit.moveSpeed);
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
        setStartOfNewMoveIndexes = new List<int>();
        amountActionLineIncreased = 0;
        IndexOfStartingActionLine = 0;
        amountMoved = 0;
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
        if (actionPointsLeft <= 0)
        {
            return;
        }

        if(IndexOfStartingActionLine > 0)
        {
            for (int i = 0; i < amountActionLineIncreased; i++)
            {
                if(i > IndexOfStartingActionLine)
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
        for(int i = IndexOfStartingActionLine; i < IndexOfStartingActionLine + amountActionLineIncreased; i++)
        { 
            if(actionLines.Count <= i)
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

            for(int j = 0; j < actionMoveAmounts[i - IndexOfStartingActionLine]; j++)
            {
                actionLines[i].Add(gameManager.spriteManager.GetWorldPosition(path[currentPathindex].x, path[currentPathindex].y));
                currentPathindex += 1;
            }
            gameManager.spriteManager.DrawLine(actionLines[i], i);
        }

        for(int i = 0; i < amountActionLineIncreased; i++)
        {
            actionMoveAmounts.RemoveAt(actionMoveAmounts.Count - 1);
        }

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
                if (!passiveEffectAreas.Contains(passivesOnLocation[j]) && !passivesUsed.Contains(passivesOnLocation[j].passive))
                {
                    passiveEffectAreas.Add(passivesOnLocation[j]);
                    Tuple<Passive, Vector2Int> tempPassiveSprite = passivesOnLocation[j].GetTargetingData(new Vector2Int(movingUnit.x, movingUnit.y), path, 
                        setPath, passivesOnLocation[j].passiveLocations);
                    if (tempPassiveSprite.Item2.x != -1)
                    {
                        passiveSprites.Add(tempPassiveSprite);
                    }
                    passivesUsed.Add(passivesOnLocation[j].passive);
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
