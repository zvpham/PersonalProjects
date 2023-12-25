using Inventory.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;



public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public MainGameManger mainGameManger;

    public bool activeGameManager = false;
    public Vector2Int gameManagerDirection;
    public Vector3 defaultGridPosition;

    public int baseTurnTime = 500;
    public int worldPriority = 0;

    public Tilemap groundTilemap;

    public ResourceManager resourceManager;

    public Grid<Unit> grid;
    public Grid<Unit> flyingGrid;
    public Grid<List<Item>> itemgrid;
    public Grid<Wall> obstacleGrid;

    public List<Wall> walls;
    public List<SetPiece> setPieces;
    public List<Item> items;

    public List<Unit> units;
    public List<Status> allStatuses;
    public List<CreatedField> createdFields = new List<CreatedField>();
    public List<AnimatedField> animatedFields = new List<AnimatedField>();

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

    public List<Vector3> initalRenderLocations;
    public List<Vector3> finalRenderLocations;

    public UnityAction<int> PlayerWent;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        grid = new Grid<Unit>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, defaultGridPosition + new Vector3(-0.5f, -0.5f, 0), (Grid<Unit> g, int x, int y) => null);
        flyingGrid = new Grid<Unit>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, defaultGridPosition + new Vector3(-0.5f, -0.5f, 0), (Grid<Unit> g, int x, int y) => null);
        itemgrid = new Grid<List<Item>>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, defaultGridPosition + new Vector3(-0.5f, -0.5f, 0), (Grid<List<Item>> g, int x, int y) => null);
        obstacleGrid = new Grid<Wall>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, defaultGridPosition + new Vector3(-0.5f, -0.5f, 0), (Grid<Wall> g, int x, int y) => null);
        initalRenderLocations = new List<Vector3>();
        finalRenderLocations = new List<Vector3>();
    }

     public virtual void Start()
    {
        currentExpectedLoactionChangeSpeed = expectedLocationChangeSpeed;
        expectedLocationMarker.selfDestructionTimer = expectedLocationChangeSpeed;
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
        if (data == null)
        {
            Debug.LogError("current Tile Data Not Found HELP PLEASE");
            return;
        }

        if (data.notNewTile)
        {
            isNewSlate = false;
        }
        else
        {
            Debug.Log("New Tile GameManager");
            isNewSlate = true;
            return;
        }
        activeGameManager = true;

        Vector2 newDefaultGridPosition = defaultGridPosition - mainGameManger.centralGameManager.defaultGridPosition;

        // Load Game Manager Data
        this.numberOfStatusRemoved = data.numberOfStatusRemoved;

        // Load Wall Data
        for (int i = 0; i < data.wallIndexes.Count; i++)
        {
            GameObject tempWall = Instantiate(resourceManager.walls[data.wallIndexes[i]], data.wallPositions[i] + newDefaultGridPosition, new Quaternion(0, 0, 0, 1f));
            Wall wall = tempWall.GetComponent<Wall>();
            wall.health = data.wallHealths[i];
            wall.gameManager = this;
        }

        // Load Wall Data
        for (int i = 0; i < data.setPieceIndexes.Count; i++)
        {
            GameObject tempWall = Instantiate(resourceManager.setPiecies[data.setPieceIndexes[i]], data.setPiecePositions[i] + newDefaultGridPosition, new Quaternion(0, 0, 0, 1f));
            SetPiece setPiece = tempWall.GetComponent<SetPiece>();
            setPiece.health = data.setPieceHealths[i];
            setPiece.gameManager = this;
        }

        // Load Item Data
        for (int i = 0; i < data.itemIndexes.Count; i++)
        {
            GameObject tempItem = Instantiate(resourceManager.itemsPrefabs[data.itemIndexes[i]], data.itemLocations[i] + newDefaultGridPosition, new Quaternion(0, 0, 0, 1f));
            Item item = tempItem.GetComponent<Item>();
            item.quantity = data.itemQuantities[i];
            item.gameManager = this;
        }

        // Load Unit Data
        for (int i = 0; i < data.numberOfUnits; i++)
        {
            unitPrefabData tempData = data.unitPrefabDatas[i];
            GameObject temp = Instantiate(resourceManager.unitPrefabs[tempData.unitPrefabIndex], tempData.position + newDefaultGridPosition, new Quaternion(0, 0, 0, 1f));
            Unit unit = temp.GetComponent<Unit>();
            unit.gameManager = this;
            this.units.Add(unit);
            mainGameManger.units.Add(unit);
            unit.health = tempData.health;
            unit.actionNamesForCoolDownOnLoad = tempData.actionNames;
            unit.currentCooldownOnLoad = tempData.actionCooldowns;
            unit.forcedMovementPathData = tempData.forcedMovementPathData;
            unit.priority = tempData.priority;
        }

        // Load Status Data
        for (int i = 0; i < data.statusPrefabIndex.Count; i++)
        {
            Status temp = Instantiate(resourceManager.statuses[data.statusPrefabIndex[i]]);
            temp.statusPriority = data.statusPriority[i];
            temp.currentStatusDuration = data.statusDuration[i];
            temp.statusIntData = data.statusIntData[i];
            temp.statusStringData = data.statusStringData[i];
            temp.statusBoolData = data.statusBoolData[i];
            temp.onLoadApply(this.units[data.indexOfUnitThatHasStatus[i]]);
        }

        // Load Created Field Data;
        for (int i = 0; i < data.createdFieldsData.Count; i++)
        {
            if (data.createdFieldsData[i].fromAnimatedField)
            {
                this.createdFields.Add(null);
                mainGameManger.createdFields.Add(null);
                continue;
            }
            CreatedField tempCreatedField = Instantiate(resourceManager.createdFields[data.createdFieldsData[i].createdFieldTypeIndex]);
            tempCreatedField.createdFieldPriority = data.createdFieldPriority[i];
            tempCreatedField.nonStandardDuration = data.createdFieldsData[i].nonStandardDuration;
            tempCreatedField.currentCreatedFieldDuration = data.createdFieldDuration[i];

            if (data.createdFieldsData[i].createdWithBlastRadius)
            {
                tempCreatedField.CreateGridOfObjects(this, data.createdFieldsData[i].originPosition + newDefaultGridPosition, data.createdFieldsData[i].fieldRadius, 20, true);
            }
        }

        // Load Animated Field data
        for (int i = 0; i < data.animatedFields.Count; i++)
        {
            GameObject tempAnimatedField = Instantiate(resourceManager.animatedFields[data.animatedFields[i].animatedFieldTypeIndex]);
            tempAnimatedField.GetComponent<AnimatedField>().animatedFieldPriority = data.animatedFieldPriority[i];
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
    public void SaveData(TileData data, bool isFrozenZone = false)
    {
        Debug.Log("Saving Game");
        Vector3 newDefaultGridPosition = defaultGridPosition - mainGameManger.centralGameManager.defaultGridPosition;

        // Game Manager Data
        data.numberOfStatusRemoved = this.numberOfStatusRemoved;
        data.notNewTile = true;

        // Wall Data
        List<int> wallIndexes = new List<int>();
        List<int> wallhealths = new List<int>();
        List<Vector2> wallPositions = new List<Vector2>();
        foreach (Wall wall in walls)
        {
            wallIndexes.Add(wall.wallIndex);
            wallhealths.Add(wall.health);
            wallPositions.Add(wall.gameObject.transform.position - newDefaultGridPosition);
        }
        data.wallIndexes = wallIndexes;
        data.wallHealths = wallhealths;
        data.wallPositions = wallPositions;


        // Set Piece Data
        List<int> setPieceIndexes = new List<int>();
        List<int> setPieceHealths = new List<int>();
        List<Vector2> setPiecePositions = new List<Vector2>();
        foreach (SetPiece setPiece in setPieces)
        {
            setPieceIndexes.Add(setPiece.setPieceIndex);
            setPieceHealths.Add(setPiece.health);
            setPiecePositions.Add(setPiece.gameObject.transform.position - newDefaultGridPosition);
        }
        data.setPieceIndexes = setPieceIndexes;
        data.setPieceHealths = setPieceHealths;
        data.setPiecePositions = setPiecePositions;


        // Item Data
        List<int> itemIndexes = new List<int>();
        List<int> itemQuantities = new List<int>();
        List<Vector2> itemPositions = new List<Vector2>();
        foreach (Item item in items)
        {
            itemIndexes.Add(item.inventoryItem.itemIndex);
            itemQuantities.Add(item.quantity);
            itemPositions.Add(item.gameObject.transform.position - newDefaultGridPosition);
        }
        data.itemIndexes = itemIndexes;
        data.itemQuantities = itemQuantities;
        data.itemLocations = itemPositions;

        // Unit data
        if(isFrozenZone)
        {
            data.numberOfUnits = this.units.Count;
            List<unitPrefabData> tempUnitPrefabData = new List<unitPrefabData>();

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                List<int> actionCooldowns = new List<int>();
                List<ActionName> actionNames = new List<ActionName>();
                foreach (Action action in unit.actions)
                {
                    actionCooldowns.Add(action.currentCooldown);
                    actionNames.Add(action.actionName);
                }
                unitPrefabData temp = new unitPrefabData(unit.gameObject.transform.position - newDefaultGridPosition, unit.priority,
                    unit.unitResourceManagerIndex, unit.health, actionCooldowns, actionNames, unit.forcedMovementPathData);
                tempUnitPrefabData.Add(temp);
            }
            data.unitPrefabDatas = tempUnitPrefabData;
        }
        else
        {
            data.numberOfUnits = this.units.Count - 1;
            List<unitPrefabData> tempUnitPrefabData = new List<unitPrefabData>();

            for (int i = 1; i < units.Count; i++)
            {
                Unit unit = units[i];
                List<int> actionCooldowns = new List<int>();
                List<ActionName> actionNames = new List<ActionName>();
                foreach (Action action in unit.actions)
                {
                    actionCooldowns.Add(action.currentCooldown);
                    actionNames.Add(action.actionName);
                }
                unitPrefabData temp = new unitPrefabData(unit.gameObject.transform.position - newDefaultGridPosition, unit.priority,
                    unit.unitResourceManagerIndex, unit.health, actionCooldowns, actionNames, unit.forcedMovementPathData);
                tempUnitPrefabData.Add(temp);
            }
            data.unitPrefabDatas = tempUnitPrefabData;
        }

        // Status Data
        List<int> statusIndexList = new List<int>();
        List<int> indexOfUnitThatHasStatus = new List<int>();
        List<int> statusIntData = new List<int>();
        List<string> statusStringData = new List<string>();
        List<bool> statusBoolData = new List<bool>();
        List<int> newStatusPriority = new List<int>();
        List<int> newStatusDuration = new List<int>();
        if(isFrozenZone)
        {
            for (int i = 0; i < allStatuses.Count; i++)
            {
                Status status = allStatuses[i];
                statusIndexList.Add(status.statusPrefabIndex);
                indexOfUnitThatHasStatus.Add(this.units.IndexOf(status.targetUnit));
                statusIntData.Add(status.statusIntData);
                statusStringData.Add(status.statusStringData);
                statusBoolData.Add(status.statusBoolData);
                newStatusPriority.Add(allStatuses[i].statusPriority);
                newStatusDuration.Add(allStatuses[i].currentStatusDuration);
            }
        }
        else
        {
            for (int i = 0; i < allStatuses.Count; i++)
            {
                Status status = allStatuses[i];
                if (status.targetUnit == units[0])
                {
                    continue;
                }
                statusIndexList.Add(status.statusPrefabIndex);
                indexOfUnitThatHasStatus.Add(this.units.IndexOf(status.targetUnit) - 1);
                statusIntData.Add(status.statusIntData);
                statusStringData.Add(status.statusStringData);
                statusBoolData.Add(status.statusBoolData);
                newStatusPriority.Add(allStatuses[i].statusPriority);
                newStatusDuration.Add(allStatuses[i].currentStatusDuration);
            }
        }

        data.statusPrefabIndex = statusIndexList;
        data.indexOfUnitThatHasStatus = indexOfUnitThatHasStatus;
        data.statusPriority = newStatusPriority;
        data.statusDuration = newStatusDuration;
        data.statusIntData = statusIntData;
        data.statusStringData = statusStringData;
        data.statusBoolData = statusBoolData;

        // Created Field Data
        List<createdFieldData> createdFieldLists = new List<createdFieldData>();
        List<int> createdFieldDurations = new List<int>();
        List<int> createdFieldPriorities = new List<int>();
        foreach (CreatedField createdField in createdFields)
        {
            createdFieldData tempCreatedField = new createdFieldData();
            tempCreatedField.createdFieldTypeIndex = createdField.createdFieldTypeIndex;
            tempCreatedField.createdFieldQuickness = createdField.createdFieldQuickness;
            tempCreatedField.createdObjectPositions = createdField.createdObjectPositions;

            tempCreatedField.createdWithBlastRadius = createdField.createdWithBlastRadius;
            tempCreatedField.fromAnimatedField = createdField.fromAnimatedField;
            tempCreatedField.nonStandardDuration = createdField.nonStandardDuration;

            tempCreatedField.originPosition = createdField.originPosition - newDefaultGridPosition;
            tempCreatedField.fieldRadius = createdField.fieldRadius;
            createdFieldDurations.Add(createdField.currentCreatedFieldDuration);
            createdFieldPriorities.Add(createdField.createdFieldPriority);
            createdFieldLists.Add(tempCreatedField);
        }
        data.createdFieldsData = createdFieldLists;
        data.createdFieldPriority = createdFieldPriorities;
        data.createdFieldDuration = createdFieldDurations;


        // Animated Field Data
        List<animatedFieldData> animatedFieldDataList = new List<animatedFieldData>();
        List<int> animatedFieldPriorities = new List<int>();

        foreach (AnimatedField animatedField in animatedFields)
        {
            animatedFieldData tempAnimatedField = new animatedFieldData();
            tempAnimatedField.animatedFieldTypeIndex = animatedField.animatedFieldTypeIndex;
            tempAnimatedField.animatedCreatedFieldTypeIndex = resourceManager.createdFields.IndexOf(animatedField.createdFieldType);
            tempAnimatedField.createdObjectIndex = resourceManager.createdObjects.IndexOf(animatedField.createdObject);
            tempAnimatedField.createdFieldQuickness = animatedField.createdFieldQuickness;
            tempAnimatedField.startPosition = animatedField.startPosition - newDefaultGridPosition;
            tempAnimatedField.initialDirection = animatedField.initialDirection;
            tempAnimatedField.range = animatedField.range;
            tempAnimatedField.angle = animatedField.angle;
            tempAnimatedField.maxUnitBlastValueAbsorbtion = animatedField.maxUnitBlastValueAbsorbtion;
            tempAnimatedField.maxObstacleBlastValueAbsorbtion = animatedField.maxObstacleBlastValueAbsorbtion;
            tempAnimatedField.affectFlying = animatedField.affectFlying;
            tempAnimatedField.ignoreWalls = animatedField.ignoreWalls;
            tempAnimatedField.IsSlowInTimeFlow = animatedField.IsSlowInTimeFlow;
            animatedFieldPriorities.Add(animatedField.animatedFieldPriority);

            List<animatedFieldNodeData> slowedNodeList = new List<animatedFieldNodeData>();
            foreach (AnimatedField.Node node in animatedField.slowedNodeList)
            {
                animatedFieldNodeData tempNode = new animatedFieldNodeData();
                tempNode.position = node.position - newDefaultGridPosition;
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
        data.animatedFieldPriority = animatedFieldPriorities;
    }

    public void PlayerTookTurn(int playerPriority)
    {
        PlayerWent?.Invoke(playerPriority);
    }

    public void FreezeTile()
    {
        for(int i = animatedFields.Count - 1; i >= 0; i--)
        {
            animatedFields[i].DestroySelf();
        }

        for(int i = createdFields.Count - 1; i >= 0; i--)
        {
            createdFields[i].RemoveStatusOnDeletion();
        }

        for (int i = allStatuses.Count -1 ; i >= 0; i--)
        {
            Unit tempUnit = allStatuses[i].targetUnit;
            allStatuses[i].RemoveEffect(tempUnit);
        }

        createdFields = new List<CreatedField>();
        animatedFields = new List<AnimatedField>();
        allStatuses = new List<Status>();
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
                temp.GetComponent<SpriteRenderer>().sprite = node.createdObjectSprite;
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

    public void StartRender()
    {
        for (int i = 0; i < initalRenderLocations.Count; i++)
        {
            int x = (int)initalRenderLocations[i].x;
            int y = (int)initalRenderLocations[i].y;
            int wallIndex = (int)initalRenderLocations[i].z;
            RenderWall(x, y, wallIndex);
        }
        FinalRender();
        initalRenderLocations = new List<Vector3>();
        finalRenderLocations = new List<Vector3>();
    }

    public void FinalRender()
    {
        int x = -1;
        int y = -1;
        int wallIndex = -1;
        for (int i = 0; i < finalRenderLocations.Count; i++)
        {
            x = (int)finalRenderLocations[i].x;
            y = (int)finalRenderLocations[i].y;
            wallIndex = (int)finalRenderLocations[i].z;
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
        if (x + 1 < mainGameManger.mapWidth && obstacleGrid.GetGridObject(x + 1, y) != null)
        {
            //East Wall
            numWallConnections += 1;
        }
        if (x - 1 >= 0 && obstacleGrid.GetGridObject(x - 1, y) != null)
        {
            //West Wall
            numWallConnections += 2;
        }
        if (y + 1 < mainGameManger.mapHeight && obstacleGrid.GetGridObject(x, y + 1) != null)
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

                finalRenderLocations.Add(new Vector3(x + 1, y, wallIndex));
                break;
            case 2:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].RightHorzontalWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.RightHorzontalWall;
                finalRenderLocations.Add(new Vector3(x - 1, y, wallIndex));
                break;
            case 3:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterHorzontalWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.CenterHorzontalWall;
                finalRenderLocations.Add(new Vector3(x - 1, y, wallIndex));
                finalRenderLocations.Add(new Vector3(x + 1, y, wallIndex));
                break;
            case 4:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].LowerVerticalWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.LowerVerticalWall;
                finalRenderLocations.Add(new Vector3(x, y + 1, wallIndex));
                break;
            case 5:
                if (obstacleGrid.GetGridObject(x + 1, y + 1) != null)
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthWestWall;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.SouthWestWall;
                    if (y - 1 > 0 && obstacleGrid.GetGridObject(x + 1, y - 1) != null)
                    {
                        finalRenderLocations.Add(new Vector3(x + 1, y, wallIndex));
                    }
                }
                else
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthWestWallModified;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.SouthWestWallModified;
                    if (y - 1 > 0 && obstacleGrid.GetGridObject(x + 1, y - 1) != null)
                    {
                        finalRenderLocations.Add(new Vector3(x + 1, y, wallIndex));
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
                        finalRenderLocations.Add(new Vector3(x - 1, y, wallIndex));
                    }

                }
                else
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthEastWallModified;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.SouthEastWallModified;
                    if (y - 1 > 0 && obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                    {
                        finalRenderLocations.Add(new Vector3(x - 1, y, wallIndex));
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
                    finalRenderLocations.Add(new Vector3(x + 1, y, wallIndex));
                }
                if (y + 1 < mainGameManger.mapHeight && obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                {
                    finalRenderLocations.Add(new Vector3(x - 1, y, wallIndex));
                }
                break;
            case 8:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].UpperVerticalWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.UpperVerticalWall;
                finalRenderLocations.Add(new Vector3(x, y - 1, wallIndex));
                break;
            case 9:
                if (obstacleGrid.GetGridObject(x + 1, y - 1) != null)
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthWestWall;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.NorthWestWall;
                    if (x - 1 > 0 && obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                    {
                        finalRenderLocations.Add(new Vector3(x, y - 1, wallIndex));
                    }
                }
                else
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthWestWallModified;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.NorthWestWallModified;
                    if (x - 1 > 0 && obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                    {
                        finalRenderLocations.Add(new Vector3(x, y - 1, wallIndex));
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
                        finalRenderLocations.Add(new Vector3(x, y - 1, wallIndex));
                    }
                    if (y + 1 < mainGameManger.mapHeight && obstacleGrid.GetGridObject(x - 1, y + 1) != null)
                    {
                        finalRenderLocations.Add(new Vector3(x - 1, y, wallIndex));
                    }
                }
                else
                {
                    obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthEastWallModified;
                    obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.NorthEastWallModified;
                    if (x + 1 < mainGameManger.mapWidth  && obstacleGrid.GetGridObject(x + 1, y - 1) != null)
                    {
                        finalRenderLocations.Add(new Vector3(x, y - 1, wallIndex));
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
                if (x + 1 >= mainGameManger.mapWidth  || x - 1 < 0)
                {
                    break;
                }
                if (obstacleGrid.GetGridObject(x - 1, y + 1) != null &&
                    obstacleGrid.GetGridObject(x + 1, y + 1) != null)
                {
                    finalRenderLocations.Add(new Vector3(x, y + 1, wallIndex));
                }
                if (obstacleGrid.GetGridObject(x - 1, y - 1) != null &&
                    obstacleGrid.GetGridObject(x + 1, y - 1) != null)
                {
                    finalRenderLocations.Add(new Vector3(x, y - 1, wallIndex));
                }
                break;
            case 13:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].WestWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.WestWall;
                if (x - 1 > 0 && y - 1 > 0 && obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                {
                    finalRenderLocations.Add(new Vector3(x, y - 1, wallIndex));
                }
                if (obstacleGrid.GetGridObject(x + 1, y + 1) == null)
                {
                    finalRenderLocations.Add(new Vector3(x, y, wallIndex));
                }
                break;
            case 14:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].EastWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.EastWall;
                if (x + 1 < mainGameManger.mapWidth  && y - 1 > 0 && obstacleGrid.GetGridObject(x - 1, y - 1) != null)
                {
                    finalRenderLocations.Add(new Vector3(x, y - 1, wallIndex));
                }
                if (obstacleGrid.GetGridObject(x - 1, y + 1) == null)
                {
                    finalRenderLocations.Add(new Vector3(x, y, wallIndex));
                }
                break;
            case 15:
                obstacleGrid.GetGridObject(x, y).GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterWall;
                obstacleGrid.GetGridObject(x, y).GetComponent<PrefabMapTile>().wallState = WallStates.CenterWall;
                break;
        }
    }
}
