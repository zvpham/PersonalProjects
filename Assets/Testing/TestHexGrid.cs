using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Inventory.Model;
using UnityEngine.UI;
using JetBrains.Annotations;
using static UnityEngine.UI.CanvasScaler;

public class TestHexGrid : MonoBehaviour
{
    public PrefabTerrain prefabTerrain;
    public PrefabUnitTerrain prefabUnitTerrain;
    public GameObject inventoryUI;
    public GameObject startCombatButton;
    public TestCharacterSystem characterSystem;
    public TestInventorySystem inventorySystem;
    public float cellSize;
    public Vector3 defaultGridAdjustment;
    public List<Vector3Int> highlightedHexws = new List<Vector3Int>();
    public ResourceManager resourceManager;
    public int index = 0;
    public int unitNameIndex = 0;
    public int mouseIndex;
    public Vector2Int startHex;
    public Vector2Int endHex;

    public InputManager inputManager;
    public TestInputManager testInputManager;
    public MenuInputManager menuInputManager;

    public int defaultElevation = 3;
    public float terrainHeightDifference = 0.16125f;

    public List<Unit> unitsinCombat =  new List<Unit>();
    public CombatGameManager gameManager;
    public Vector2Int currentlySelectedHex;
    public GameObject currentlySelectedHexSprite;

    public bool inInventory = false;
    public int earthWallIndex = 0;

    public UnitGroup currentlyPlacingUnitGroup;
    public int unitInUnitGroupIndex = 0;

    public Vector2Int mouseHex;
    public Vector2Int prevMouseHex;
    public List<Vector2Int> lineOfSightHexes= new List<Vector2Int>();   

    // Start is called before the first frame update
    void Start()
    {
        //hexgrid =  new GridHex<GridPosition>(10, 10, 1, new Vector3(-0.5f, -0.5f, 0), (GridHex<GridPosition> Grid, int x, int y) => new GridPosition(Grid, x, y, 3), true);
        inputManager.gameObject.SetActive(false);
        testInputManager.MouseMoved += SelectMouseHex;
        CreateGrid(32, 32, 7);
        LoadUnitsToBoard();
        inventoryUI.SetActive(false);
    }

    public void StartCombat()
    {
        if(currentlyPlacingUnitGroup != null || unitsinCombat.Count ==0)
        {
            return;
        }

        List<UnitGroup> unitGroupsInCombat = new List<UnitGroup>();

        for(int i = 0; i < unitsinCombat.Count; i++)
        {
            unitsinCombat[i].GetReadyForCombat();
            if (unitsinCombat[i].group != null && !unitGroupsInCombat.Contains(unitsinCombat[i].group))
            {
                unitGroupsInCombat.Add(unitsinCombat[i].group);
            }
        }

        for(int i = 0; i < unitGroupsInCombat.Count; i++)
        {
            unitGroupsInCombat[i].GetReadyForCombat();
        }

        gameManager.StartCombat();
        inputManager.gameObject.SetActive(true);
        testInputManager.gameObject.SetActive(false);
        menuInputManager.gameObject.SetActive(false);
        inventoryUI.SetActive(false);
        startCombatButton.SetActive(false);
    }

    public void ResetTest()
    {
        gameManager.EndCombatTesting();
        inputManager.gameObject.SetActive(false);
        testInputManager.gameObject.SetActive(true);
        menuInputManager.gameObject.SetActive(true);

        inventoryUI.SetActive(true);
        startCombatButton.SetActive(true);
        unitsinCombat = new List<Unit>();
    }
    public void CreateGrid(int mapWidth, int mapHeight, int amountOfTerrainLevels)
    {
        gameManager.spriteManager.CreateGrid(mapWidth, mapHeight, amountOfTerrainLevels, cellSize, defaultGridAdjustment);
        for (int i = 0; i < prefabTerrain.terrainElevation.Count; i++)
        {
            Vector3Int terrainHexData = prefabTerrain.terrainElevation[i];
            gameManager.spriteManager.elevationOfHexes[terrainHexData.x, terrainHexData.y] = terrainHexData.z;
        }
    
        for (int i = 0; i < prefabTerrain.terrainElevation.Count; i++)
        {
            Vector3Int terrainHexData = prefabTerrain.terrainElevation[i];
            gameManager.spriteManager.terrainTilePositions[terrainHexData.z].Add(new Vector2Int(terrainHexData.x, terrainHexData.y));
            gameManager.spriteManager.ChangeElevation(terrainHexData.x, terrainHexData.y, 0);
        }
        gameManager.grid = new GridHex<GridPosition>(mapWidth, mapHeight, cellSize, defaultGridAdjustment, (GridHex<GridPosition> g, int x, int y) =>
        new GridPosition(g, x, y, defaultElevation), false);
        gameManager.map = new DijkstraMap(mapWidth, mapHeight, cellSize, defaultGridAdjustment, false);
        gameManager.passiveGrid = new GridHex<PassiveGridObject>(mapWidth, mapHeight, cellSize, defaultGridAdjustment, (GridHex<PassiveGridObject> g, int x, int y) =>
        new PassiveGridObject(g, x, y), false);

        Debug.LogWarning("Change MOveCostMap Values when there are new terrain tiles");

        gameManager.moveCostMap = new int[mapHeight, mapWidth];
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                gameManager.moveCostMap[j, i] = 6;
                //moveCostMap[j, i] = resourceManager.terrainTiles[0].moveCost;
            }
        }
        inventorySystem.LoadInitialItems();
        characterSystem.LoadInitialUnits();

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            mouseHex = currentlySelectedHex;
        }

        if (currentlyPlacingUnitGroup != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Unit currentlyPlacingUnit = currentlyPlacingUnitGroup.units[unitInUnitGroupIndex];
                HandlePlaceUnit(currentlyPlacingUnit);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            //TestAiTime();
            //TestParameters();

        }


        /* Testing Get MovementMaps of Unit Actions: IE - Get Evade Movement Map and displaying it
        if(Input.GetMouseButtonDown(1))
        //if (prevMouseHex != currentlySelectedHex)
        {
            int[,] testMovementMap;
            //LineOfSight(mouseHex, currentlySelectedHex);
            Unit currentUnit =  gameManager.playerTurn.currentlySelectedUnit;
            for(int i =0; i < currentUnit.actions.Count; i++)
            {
                if (currentUnit.actions[i].action.GetType() == typeof(Evade))
                {
                        AIActionData data = new AIActionData();
                    data.unit = currentUnit;
                    data.originalPosition = new Vector2Int(currentUnit.x, currentUnit.y);
                    testMovementMap = currentUnit.actions[i].action.GetMovementMap(data);
                    DijkstraMap map = gameManager.map;

                    for(int k = 0; k < testMovementMap.GetLength(0); k++)
                    {
                        for(int j  = 0; j < testMovementMap.GetLength(1); j++)
                        {
                            DijkstraMapNode node = map.getGrid().GetGridObject(k, j);
                            node.value = testMovementMap[k, j];
                            map.getGrid().SetGridObject(k, j, node);
                        }
                    }
                }
            }
            prevMouseHex = currentlySelectedHex;
        }
        */
    }
 


    public bool LineOfSight(Vector2Int originalPosition, Vector2Int targetPosiiton)
    {

        for(int i = 0; i < lineOfSightHexes.Count; i++)
        {
            gameManager.spriteManager.terrain[lineOfSightHexes[i].x, lineOfSightHexes[i].y].sprite.color = Color.white;
        }

        List<List<Vector2Int>> sightLines = gameManager.grid.LineOfSightSuperCover(originalPosition, targetPosiiton);
        int originalElevation = gameManager.spriteManager.elevationOfHexes[originalPosition.x, originalPosition.y];
        bool reachedEndHex = false;
        for(int i = 0; i < sightLines.Count; i++)
        {
            /*
            string debug = " " + i + ": ";
            for (int j = 0; j < sightLines[i].Count; j++)
            {
                debug += sightLines[i][j] + ", ";
            }
            Debug.Log(debug);
            */
            reachedEndHex = true;
            for (int j = 0; j < sightLines[i].Count; j++)
            {
                Vector2Int currentHex = sightLines[i][j];
                if(gameManager.spriteManager.elevationOfHexes[currentHex.x, currentHex.y] > originalElevation)
                {
                    reachedEndHex = false;
                    break;
                }
                lineOfSightHexes.Add(currentHex);
                gameManager.spriteManager.terrain[currentHex.x, currentHex.y].sprite.color = Color.cyan;
            }
            if(reachedEndHex)
            {
                break;
            }
        }

        return reachedEndHex;
    }

    public void TestParameters()
    {
        List<IInititiave> test1 = new List<IInititiave>();
        for(int i = 0; i < 4; i++)
        {
            Unit tempHero = Instantiate(resourceManager.emptyHero);
            tempHero.name = tempHero.name + ", " + i.ToString();
            test1.Add(tempHero);
        }
        List<int> test2 = new List<int>() { 3, 5, 1, 2 };

        Debug.Log(test2.Count);
        string debugWOrd = "";
        for(int i =0 ; i < test1.Count; i++)
        {
            debugWOrd += test1[i] + ", ";
        }

        Debug.Log(debugWOrd);
        test1[0].Quicksort(test2, test1, 0, test1.Count - 1);


        debugWOrd = "";
        for (int i = 0; i < test1.Count; i++)
        {
            debugWOrd += test1[i] + ", ";
        }

        Debug.Log(debugWOrd);
    }

    public void TestAiTime()
    {
        Unit unit = gameManager.playerTurn.currentlySelectedUnit;
        int[,] movementData = new int[gameManager.mapSize, gameManager.mapSize];
        AIActionData actionData = new AIActionData();
        actionData.unit = unit;
        actionData.originalPosition = new Vector2Int(unit.x, unit.y);
        actionData.AIState = AITurnStates.Combat;

        List<Unit> visibleUnits = new List<Unit>();
        List<Vector2Int> enemyUnits =  new List<Vector2Int>();
        for (int i = 0; i < gameManager.units.Count; i++)
        {
            if (gameManager.units[i].team != Team.Player)
            {
                visibleUnits.Add(gameManager.units[i]);
                enemyUnits.Add(new Vector2Int(gameManager.units[i].x, gameManager.units[i].y));
            }
        }
        actionData.enemyUnits = enemyUnits;

        int actionIndex = -1;
        for (int i = 0; i < unit.actions.Count; i++)
        {
            if (unit.actions[i].action.actionTypes.Contains(ActionType.Movement))
            {
                AIActionData data = new AIActionData();
                data.unit = unit;
                data.originalPosition = new Vector2Int(unit.x, unit.y);
                unit.actions[i].action.GetMovementMap(data);
            }
        }
        
        actionData.inMelee = false;
        actionData.movementData = movementData;
        List<Vector2Int> enemyUnitHexPositions = new List<Vector2Int>();
        for (int i = 0; i < visibleUnits.Count; i++)
        {
            enemyUnitHexPositions.Add(new Vector2Int(visibleUnits[i].x, visibleUnits[i].y));
        }

        List<int> actionsInRange = GetActionsInRange(actionData, unit);
        if (actionsInRange.Count <= 0)
        {
            Debug.LogWarning("Testing Change This");
        }
        else
        {
            actionIndex = GetHighestActionIndex(actionData, unit, actionsInRange);
            Debug.Log(actionIndex);
        }
    }


    public int GetHighestActionIndex(AIActionData actionData, Unit unit, List<int> actionsInRange)
    {
        int actionIndex = -1;
        int highestActionWeight = -1;
        for (int i = 0; i < actionsInRange.Count; i++)
        {
            int actionWieght = unit.actions[actionsInRange[i]].action.CalculateWeight(actionData);
            if (highestActionWeight < actionWieght)
            {
                actionIndex = i;
                highestActionWeight = actionIndex;
            }
        }
        return actionIndex;
    }

    public List<int> GetActionsInRange(AIActionData actionData, Unit unit)
    {
        List<int> actionsInRange = new List<int>();
        for (int i = 0; i < unit.actions.Count; i++)
        {
            if (unit.actions[i].action.CheckIfActionIsInRange(actionData))
            {
                actionsInRange.Add(i);
            }
        }
        return actionsInRange;
    }


    public void GetTriangels()
    {
                    /*
            gameManager.spriteManager.ResetTriangle();
            List<List<Vector2Int>> sightLines = gameManager.grid.GetTriangles(originalPosition, targetPosiiton);
            for (int i = 0; i < sightLines.Count; i++)
            {
                for (int j = 0; j < sightLines[i].Count; j++)
                {
                    Vector2Int currentTriangle = sightLines[i][j];
                    gameManager.spriteManager.UseTriangle(currentTriangle);
                }
                gameManager.spriteManager.SetTriangleLine(i, sightLines[i][0], sightLines[i][sightLines[i].Count - 1]);
            }
            */
    }

    public void FindAngle(Vector2Int a, Vector2Int b)
    {
        Vector3 startPosition = gameManager.grid.GetWorldPosition(a);
        Vector3 endPosition = gameManager.grid.GetWorldPosition(b);
        float angle = (Mathf.Atan2(endPosition.y - startPosition.y, endPosition.x - startPosition.x) * Mathf.Rad2Deg);
        Debug.Log(Mathf.RoundToInt(angle));
    }

    public void HandlePlaceUnit(Unit unit)
    {
        Vector3 newUnitPosition = gameManager.spriteManager.GetWorldPosition(currentlySelectedHex);
        if (gameManager.spriteManager.spriteGrid.GetGridObject(newUnitPosition).sprites[0] != null)
        {
            if(gameManager.grid.GetGridObject(newUnitPosition).unit.group != null)
            {
                UnitGroup unitGroup = gameManager.grid.GetGridObject(newUnitPosition).unit.group;
                if(unitGroup == currentlyPlacingUnitGroup)
                {
                    return;
                }

                for (int i = 0; i < unitGroup.units.Count; i++)
                {
                    unitsinCombat.Remove(unitGroup.units[i]);
                    Vector3 UnitInExistingUnitGroupPosition = unitGroup.units[i].transform.position;
                    Destroy(gameManager.spriteManager.spriteGrid.GetGridObject(UnitInExistingUnitGroupPosition).sprites[0].gameObject);
                    Destroy(gameManager.grid.GetGridObject(UnitInExistingUnitGroupPosition).unit.gameObject);
                    gameManager.grid.GetGridObject(UnitInExistingUnitGroupPosition).unit = null;
                }
                Destroy(unitGroup.gameObject);
            }
            else
            {
                unitsinCombat.Remove(gameManager.grid.GetGridObject(newUnitPosition).unit);
                Destroy(gameManager.spriteManager.spriteGrid.GetGridObject(newUnitPosition).sprites[0].gameObject);
                Destroy(gameManager.grid.GetGridObject(newUnitPosition).unit.gameObject);
                gameManager.grid.GetGridObject(newUnitPosition).unit = null;
            }
        }
        if(unit.group != null)
        {
            unitInUnitGroupIndex += 1;
            if (currentlyPlacingUnitGroup.units.Count <= unitInUnitGroupIndex)
            {
                currentlyPlacingUnitGroup = null;
            }
        }

        unit.transform.position = newUnitPosition;
        gameManager.SetGridObject(unit, unit.transform.position);
        unit.unitSpriteRenderer = gameManager.spriteManager.CreateSpriteRenderer(0, unit.unitProfile, unit.transform.position);
        unitsinCombat.Add(unit);
    }

    public void RemoveUnit()
    {
        if (currentlyPlacingUnitGroup != null)
        {
            return;
        }

        Unit unit = gameManager.grid.GetGridObject(currentlySelectedHex).unit;
        if(unit != null)
        {
            // Remove entire Unit Group
            if(unit.group != null)
            {
                UnitGroup unitGroup = unit.group;
                for (int i = 0; i < unitGroup.units.Count; i++)
                {
                    unitsinCombat.Remove(unitGroup.units[i]);
                    Vector3 UnitInExistingUnitGroupPosition = unitGroup.units[i].transform.position;
                    Destroy(gameManager.spriteManager.spriteGrid.GetGridObject(UnitInExistingUnitGroupPosition).sprites[0].gameObject);
                    Destroy(gameManager.grid.GetGridObject(UnitInExistingUnitGroupPosition).unit.gameObject);
                    gameManager.grid.GetGridObject(UnitInExistingUnitGroupPosition).unit = null;
                    gameManager.grid.SetGridObject(UnitInExistingUnitGroupPosition, gameManager.grid.GetGridObject(UnitInExistingUnitGroupPosition));
                }
            }
            else
            {
                unitsinCombat.Remove(unit);
                Destroy(gameManager.spriteManager.spriteGrid.GetGridObject(currentlySelectedHex).sprites[0].gameObject);
                Destroy(unit.gameObject);
                gameManager.grid.GetGridObject(currentlySelectedHex).unit = null;
                gameManager.grid.SetGridObject(currentlySelectedHex, gameManager.grid.GetGridObject(currentlySelectedHex));
            }
        }
    }
    public void PlaceUnit(int teamIndex)
    {
        if (currentlyPlacingUnitGroup != null)
        {
            return;
        }

        if (characterSystem.currentUnit != null)
        {
            if(characterSystem.currentUnit.group != null)
            {
                UnitGroup unitGroup = Instantiate(characterSystem.currentUnit.group);
                currentlyPlacingUnitGroup = unitGroup;
                unitInUnitGroupIndex = 0;
                Team unitTeam = Team.Player;
                switch (teamIndex)
                {
                    case 0:
                        unitTeam = Team.Player;
                        break;
                    case 1:
                        unitTeam = Team.Team2;
                        break;
                    case 2:
                        unitTeam = Team.Team3;
                        break;
                    case 3:
                        unitTeam = Team.Team4;
                        break;
                }

                for (int i = 0; i < unitGroup.units.Count; i++)
                {
                    Unit childUnit = unitGroup.units[i];
                    unitGroup.units[i].team = unitTeam;
                    childUnit.name += " " + unitNameIndex;
                    unitNameIndex++;
                }
                HandlePlaceUnit(unitGroup.units[unitInUnitGroupIndex]);
            }
            else
            {
                Unit newUnit = Instantiate(characterSystem.currentUnit);
                newUnit.name += " " + unitNameIndex;
                unitNameIndex++;
                switch (teamIndex)
                {
                    case 0:
                        newUnit.team = Team.Player;
                        break;
                    case 1:
                        newUnit.team = Team.Team2;
                        break;
                    case 2:
                        newUnit.team = Team.Team3;
                        break;
                    case 3:
                        newUnit.team = Team.Team4;
                        break;
                }
                HandlePlaceUnit(newUnit);
            }
        }
    }

    public void ChangeViewElevation(int viewChangeAmount)
    {
        gameManager.spriteManager.SetViewElevation(gameManager.spriteManager.currentViewingElevation + viewChangeAmount);
    }

    public void ChangeElevation(int elevationChangeAmount)
    {
        if (inInventory || currentlyPlacingUnitGroup != null)
        {
            return;
        }
        gameManager.spriteManager.ChangeElevation(currentlySelectedHex.x, currentlySelectedHex.y, elevationChangeAmount, true);
    }

    public void EarthWall()
    {
        if (earthWallIndex == 0)
        {
            startHex = currentlySelectedHex;
            earthWallIndex = 1;
        }
        else
        {
            endHex = currentlySelectedHex;

            Vector3Int startCube = gameManager.spriteManager.spriteGrid.OffsetToCube(startHex.x, startHex.y);
            Vector3Int endCube = gameManager.spriteManager.spriteGrid.OffsetToCube(endHex.x, endHex.y);

            List<Vector3Int> cubePath = gameManager.spriteManager.spriteGrid.CubeLineDraw(startCube, endCube);

            for (int i = 0; i < cubePath.Count; i++)
            {
                Vector2Int offSetPosition = gameManager.spriteManager.spriteGrid.CubeToOffset(cubePath[i]);
                gameManager.spriteManager.ChangeElevation(offSetPosition.x, offSetPosition.y, 2, true, false);
            }
            earthWallIndex = 0;
        }
    }

    public void InteractWithInventory()
    {
        if(currentlyPlacingUnitGroup != null)
        {
            return;
        }

        if (inventoryUI.activeInHierarchy)
        {
            inInventory = false;
            inventorySystem.CloseMenu();
            startCombatButton.SetActive(true);
        }
        else
        {
            inInventory = true;
            inventorySystem.OpenMenu();
            startCombatButton.SetActive(false);
        }
    }

    public void SelectMouseHex()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);
        RaycastHit2D[] hit = Physics2D.RaycastAll(worldPoint, Vector2.zero);
        if(hit.GetLength(0) == 0)
        {
            Vector3 worldMousePosition = UtilsClass.GetMouseWorldPosition();
            gameManager.grid.GetXY(worldMousePosition, out int endX, out int endY);
            if(endX < 0 || endY < 0 || endX >= 32 || endY >= 32)
            {
                endX = 0; 
                endY = 0;
            }
            currentlySelectedHex = new Vector2Int(endX, endY);
            
        }
        else
        {
            List<TerrainHolder> terrains =  new List<TerrainHolder>();
            TerrainHolder terrainHolder = null;
            int highestElevation = -20;
            for(int i = 0; i < hit.GetLength(0); i++)
            {
                TerrainHolder tempterrainHolder = hit[i].transform.gameObject.GetComponent<TerrainHolder>();
                if(tempterrainHolder.sprite.sortingOrder > highestElevation)
                {
                    terrainHolder = tempterrainHolder;
                    highestElevation = terrainHolder.sprite.sortingOrder;
                }
            }
            currentlySelectedHex = new Vector2Int(terrainHolder.x, terrainHolder.y);
        }
        int hexElevation = gameManager.spriteManager.elevationOfHexes[currentlySelectedHex.x, currentlySelectedHex.y] - defaultElevation;
        currentlySelectedHexSprite.transform.position = gameManager.grid.GetWorldPosition(currentlySelectedHex) + new Vector3(0, terrainHeightDifference * hexElevation);
    }

    private void LoadUnitsToBoard()
    {
        int positionIndex = 0;
        for(int i = 0; i < prefabUnitTerrain.unitTeam.Count; i++)
        {
            battleLineData newBattleLineUnit = prefabUnitTerrain.units[i];
            // Load Player Unit
            if (prefabUnitTerrain.unitTeam[i] == 0)
            {
                positionIndex =  LoadPlayerUnitSuperGroupData(newBattleLineUnit, positionIndex);
            }
            else if(prefabUnitTerrain.unitTeam[i] == 1)
            {
                battleLineData enemyUnit = newBattleLineUnit;
                if (enemyUnit.unitGroupData.mercenaryIndex != -1)
                {
                    UnitGroup newUnitGroup = Instantiate(resourceManager.mercenaries[enemyUnit.unitGroupData.mercenaryIndex]);
                    newUnitGroup.gameManager = gameManager;
                    newUnitGroup.team = Team.Team4;
                    for (int j = 0; j < newUnitGroup.transform.childCount; j++)
                    {
                        Unit childUnit = newUnitGroup.transform.GetChild(j).GetComponent<Unit>();
                        childUnit.name += " " + unitNameIndex;
                        unitNameIndex++;
                        childUnit.team = Team.Team4;
                        childUnit.gameManager = gameManager;
                        childUnit.group = newUnitGroup;
                        newUnitGroup.units.Add(childUnit);
                        childUnit.transform.position = gameManager.spriteManager.GetWorldPosition(prefabUnitTerrain.unitPositions[positionIndex]);
                        positionIndex += 1;

                    }
                }
                else
                {
                    Unit newHero = Instantiate(resourceManager.heroes[enemyUnit.unitData.heroIndex]);
                    newHero.name += " " + unitNameIndex;
                    unitNameIndex++;
                    newHero.gameManager = gameManager;
                    newHero.team = Team.Team4;
                    newHero.transform.SetParent(gameObject.transform);
                    newHero.transform.position = gameManager.spriteManager.GetWorldPosition(prefabUnitTerrain.unitPositions[positionIndex]);
                    positionIndex += 1;
                }
            }
        }
    }

    private int LoadPlayerUnitSuperGroupData(battleLineData backLineUnit, int positionIndex)
    {
        int newPositionIndex = positionIndex;
        if (backLineUnit.unitGroupData.mercenaryIndex != -1)
        {
            UnitGroup newUnitGroup = Instantiate(resourceManager.mercenaries[backLineUnit.unitGroupData.mercenaryIndex]);
            newUnitGroup.gameManager = gameManager;
            for (int j = 0; j < newUnitGroup.transform.childCount; j++)
            {
                Unit childUnit = newUnitGroup.transform.GetChild(j).GetComponent<Unit>();
                childUnit.name += " " + unitNameIndex;
                unitNameIndex++;
                childUnit.team = Team.Player;
                childUnit.gameManager = gameManager;
                childUnit.group = newUnitGroup;
                newUnitGroup.units.Add(childUnit);
                childUnit.transform.position = gameManager.spriteManager.GetWorldPosition(prefabUnitTerrain.unitPositions[positionIndex]);
                newPositionIndex += 1;
            }
            newUnitGroup.team = Team.Player;
            return newPositionIndex;
        }
        else
        {
            unitLoadoutData hero = backLineUnit.unitData;
            Unit newHero = Instantiate(resourceManager.emptyHero);
            newHero.name += " " + unitNameIndex;
            unitNameIndex++;
            newHero.gameManager = gameManager;
            newHero.team = Team.Player;
            newHero.transform.SetParent(gameObject.transform);
            newHero.unitClass = resourceManager.job[hero.jobIndex];

            for (int j = 0; j < hero.skillTree1Branch1Unlocks.Count; j++)
            {
                if (hero.skillTree1Branch1Unlocks[j])
                {
                    newHero.unitClass.skillTree1.branch1.BranchSkills[j].UnlockSkill(newHero);
                }
            }

            for (int j = 0; j < hero.skillTree1Branch2Unlocks.Count; j++)
            {
                if (hero.skillTree1Branch2Unlocks[j])
                {
                    newHero.unitClass.skillTree1.branch2.BranchSkills[j].UnlockSkill(newHero);
                }
            }

            for (int j = 0; j < hero.skillTree2Branch1Unlocks.Count; j++)
            {
                if (hero.skillTree2Branch1Unlocks[j])
                {
                    newHero.unitClass.skillTree2.branch1.BranchSkills[j].UnlockSkill(newHero);
                }
            }

            for (int j = 0; j < hero.skillTree2Branch2Unlocks.Count; j++)
            {
                if (hero.skillTree2Branch2Unlocks[j])
                {
                    newHero.unitClass.skillTree2.branch2.BranchSkills[j].UnlockSkill(newHero);
                }
            }

            if (hero.helmetIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.helmetIndex];
                newHero.helmet = item;
            }
            if (hero.armorIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.armorIndex];
                newHero.armor = item;
            }
            if (hero.bootsIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.bootsIndex];
                newHero.legs = item;
            }
            if (hero.mainHandIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.mainHandIndex];
                newHero.mainHand = item;
            }
            if (hero.offHandIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.offHandIndex];
                newHero.offHand = item;
            }
            if (hero.item1Index != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.item1Index];
                newHero.Item1 = item;
            }
            if (hero.item2Index != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.item2Index];
                newHero.Item2 = item;
            }
            if (hero.item3Index != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.item3Index];
                newHero.Item3 = item;
            }
            if (hero.item4Index != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.item4Index];
                newHero.Item4 = item;
            }
            if (hero.backUpMainHandIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.backUpMainHandIndex];
                newHero.backUpMainHand = item;
            }
            if (hero.backUpOffHandIndex != -1)
            {
                EquipableItemSO item = resourceManager.allItems[hero.backUpOffHandIndex];
                newHero.backUpOffHand = item;
            }
            newHero.transform.position = gameManager.spriteManager.GetWorldPosition(prefabUnitTerrain.unitPositions[positionIndex]);
            newPositionIndex += 1;
            return newPositionIndex;
        }
    }

    public void SaveTerrain()
    {
        prefabTerrain.terrainElevation = new List<Vector3Int>();
        for (int i = 0; i < gameManager.spriteManager.elevationOfHexes.GetLength(1); i++)
        {
            for (int j = 0; j < gameManager.spriteManager.elevationOfHexes.GetLength(0); j++)
            {
                prefabTerrain.terrainElevation.Add(new Vector3Int(j, i, gameManager.spriteManager.elevationOfHexes[j, i]));
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(prefabTerrain);
        AssetDatabase.SaveAssets();
    }

    public void DisplayTerrain()    
    {

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TestHexGrid))]
public class TestGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TestHexGrid me = (TestHexGrid)target;
        if (GUILayout.Button("Save Terrain"))
        {
            me.SaveTerrain();
        }
        if (GUILayout.Button("Display Terrain"))
        {
            me.DisplayTerrain();
        }
        DrawDefaultInspector();
    }
}
#endif
