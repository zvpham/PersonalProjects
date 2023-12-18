using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;



public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public MainGameManger mainGameManger;

    public int mapWidth;
    public int mapHeight;
    public Vector3 defaultGridPosition;

    public List<double> speeds;
    public List<int> priority;
    public List<int> statusPriority;
    public List<int> statusDuration;

    public List<int> createdFieldPriority;
    public List<int> createdFieldDuration;

    public List<int> animatedFieldPriority;

    public int baseTurnTime = 500;
    public int worldPriority = 0;

    public Tilemap groundTilemap;

    public MapGenerator mapGenerator;

    public ResourceManager resourceManager;

    public Grid<Unit> grid;
    public Grid<Unit> flyingGrid;
    public Grid<List<Item>> itemgrid;
    public Grid<Wall> obstacleGrid;

    public List<Unit> units;
    public List<Item> items;
    public List<Status> allStatuses;
    public List<CreatedField> createdFields = new List<CreatedField>();
    public List<AnimatedField> animatedFields = new List<AnimatedField>();

    public int least;
    public int index = 0;
    private bool aUnitActed = false;
    // during turn 0 = no; 1 = yes
    private int duringTurn = 0;

    public int numberOfStatusRemoved = 0;

    public float secSpriteChangeSpeed;
    private float currentTime = 0;

    public float expectedLocationChangeSpeed;
    public float currentExpectedLoactionChangeSpeed;
    public int isLocationChangeStatus = 0;
    public ExpectedLocationMarker expectedLocationMarker;
    public List<int> expectedLocationChangeList = new List<int>();
    public List<Unit> unitWhoHaveLocationChangeStatus = new List<Unit>();

    public List<List<List<AnimatedField.Node>>> expectedBlastPaths = new List<List<List<AnimatedField.Node>>>();
    public List<int> expectedBlastRowNumber = new List<int>();

    public bool isNewSlate = true;

    public List<Tuple<int, int, int>> initalRenderLocations;
    public List<Tuple<int, int, int>> finalRenderLocations;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        grid = new Grid<Unit>(mapWidth, mapHeight, 1f, defaultGridPosition, (Grid<Unit> g, int x, int y) => null);
        flyingGrid = new Grid<Unit>(mapWidth, mapHeight, 1f, defaultGridPosition, (Grid<Unit> g, int x, int y) => null);
        itemgrid = new Grid<List<Item>>(mapWidth, mapHeight, 1f, defaultGridPosition, (Grid<List<Item>> g, int x, int y) => null);
        obstacleGrid = new Grid<Wall>(mapWidth, mapHeight, 1f, defaultGridPosition, (Grid<Wall> g, int x, int y) => null);
    }

    void Start()
    {
        currentExpectedLoactionChangeSpeed = expectedLocationChangeSpeed;
        expectedLocationMarker.selfDestructionTimer = expectedLocationChangeSpeed;
        for(int i = 0; i < initalRenderLocations.Count; i++)
        {
            int x = initalRenderLocations[i].Item1;
            int y = initalRenderLocations[i].Item2;
            int wallIndex = initalRenderLocations[i].Item3;
            RenderWall(x, y, wallIndex);
        }
        FinalRender();
        initalRenderLocations = null;
        finalRenderLocations = null;
    }

    public void ChangeUnits(Vector3 worldPosition, Unit unit, bool isFlying = false)
    {
        if(isFlying)
        {
            flyingGrid.SetGridObject(worldPosition, unit);
        }
        else
        {
            grid.SetGridObject(worldPosition, unit);
        }
        mainGameManger.UpdatePathFindingGrid(worldPosition, obstacleGrid, grid);
    }

    public void ChangeWalls(Vector3 worldPosition, Wall wall)
    {
        obstacleGrid.SetGridObject(worldPosition, wall);
        mainGameManger.UpdatePathFindingGrid(worldPosition, obstacleGrid, grid);
    }

    public void ClearBoard()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadData(TileData data)
    {
        currentExpectedLoactionChangeSpeed = expectedLocationChangeSpeed;
        expectedLocationMarker.selfDestructionTimer = expectedLocationChangeSpeed;
        if (data == null)
        {
            Debug.LogError("current Tile Data Not Found HELP PLEASE");
            return;
        }
        if (data.unitPrefabDatas.Count == 0)
        {
            isNewSlate = true;
            GameObject temp = Instantiate(resourceManager.unitPrefabs[0], new Vector3(100, 45, 0), new Quaternion(0, 0, 0, 1f));
            return;
        }
        else
        {
            isNewSlate = false;
        }
        // Load Game Manager Data
        this.numberOfStatusRemoved = data.numberOfStatusRemoved;
        this.least = data.least;
        this.index = data.index;
        this.aUnitActed = data.aUnitActed;
        this.duringTurn = data.duringTurn;

        // Load Unit Data
        for (int i = 0; i < data.numberOfUnits; i++)
        {
            unitPrefabData tempData = data.unitPrefabDatas[i];
            GameObject temp = Instantiate(resourceManager.unitPrefabs[tempData.unitPrefabIndex], tempData.position, new Quaternion(0, 0, 0, 1f));
            Unit unit = temp.GetComponent<Unit>();
            unit.gameManager = this;
            this.units.Add(unit);
            unit.health = tempData.health;
            unit.actionNamesForCoolDownOnLoad = tempData.actionNames;
            unit.currentCooldownOnLoad = tempData.actionCooldowns;
            unit.forcedMovementPathData = tempData.forcedMovementPathData;
        }
        this.speeds = data.speeds;
        this.priority = data.priority;

        // Load Player Specific Data
        Player playerTemp = (Player)this.units[0];
        playerTemp.soulSlotIndexes = data.soulSlotIndexes;

        List<SoulItemSO> souls = new List<SoulItemSO>();
        foreach (int soulIndex in data.soulIndexes)
        {
            souls.Add(resourceManager.souls[soulIndex]);
        }
        playerTemp.onLoadSouls = souls;

        // Load Item Data
        for (int i = 0; i < data.itemIndexes.Count; i++)
        {
            GameObject tempItem = Instantiate(resourceManager.genericItemPrefab, data.itemLocations[i], new Quaternion(0, 0, 0, 1f));
            Item item = tempItem.GetComponent<Item>();
            item.inventoryItem = resourceManager.items[data.itemIndexes[i]];
            item.quantity = data.itemQuantities[i];
        }

        // Load Status Data
        this.statusPriority = data.statusPriority;
        this.statusDuration = data.statusDuration;
        for (int i = 0; i < data.statusPrefabIndex.Count; i++)
        {
            Status temp = Instantiate(resourceManager.statuses[data.statusPrefabIndex[i]]);
            temp.statusIntData = data.statusIntData[i];
            temp.statusStringData = data.statusStringData[i];
            temp.statusBoolData = data.statusBoolData[i];
            temp.onLoadApply(this.units[data.indexOfUnitThatHasStatus[i]]);
        }

        // Load Created Field Data;
        this.createdFieldPriority = data.createdFieldPriority;
        this.createdFieldDuration = data.createdFieldDuration;
        for (int i = 0; i < data.createdFieldsData.Count; i++)
        {
            if (data.createdFieldsData[i].fromAnimatedField)
            {
                this.createdFields.Add(null);
                continue;
            }
            CreatedField tempCreatedField = Instantiate(resourceManager.createdFields[data.createdFieldsData[i].createdFieldTypeIndex]);
            tempCreatedField.nonStandardDuration = data.createdFieldsData[i].nonStandardDuration;

            if (data.createdFieldsData[i].createdWithBlastRadius)
            {
                tempCreatedField.CreateGridOfObjects(this, data.createdFieldsData[i].originPosition, data.createdFieldsData[i].fieldRadius, 20, true);
            }
        }

        // Load Animated Field data
        this.animatedFieldPriority = data.animatedFieldPriority;
        for (int i = 0; i < data.animatedFields.Count; i++)
        {
            GameObject tempAnimatedField = Instantiate(resourceManager.animatedFields[data.animatedFields[i].animatedFieldTypeIndex]);
            tempAnimatedField.GetComponent<AnimatedField>().onLoad(data.animatedFields[i], resourceManager);
        }

        // Load Save Data Changes
        if (data.saveToDelete != null && !data.saveToDelete.Equals(""))
        {
            Debug.Log(data.saveToDelete);
            DataPersistenceManager dataPersistenceManager = DataPersistenceManager.Instance;
            dataPersistenceManager.DeleteUserData(data.saveToDelete);
        }
    }

    // Important Make sure you are assigning each value to GameData not adding to a list
    // will cause error in saves which will cause more entities than intended to be added
    public void SaveData(TileData data)
    {
        Debug.Log("Saving Game");
        // Game Manager Data
        data.numberOfStatusRemoved = this.numberOfStatusRemoved;
        data.least = least;
        data.index = index;
        data.aUnitActed = aUnitActed;
        data.duringTurn = duringTurn;

        // Unit data
        data.numberOfUnits = this.units.Count;
        data.speeds = this.speeds;
        data.priority = this.priority;
        List<unitPrefabData> tempUnitPrefabData = new List<unitPrefabData>();
        foreach (Unit unit in this.units)
        {
            List<int> actionCooldowns = new List<int>();
            List<ActionName> actionNames = new List<ActionName>();
            foreach (Action action in unit.actions)
            {
                actionCooldowns.Add(action.currentCooldown);
                actionNames.Add(action.actionName);
            }
            unitPrefabData temp = new unitPrefabData(unit.gameObject.transform.position, unit.unitResourceManagerIndex, unit.health,
                actionCooldowns, actionNames, unit.forcedMovementPathData);
            tempUnitPrefabData.Add(temp);
        }
        data.unitPrefabDatas = tempUnitPrefabData;

        // Player Specific Data
        Player player = (Player)units[0];
        List<int> tempOnLoadSouls = new List<int>();
        foreach (SoulItemSO soul in player.onLoadSouls)
        {
            tempOnLoadSouls.Add(resourceManager.souls.IndexOf(soul));
        }
        data.soulSlotIndexes = player.soulSlotIndexes;
        data.soulIndexes = tempOnLoadSouls;

        // Item Data
        List<int> itemIndexes = new List<int>();
        List<int> itemQuantities = new List<int>();
        List<Vector3> itemPositions = new List<Vector3>();
        foreach (Item item in items)
        {
            itemIndexes.Add(resourceManager.items.IndexOf(item.inventoryItem));
            itemQuantities.Add(item.quantity);
            itemPositions.Add(item.gameObject.transform.position);
        }
        data.itemIndexes = itemIndexes;
        data.itemQuantities = itemQuantities;
        data.itemLocations = itemPositions;

        // Status Data
        List<int> statusIndexList = new List<int>();
        List<int> indexOfUnitThatHasStatus = new List<int>();
        List<int> statusIntData = new List<int>();
        List<string> statusStringData = new List<string>();
        List<bool> statusBoolData = new List<bool>();
        foreach (Status status in allStatuses)
        {
            statusIndexList.Add(status.statusPrefabIndex);
            indexOfUnitThatHasStatus.Add(this.units.IndexOf(status.targetUnit));
            statusIntData.Add(status.statusIntData);
            statusStringData.Add(status.statusStringData);
            statusBoolData.Add(status.statusBoolData);
        }
        data.statusPrefabIndex = statusIndexList;
        data.indexOfUnitThatHasStatus = indexOfUnitThatHasStatus;
        data.statusPriority = this.statusPriority;
        data.statusDuration = this.statusDuration;
        data.statusIntData = statusIntData;
        data.statusStringData = statusStringData;
        data.statusBoolData = statusBoolData;

        // Created Field Data
        List<createdFieldData> createdFieldLists = new List<createdFieldData>();
        foreach (CreatedField createdField in createdFields)
        {
            createdFieldData tempCreatedField = new createdFieldData();
            tempCreatedField.createdFieldTypeIndex = createdField.createdFieldTypeIndex;
            tempCreatedField.createdFieldQuickness = createdField.createdFieldQuickness;
            tempCreatedField.createdObjectPositions = createdField.createdObjectPositions;

            tempCreatedField.createdWithBlastRadius = createdField.createdWithBlastRadius;
            tempCreatedField.fromAnimatedField = createdField.fromAnimatedField;
            tempCreatedField.nonStandardDuration = createdField.nonStandardDuration;

            tempCreatedField.originPosition = createdField.originPosition;
            tempCreatedField.fieldRadius = createdField.fieldRadius;

            createdFieldLists.Add(tempCreatedField);
        }
        data.createdFieldsData = createdFieldLists;
        data.createdFieldPriority = this.createdFieldPriority;
        data.createdFieldDuration = this.createdFieldDuration;


        // Animated Field Data
        data.animatedFieldPriority = animatedFieldPriority;
        List<animatedFieldData> animatedFieldDataList = new List<animatedFieldData>();

        foreach (AnimatedField animatedField in animatedFields)
        {
            animatedFieldData tempAnimatedField = new animatedFieldData();
            tempAnimatedField.animatedFieldTypeIndex = animatedField.animatedFieldTypeIndex;
            tempAnimatedField.animatedCreatedFieldTypeIndex = resourceManager.createdFields.IndexOf(animatedField.createdFieldType);
            tempAnimatedField.createdObjectIndex = resourceManager.createdObjects.IndexOf(animatedField.createdObject);
            tempAnimatedField.createdObjectHolderIndex = animatedField.createdObjectHolder.GetComponent<CreatedObjectHolder>().CreatedObjectIndex;
            tempAnimatedField.createdFieldQuickness = animatedField.createdFieldQuickness;
            tempAnimatedField.startPosition = animatedField.startPosition;
            tempAnimatedField.initialDirection = animatedField.initialDirection;
            tempAnimatedField.range = animatedField.range;
            tempAnimatedField.angle = animatedField.angle;
            tempAnimatedField.maxUnitBlastValueAbsorbtion = animatedField.maxUnitBlastValueAbsorbtion;
            tempAnimatedField.maxObstacleBlastValueAbsorbtion = animatedField.maxObstacleBlastValueAbsorbtion;
            tempAnimatedField.affectFlying = animatedField.affectFlying;
            tempAnimatedField.ignoreWalls = animatedField.ignoreWalls;
            tempAnimatedField.IsSlowInTimeFlow = animatedField.IsSlowInTimeFlow;

            List<animatedFieldNodeData> slowedNodeList = new List<animatedFieldNodeData>();
            foreach (AnimatedField.Node node in animatedField.slowedNodeList)
            {
                animatedFieldNodeData tempNode = new animatedFieldNodeData();
                tempNode.position = node.position;
                tempNode.direction = node.direction;
                tempNode.priority = node.priority;
                tempNode.blastValue = node.blastValue;
                tempNode.blastSpeed = node.blastSpeed;
                tempNode.affectedTimeFlow = node.affectedTimeFlow;
                slowedNodeList.Add(tempNode);
            }
            tempAnimatedField.slowedNodeList = slowedNodeList;
            animatedFieldDataList.Add(tempAnimatedField);
        }
        data.animatedFields = animatedFieldDataList;
    }

    public void FreezeTile()
    {
        for (int i = 0; i < allStatuses.Count; i++)
        {
            numberOfStatusRemoved = 0;
            if (i < 0)
            {
                break;
            }
            Unit tempUnit = allStatuses[i].targetUnit;
            int tempIndex = tempUnit.statusDuration.Count;
            allStatuses[i].RemoveEffect(tempUnit);
            i -= numberOfStatusRemoved;
            numberOfStatusRemoved = 0;
        }
    }

    // Update is called once per frame 
    void Update()
    {
        // Adds a phantome image of things that have a set or predicted path
        currentTime += Time.deltaTime;

        if (currentTime >= currentExpectedLoactionChangeSpeed)
        {
            if (isLocationChangeStatus >= 1)
            {
                ExpectedLocationSpriteManager();
            }

            if (expectedBlastPaths.Count > 0)
            {
                ExpectedBlastManager();
            }
            currentExpectedLoactionChangeSpeed += expectedLocationChangeSpeed;
        }
        // Changes Sprites of all units based on statuses they have independent of Turns
        if (currentTime >= secSpriteChangeSpeed)
        {
            SpriteChangeManager();
        }

        if (CanContinue(units[index]))
        {
            // finds the lowest priority amongst all the units, statuses, worldtimer
            // if we are at the top of a turn
            if (duringTurn == 0)
            {
                least = priority[0];
                for (int i = 0; i < priority.Count; i++)
                {
                    if (priority[i] < least)
                    {
                        least = priority[i];
                    }
                }
                if (allStatuses.Count > 0)
                {
                    for (int i = 0; i < statusPriority.Count; i++)
                    {
                        if (statusPriority[i] < least)
                        {
                            least = statusPriority[i];
                        }
                    }
                }

                if (createdFieldPriority.Count > 0)
                {
                    for (int i = 0; i < createdFieldPriority.Count; i++)
                    {
                        if (createdFieldPriority[i] < least)
                        {
                            least = createdFieldPriority[i];
                        }

                    }
                }

                if (animatedFieldPriority.Count > 0)
                {
                    for (int i = 0; i < animatedFieldPriority.Count; i++)
                    {
                        if (animatedFieldPriority[i] < least)
                        {
                            least = animatedFieldPriority[i];
                        }

                    }
                }
            }

            //lowers priority of a unit by the least amount of priority 
            // if priority is 0 activate unit and end loop
            // if mid turn will resume list one more than unit that just went
            aUnitActed = false;
            for (int i = index + duringTurn; i < priority.Count;)
            {
                priority[i] = priority[i] - least;

                if ((int)priority[i] == 0)
                {
                    index = i;
                    duringTurn = 1;
                    units[i].enabled = true;
                    aUnitActed = true;
                    break;
                }
                else if (i == 0)
                {
                    duringTurn = 1;
                    i++;
                }
                else
                {
                    index = i;
                    i++;
                }
            }


            //end turn reset all turn variables and reset priority of units who acted
            if (index + duringTurn == priority.Count && !aUnitActed)
            {
                index = 0;
                duringTurn = 0;
                for (int i = 0; i < priority.Count; i++)
                {
                    if (priority[i] <= 0)
                    {
                        priority[i] = (int)(baseTurnTime * speeds[i]);
                    }
                }

                if (allStatuses.Count > 0)
                {
                    numberOfStatusRemoved = 0;
                    for (int i = 0; i < statusPriority.Count; i++)
                    {
                        if (i < 0)
                        {
                            break;
                        }
                        if (allStatuses[i].ApplyEveryTurn && allStatuses[i].isFirstWorldTurn)
                        {
                            allStatuses[i].isFirstWorldTurn = false;
                        }
                        else
                        {
                            statusPriority[i] -= least;
                            if (statusPriority[i] <= 0)
                            {
                                statusPriority[i] = (int)(allStatuses[i].statusQuickness * baseTurnTime);
                                Unit tempUnit = allStatuses[i].targetUnit;
                                int tempIndex = tempUnit.statusDuration.Count;

                                //reduces status duration of a status if it is supposed to go down at the end of a turn
                                if (!allStatuses[i].nonStandardDuration)
                                {
                                    statusDuration[i] -= 1;
                                }

                                // if an affect applyies everyturn apply the affect if it isn't the  turn it was activated

                                if (allStatuses[i].ApplyEveryTurn)
                                {
                                    allStatuses[i].ApplyEffect(tempUnit);
                                }

                                //if a status was removed in previous step reset sprite of unit otherwise if a status duration is 0 remove the status from the unit and reset the units sprite
                                if (tempUnit.statusDuration.Count != tempIndex || statusDuration[i] <= 0)
                                {
                                    if (tempUnit.statusDuration.Count == tempIndex)
                                    {
                                        allStatuses[i].RemoveEffect(tempUnit);
                                    }
                                    tempUnit.ChangeSprite(tempUnit.originalSprite);
                                    tempUnit.spriteIndex = -1;

                                }
                                i -= numberOfStatusRemoved;
                                numberOfStatusRemoved = 0;
                            }
                        }
                    }
                }

                // change Status Priority at top of turn
                if (createdFieldPriority.Count > 0)
                {
                    for (int i = 0; i < createdFieldPriority.Count; i++)
                    {
                        createdFieldPriority[i] -= least;
                        if (createdFieldPriority[i] <= 0)
                        {
                            createdFieldPriority[i] = (int)(createdFields[i].createdFieldQuickness * baseTurnTime);

                            //reduces status duration of a status if it is supposed to go down at the end of a turn
                            if (!createdFields[i].nonStandardDuration)
                            {
                                createdFieldDuration[i] -= 1;
                            }

                            //if a status was removed in previous step reset sprite of unit otherwise if a status duration is 0 remove the status from the unit and reset the units sprite
                            if (createdFieldDuration[i] <= 0)
                            {
                                createdFields[i].RemoveStatusOnDeletion();
                            }
                        }
                    }
                }

                if (animatedFieldPriority.Count > 0)
                {
                    for (int i = 0; i < animatedFieldPriority.Count; i++)
                    {
                        animatedFieldPriority[i] -= least;
                        if (animatedFieldPriority[i] <= 0)
                        {
                            animatedFieldPriority[i] = (int)(animatedFields[i].createdFieldQuickness * baseTurnTime);

                            animatedFields[i].Activate();
                            //Animated Fields Handle thier Own Deletion
                        }
                    }
                }
            }
        }
    }


    private bool CanContinue(MonoBehaviour script)
    {
        return !script.isActiveAndEnabled;
    }

    private void ExpectedLocationSpriteManager()
    {
        foreach (Unit unit in units)
        {
            if (unit.hasLocationChangeStatus >= 1)
            {
                if (!unitWhoHaveLocationChangeStatus.Contains<Unit>(unit))
                {
                    unitWhoHaveLocationChangeStatus.Add(unit);
                    expectedLocationChangeList.Add(0);
                }

                int i = unitWhoHaveLocationChangeStatus.IndexOf(unit);
                if (!(expectedLocationChangeList[i] + unit.forcedMovementPathData.currentPathIndex >
                    unit.forcedMovementPathData.forcedMovementPath.Count - 1))
                {
                    GameObject temp = Instantiate(expectedLocationMarker.gameObject);
                    temp.GetComponent<SpriteRenderer>().sprite = unit.originalSprite;
                    temp.transform.position = unit.forcedMovementPathData.forcedMovementPath
                        [unit.forcedMovementPathData.currentPathIndex + expectedLocationChangeList[i]];
                }
                expectedLocationChangeList[i] += 1;
                if (expectedLocationChangeList[i] + unit.forcedMovementPathData.currentPathIndex
                   > unit.forcedMovementPathData.forcedMovementPath.Count)
                {
                    expectedLocationChangeList[i] = 0;
                }
            }
        }
    }

    private void ExpectedBlastManager()
    {
        for (int individualBlastIndex = 0; individualBlastIndex < expectedBlastPaths.Count; individualBlastIndex++)
        {
            int rowIndex = expectedBlastRowNumber[individualBlastIndex];
            List<GameObject> blastmarkerList = new List<GameObject>();
            for (int markerIndex = 0; markerIndex < expectedBlastPaths[individualBlastIndex][rowIndex].Count; markerIndex++)
            {
                AnimatedField.Node node = expectedBlastPaths[individualBlastIndex][rowIndex][markerIndex];
                GameObject temp = Instantiate(expectedLocationMarker.gameObject);
                temp.GetComponent<SpriteRenderer>().sprite = node.createdObject.createdObjectSpritePrefab.GetComponent<SpriteRenderer>().sprite;
                temp.transform.position = node.position;
            }
            expectedBlastRowNumber[individualBlastIndex] += 1;
            if (expectedBlastRowNumber[individualBlastIndex] >= expectedBlastPaths[individualBlastIndex].Count())
            {
                expectedBlastRowNumber[individualBlastIndex] = 0;
            }
        }
    }

    private void SpriteChangeManager()
    {
        foreach (Unit unit in units)
        {
            currentExpectedLoactionChangeSpeed = expectedLocationChangeSpeed;
            if (unit.statuses.Count != 0)
            {
                unit.spriteIndex += 1;
                if (unit.spriteIndex == unit.statuses.Count)
                {
                    unit.spriteIndex = -1;
                    unit.ChangeSprite(unit.originalSprite);
                }
                else
                {
                    if ((unit.spriteIndex < unit.statuses.Count))
                    {
                        unit.ChangeSprite(unit.statuses[unit.spriteIndex].statusImage);
                    }
                }
            }
            else
            {
                unit.ChangeSprite(unit.originalSprite);
            }
        }
        currentTime = 0;
    }

    public void FinalRender()
    {
        int x = -1;
        int y = -1;
        int wallIndex = -1;
        for (int i = 0; i < finalRenderLocations.Count; i++)
        {
            x = finalRenderLocations[i].Item1;
            y = finalRenderLocations[i].Item2;
            wallIndex = finalRenderLocations[i].Item3;
            switch (obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState)
            {
                case (WallStates.EastWall):
                    if (obstacleGrid.GetGridObject(x - 1, y).GetComponent<PrefabMapTile>().wallState == WallStates.CenterHorzontalWall ||
                       obstacleGrid.GetGridObject(x - 1, y).GetComponent<PrefabMapTile>().wallState == WallStates.LeftHorzontalWall)
                    {
                        obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].EastThreeWay;
                    }
                    else if (obstacleGrid.GetGridObject(x - 1, y + 1) == null)
                    {
                        obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].EastModifiedLowerConnector;
                    }
                    else if (obstacleGrid.GetGridObject(x - 1, y - 1) == null)
                    {
                        obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].EastModifiedUpperConnector;
                    }
                    break;
                case (WallStates.WestWall):
                    if (obstacleGrid.GetGridObject(x + 1, y).GetComponent<PrefabMapTile>().wallState == WallStates.CenterHorzontalWall ||
                        obstacleGrid.GetGridObject(x + 1, y).GetComponent<PrefabMapTile>().wallState == WallStates.RightHorzontalWall)
                    {
                        obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].WestThreeWay;
                    }
                    else if (obstacleGrid.GetGridObject(x + 1, y + 1) == null)
                    {
                        obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].WestModifiedLowerConnector;
                    }
                    else if (obstacleGrid.GetGridObject(x + 1, y - 1) == null)
                    {
                        obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].WestModifiedUpperConnector;
                    }
                    break;
                case (WallStates.SouthWall):
                    if (obstacleGrid.GetGridObject(x + 1, y + 1) == null ||
                        obstacleGrid.GetGridObject(x - 1, y + 1) == null)
                    {
                        if (obstacleGrid.GetGridObject(x + 1, y + 1) == null &&
                        obstacleGrid.GetGridObject(x - 1, y + 1) == null)
                        {
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthWallThreeWay;
                        }
                        else if (obstacleGrid.GetGridObject(x + 1, y + 1) == null)
                        {
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthModifiedRightSmallConnector;
                        }
                        else
                        {
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthModifiedLeftSmallConnector;
                        }
                    }
                    break;
                case (WallStates.NorthWall):
                    if (obstacleGrid.GetGridObject(x + 1, y - 1) == null ||
                        obstacleGrid.GetGridObject(x - 1, y - 1) == null)
                    {
                        if (obstacleGrid.GetGridObject(x + 1, y - 1) == null &&
                        obstacleGrid.GetGridObject(x - 1, y - 1) == null)
                        {
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthWallThreeWay;
                        }
                        else if (obstacleGrid.GetGridObject(x + 1, y - 1) == null)
                        {
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthModifiedRightSmallConnector;
                        }
                        else
                        {
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthModifiedLeftSmallConnector;
                        }
                    }
                    break;
                case (WallStates.CenterWall):
                    int numWallConnections = 0;

                    if (obstacleGrid.GetGridObject(x + 1, y + 1) != null)
                    {
                        //NorthEast Wall
                        numWallConnections += 1;
                    }
                    if (obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                    {
                        //SouthWest Wall
                        numWallConnections += 2;
                    }
                    if (obstacleGrid.GetGridObject(x - 1, y + 1) != null)
                    {
                        //is NorthWest Wall
                        numWallConnections += 4;
                    }
                    if (obstacleGrid.GetGridObject(x + 1, y - 1) != null)
                    {
                        //is SouthEast
                        numWallConnections += 8;
                    }
                    switch (numWallConnections)
                    {
                        case 0:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterFourWay;
                            break;
                        case 1:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthEastConnector;
                            break;
                        case 2:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterSouthWestConnector;
                            break;
                        case 3:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthEastToSouthWest;
                            break;
                        case 4:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthWestConnector;
                            break;
                        case 5:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthThreeWay;
                            break;
                        case 6:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterWestThreeWay;
                            break;
                        case 7:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthSmallRightConnectorWall;
                            break;
                        case 8:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterSouthEastConnector;
                            break;
                        case 9:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterEastThreeWay;
                            break;
                        case 10:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterSouthThreeWay;
                            break;
                        case 11:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterSouthSmallLeftConnectorWall;
                            break;
                        case 12:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthWestToSouthEast;
                            break;
                        case 13:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthSmallLeftConnectorWall;
                            break;
                        case 14:
                            obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterSouthSmallRightConnectorWall;
                            break;
                        case 15:
                            break;
                    }
                    break;

            }
        }

    }

    public void RenderWall(int x, int y, int wallIndex)
    {
        int numWallConnections = 0;
        if (x + 1 < mapWidth && obstacleGrid.GetGridObject(x + 1, y) != null)
        {
            //East Wall
            numWallConnections += 1;
        }
        if (x - 1 >= 0 && obstacleGrid.GetGridObject(x - 1, y) != null)
        {
            //West Wall
            numWallConnections += 2;
        }
        if (y + 1 < mapHeight && obstacleGrid.GetGridObject(x, y + 1) != null)
        {
            //is NorthWall
            numWallConnections += 4;
        }
        if (y - 1 >= 0 && obstacleGrid.GetGridObject(x, y - 1) != null)
        {
            //is SouthWall
            numWallConnections += 8;
        }

        switch (numWallConnections)
        {
            case 0:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SoleWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.SoleWall;
                break;
            case 1:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].LeftHorzontalWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.LeftHorzontalWall;
                finalRenderLocations.Add(new Tuple<int, int, int>(x + 1, y, wallIndex));
                break;
            case 2:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].RightHorzontalWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.RightHorzontalWall;
                finalRenderLocations.Add(new Tuple<int, int, int>(x - 1, y, wallIndex));
                break;
            case 3:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterHorzontalWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.CenterHorzontalWall;
                finalRenderLocations.Add(new Tuple<int, int, int>(x - 1, y, wallIndex));
                finalRenderLocations.Add(new Tuple<int, int, int>(x + 1, y, wallIndex));
                break;
            case 4:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].LowerVerticalWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.LowerVerticalWall;
                finalRenderLocations.Add(new Tuple<int, int, int>(x, y + 1, wallIndex));
                break;
            case 5:
                if (obstacleGrid.GetGridObject(x + 1, y + 1) != null)
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthWestWall;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.SouthWestWall;
                    if (y - 1 > 0 && obstacleGrid.GetGridObject(x + 1, y - 1) != null)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x + 1, y, wallIndex));
                    }
                }
                else
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthWestWallModified;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.SouthWestWallModified;
                    if (y - 1 > 0 && obstacleGrid.GetGridObject(x + 1, y - 1) != null)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x + 1, y, wallIndex));
                    }
                }
                break;
            case 6:
                if (obstacleGrid.GetGridObject(x - 1, y + 1) != null)
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthEastWall;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.SouthEastWall;
                    if (y - 1 > 0 && obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x - 1, y, wallIndex));
                    }

                }
                else
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthEastWallModified;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.SouthEastWallModified;
                    if (y - 1 > 0 && obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x - 1, y, wallIndex));
                    }
                }
                break;
            case 7:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.SouthWall;
                if (y - 1 < 0)
                {
                    break;
                }
                if (obstacleGrid.GetGridObject(x + 1, y - 1) != null)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x + 1, y, wallIndex));
                }
                if (y + 1 < mapHeight && obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x - 1, y, wallIndex));
                }
                break;
            case 8:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].UpperVerticalWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.UpperVerticalWall;
                finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                break;
            case 9:
                if (obstacleGrid.GetGridObject(x + 1, y - 1) != null)
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthWestWall;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.NorthWestWall;
                    if (x - 1 > 0 && obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                    }
                }
                else
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthWestWallModified;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.NorthWestWallModified;
                    if (x - 1 > 0 && obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                    }
                }
                break;
            case 10:
                if (obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthEastWall;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.NorthEastWall;
                    if (obstacleGrid.GetGridObject(x + 1, y - 1) == null)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                    }
                    if (y + 1 < mapHeight && obstacleGrid.GetGridObject(x - 1, y + 1) != null)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x - 1, y, wallIndex));
                    }
                }
                else
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthEastWallModified;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.NorthEastWallModified;
                    if (x + 1 < mapWidth  && obstacleGrid.GetGridObject(x + 1, y - 1) != null)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                    }
                }
                break;
            case 11:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.NorthWall;
                break;
            case 12:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterVerticalWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.CenterVerticalWall;
                if (x + 1 >= mapWidth  || x - 1 < 0)
                {
                    break;
                }
                if (obstacleGrid.GetGridObject(x - 1, y + 1) != null &&
                    obstacleGrid.GetGridObject(x + 1, y + 1) != null)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x, y + 1, wallIndex));
                }
                if (obstacleGrid.GetGridObject(x - 1, y - 1) != null &&
                    obstacleGrid.GetGridObject(x + 1, y - 1) != null)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                }
                break;
            case 13:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].WestWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.WestWall;
                if (x - 1 > 0 && y - 1 > 0 && obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                }
                if (obstacleGrid.GetGridObject(x + 1, y + 1) == null)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x, y, wallIndex));
                }
                break;
            case 14:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].EastWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.EastWall;
                if (x + 1 < mapWidth  && y - 1 > 0 && obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                }
                if (obstacleGrid.GetGridObject(x - 1, y + 1) == null)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x, y, wallIndex));
                }
                break;
            case 15:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.CenterWall;
                break;
        }
    }
}
