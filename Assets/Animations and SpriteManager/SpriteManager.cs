using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;
using UnityEngine.WSA;
using UnityEngine.UIElements;
using System.Runtime.ConstrainedExecution;

public class SpriteManager : MonoBehaviour
{
    [SerializeField] private List<LineController> lineControllers;
    [SerializeField] private LineController oneActionLineController;
    [SerializeField] private LineController twoActionLineController;
    [SerializeField] private CombatGameManager combatGameManager;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private CombatUISystem combatUI;

    public GridHex<SpriteNode> spriteGrid;
    public int[,] elevationOfHexes;
    public int[,] terrainSprites;
    public bool[,] terrainIsChangingElevation;
    public TerrainHolder[,] terrain;
    public List<TerrainHolder> allGroundTerrainHolders;
    public List<List<Vector2Int>> terrainTilePositions;
    public List<TerrainHolder> inactiveWalls;
    public List<TerrainHolder> activeWalls;
    public List<GameObject> inactiveHighlightedHexes;
    public List<GameObject> activeHighlightedHexes;
    public List<GameObject> inactiveTargetHexes;
    public List<GameObject> activeTargetHexes;
    public List<GameObject> inactiveTriangles;
    public List<GameObject> activeTriangles;
    public List<SpriteHolder> inactiveSpriteHolders;
    public List<SpriteHolder> activeSpriteHolders;
    public List<SpriteHolder> unitPassiveAreaSpriteHolders;
    public List<SpriteHolder> inactivetargetingPassiveSpriteHolders;
    public List<SpriteHolder> activetargetingPassiveSpriteHolders;
    public TerrainHolder newGroundprefab;
    public TerrainHolder wallPrefab;
    public GameObject highlightedHexPrefab;
    public GameObject highlightedHexHolder;
    public GameObject wallHolder;
    public GameObject targetHexPrefab;
    public GameObject targetHexHolder;
    public GameObject spriteHolderHolder;
    public GameObject targetSpriteHolderHolder;
    public GameObject triangleHolder;
    public GameObject trianglePrefab;
    public LineController triangleLineOne;
    public LineController triangleLineTwo;
    public TerrainElevationChangeAnimation ChangeElevationAnimation;

    public int currentViewingElevation;

    public Vector2Int currentlySelectedHex;
    public GameObject currentlySelectedHexSprite;
    public MovementTargeting movementTargeting;
    public MeleeTargeting meleeTargeting;
    public ExtendedMeleeTargeting extendedMeleeTargeting;
    public RangedTargeting rangedTargeting;
    public ConeTargeting coneTargeting;

    public ActionConfirmationMenu actionConfirmationMenu;
    public SpriteHolder spriteHolderPrefab;
    public SpriteHolder targetSpriteHolderPrefab;

    // 0 - Ground Level, 1 GroundLevelHighlights, 2 GrondLevelTargetingHighlights, 3 Ground Level Units
    [SerializeField] private List<Tilemap> tileMaps;

    [SerializeField] private TargetingSystem activeTargetingSystems;

    public List<List<CustomAnimations>> animations = new List<List<CustomAnimations>>();
    public bool playNewAnimation;
    public bool DebugAnimations = false;
    public List<float> timeBetweenAnimations = new List<float>();
    public float currentTime;
    public string debugWord;
    // BL, B, BR, TL, T, TR
    public Vector2[] triangleAdjustmenst = new Vector2[6] { new Vector2(-0.38f, -0.21f), new Vector2(0, -.435f), 
        new Vector2(0.38f, -0.21f), new Vector2(-0.38f, .235f), new Vector2(0, .45f), new Vector2(0.38f, .235f) };

    public UnityAction<Vector2Int> NewSelectedHex;

    public void Start()
    {
        inputManager.TargetPositionMoved += SelectMouseHex;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (timeBetweenAnimations.Count != 0 && currentTime >= timeBetweenAnimations[0])
        {
            if (playNewAnimation)
            {
                if (DebugAnimations)
                {
                    Debug.Log("Playing New Line Of Animation");
                    debugWord = "";
                }

                debugWord += animations[0].ToString() + " ";
                for(int i = 0; i < animations[0].Count; i++)
                {
                    animations[0][i].PlayAnimation();
                }

                if (DebugAnimations)
                {
                    Debug.Log(debugWord);
                }
            }
            currentTime = 0;
        }

        if(currentTime > 100)
        {
            currentTime = 0;
        }
    }

    public void ResetSpriteManager()
    {
        EndAnimations();
        ClearLines();
        DeactiveTargetingSystem();
        ResetCombatAttackUI();
        for (int i = 0; i < unitPassiveAreaSpriteHolders.Count; i++)
        {
            DisableSpriteHolder(unitPassiveAreaSpriteHolders[i]);
        }
        unitPassiveAreaSpriteHolders.Clear();
        for(int i = 0; i < combatGameManager.mapSize; i++)
        {
            for(int j = 0;  j < combatGameManager.mapSize; j++)
            {
                SpriteRenderer[] sprites =  spriteGrid.GetGridObject(i, j).sprites;
                for(int k = 0; k < sprites.Length; k++)
                {
                    if(sprites[k] != null)
                    {
                        Destroy(sprites[k].gameObject);
                    }
                    sprites[k] = null;
                }
            }
        }
    }

    public void AddAnimations(CustomAnimations newAnimation, int index, float timeInterval = 0)
    {
        combatGameManager.playingAnimation = true;
        newAnimation.spriteManager = this;
        if (animations.Count == 0)
        {
            ChangePlayNewAnimation(true);
            animations.Add(new List<CustomAnimations>() { newAnimation });
            enabled = true;
            timeBetweenAnimations.Add(timeInterval);
        }
        else if (index >= animations.Count)
        {
            animations.Add(new List<CustomAnimations>() { newAnimation });
            timeBetweenAnimations.Add(timeInterval);
        }
        else
        {
            animations[index].Add(newAnimation);

        }

        if (DebugAnimations)
        {
            string debugWord = "";
            for (int i = 0; i < animations.Count; i++)
            {
                debugWord += "index " + i + ": " + animations[i].ToString();
                debugWord += "\n";
            }

            Debug.Log(debugWord);
        }
    }

    public void EndAnimations()
    {
        ChangePlayNewAnimation(false);
        combatGameManager.playingAnimation = false;
    }

    public void ChangePlayNewAnimation(bool playNewAnimation)
    {
        this.playNewAnimation = playNewAnimation;
    }

    public void UnitDied(Unit unit)
    {
        Destroy(spriteGrid.GetGridObject(unit.x, unit.y).sprites[0]);
        spriteGrid.GetGridObject(unit.x, unit.y).sprites[0] = null;
        //Destroy(unit.unitSpriteRenderer.gameObject);

        /*
        for(int i = 0; i < animations.Count; i++)
        {
            for(int j = 0; j < animations[i].Count; j++)
            {
                if (animations[i][j].actingUnit == unit)
                {
                    Destroy(animations[i][j].gameObject);
                    animations[i].RemoveAt(j);
                    j--;
                }
            }
            if (animations[i].Count == 0)
            {
                animations.RemoveAt(i);
                i--;
            }
        }
        */
    }

    public void SelectMouseHex()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 previousSelectedHex = currentlySelectedHex;
        RaycastHit2D[] hit = Physics2D.RaycastAll(worldPoint, Vector2.zero);
        if(hit.GetLength(0) == 0)
        {
            Debug.LogWarning("HIt Nothing");
            currentlySelectedHex = new Vector2Int(0, 0);
        }
        else
        {
            TerrainHolder currentTerrain = null;
            int highestElevation = -20;
            for(int i = 0; i < hit.GetLength(0); i++)
            {
                TerrainHolder tempTerrainHolder = hit[i].transform.gameObject.GetComponent<TerrainHolder>();
                if(tempTerrainHolder.sprite.sortingOrder > highestElevation)
                {
                    currentTerrain = tempTerrainHolder;
                    highestElevation = tempTerrainHolder.sprite.sortingOrder;
                }
            }
            currentlySelectedHex = new Vector2Int(currentTerrain.x, currentTerrain.y);
        }
        int hexElevation = elevationOfHexes[currentlySelectedHex.x, currentlySelectedHex.y] - combatGameManager.defaultElevation;
        currentlySelectedHexSprite.transform.position = spriteGrid.GetWorldPosition(currentlySelectedHex) + new Vector3(0, combatGameManager.terrainHeightDifference * hexElevation);
        DisplayUnitPassiveAreas();
        if (previousSelectedHex != currentlySelectedHex)
        {
            NewSelectedHex?.Invoke(currentlySelectedHex);
        }
    }

    public void DisplayUnitPassiveAreas()
    {
        for(int i = 0; i < unitPassiveAreaSpriteHolders.Count; i++)
        {
            DisableSpriteHolder(unitPassiveAreaSpriteHolders[i]);
        }
        unitPassiveAreaSpriteHolders.Clear();
        Unit unit = combatGameManager.grid.GetGridObject(currentlySelectedHex).unit;
        if(unit != null)
        {
            for(int i = 0; i < unit.passiveEffects.Count; i++)
            {
                PassiveEffectArea passiveArea =  unit.passiveEffects[i];
                for (int j = 0; j < passiveArea.passiveLocations.Count; j++)
                {
                    Vector2Int passivePosition = passiveArea.passiveLocations[j];
                    SpriteHolder currentSpriteHolder = UseOpenSpriteHolder();
                    unitPassiveAreaSpriteHolders.Add(currentSpriteHolder);
                    currentSpriteHolder.gameObject.transform.parent = transform;
                    currentSpriteHolder.gameObject.transform.position = GetWorldPosition(passiveArea.passiveLocations[j]);
                    currentSpriteHolder.spriteRenderer.sortingOrder = terrain[passivePosition.x, passivePosition.y].sprite.sortingOrder + 2;
                    currentSpriteHolder.spriteRenderer.sprite = passiveArea.passive.passive.UISkillImage;
                }
            }
        }
    }
    

    public void ActivateActionConfirmationMenu(UnityAction confirmAction, UnityAction cancelAction)
    {
        actionConfirmationMenu.ActivateMenu(confirmAction, cancelAction);
    }

    public void ConfirmAction()
    {
        actionConfirmationMenu.ActivateConfirmAction();
    }


    public void CancelAction()
    {
        actionConfirmationMenu.ActivateCancelAction();
    }

    public void DrawLine(List<Vector3> oneActionLinePath = null, List<Vector3> twoActionLinePath = null)
    { 
        if(oneActionLinePath != null && oneActionLinePath.Count != 0)
        {
            oneActionLineController.SetLine(oneActionLinePath);
        }

        if (twoActionLinePath != null && twoActionLinePath.Count != 0)
        {
            twoActionLineController.SetLine(twoActionLinePath);
        }
    }

    public void DrawLine(List<Vector3> linePath, int lineControllerIndex)
    {
        if (linePath != null)
        {
            lineControllers[lineControllerIndex].SetLine(linePath);
        }
    }

    public void ClearLines()
    {
        List<Vector3> emptyList = new List<Vector3>();
        for(int i = 0; i < lineControllers.Count; i++)
        {
            lineControllers[i].SetLine(emptyList);
        }
        oneActionLineController.SetLine(emptyList);
        twoActionLineController.SetLine(emptyList);
    }

    public void ActivateMovementTargeting(Unit movingUnit, bool targetFriendly, int actionPointsLeft)
    {
        NewSelectedHex += movementTargeting.SelectNewPosition;
        inputManager.FoundPosition += movementTargeting.EndTargeting;
        activeTargetingSystems = movementTargeting;
        movementTargeting.SetParameters(movingUnit, targetFriendly, actionPointsLeft);
        movementTargeting.SelectNewPosition(currentlySelectedHex);
    }

    public void ActivateMeleeAttackTargeting(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int actionPointUseAmount, int meleeRange,
        MeleeTargeting.CalculateAttackData calculateAttackData)
    {
        combatUI.OnActivateTargetingSystem();
        NewSelectedHex += meleeTargeting.SelectNewPosition;
        inputManager.FoundPosition += meleeTargeting.EndTargeting;
        activeTargetingSystems = meleeTargeting;
        meleeTargeting.SetParameters(movingUnit, targetFriendly, actionPointsLeft, actionPointUseAmount, meleeRange, calculateAttackData);
        meleeTargeting.SelectNewPosition(currentlySelectedHex);
    }

    public void ActivateExtendedMeleeAttackTargeting(Unit movingUnit, bool targetFriendly, int actionPointsLeft, 
        int actionPointUseAmount, int meleeRange, ExtendedMeleeTargeting.CalculateAttackData calculateAttackData)
    {
        combatUI.OnActivateTargetingSystem();
        NewSelectedHex += extendedMeleeTargeting.SelectNewPosition;
        inputManager.FoundPosition += extendedMeleeTargeting.EndTargeting;
        activeTargetingSystems = extendedMeleeTargeting;
        extendedMeleeTargeting.SetParameters(movingUnit, targetFriendly, actionPointsLeft, actionPointUseAmount, meleeRange, calculateAttackData);
        extendedMeleeTargeting.SelectNewPosition(currentlySelectedHex);
    }
    public void ActivateRangedTargeting(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int actionPointUseAmount, int range,
       AttackData attackData, List<EquipableAmmoSO> unitAmmo)
    {
        combatUI.OnActivateTargetingSystem();
        NewSelectedHex += rangedTargeting.SelectNewPosition;
        inputManager.FoundPosition += rangedTargeting.EndTargeting;
        activeTargetingSystems = rangedTargeting;
        rangedTargeting.SetParameters(movingUnit, targetFriendly, actionPointsLeft, actionPointUseAmount, range, attackData ,unitAmmo);
        rangedTargeting.SelectNewPosition(currentlySelectedHex);
    }
    public void ActivateConeTargeting(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int range, int coneRange)
    {
        combatUI.OnActivateTargetingSystem();
        NewSelectedHex += coneTargeting.SelectNewPosition;
        inputManager.FoundPosition += coneTargeting.EndTargeting;
        activeTargetingSystems = coneTargeting;
        coneTargeting.SetParameters(movingUnit, targetFriendly, actionPointsLeft, range, coneRange);
        coneTargeting.SelectNewPosition(currentlySelectedHex);
    }

    public void NextItem()
    {
        activeTargetingSystems.NextItem();
    }

    public void PreviousItem()
    {
        activeTargetingSystems.PreviousItem();
    }

    public void DeactiveTargetingSystem()
    {
        combatUI.OnDeactivateTargetingSystem();
        CancelAction();
        if(activeTargetingSystems != null)
        {
            NewSelectedHex = null;
            inputManager.FoundPosition = null;
            activeTargetingSystems.DeactivateTargetingSystem();
        }
    }

    public void ActivateLineOfSightTargeting(TargetingSystem currentTargetingSystem, int range, UnityAction confirmAction, UnityAction cancelAction)
    {

    }

    public void ChangeTile(Vector2Int position, int tilemapIndex, int hexIndex)
    {
        Vector3Int currentNodePosition = new Vector3Int(position.y, position.x);
       tileMaps[tilemapIndex].SetTile(currentNodePosition, resourceManager.BaseTile[hexIndex]);
    }

    public GameObject CreateTempSpriteHolder(Vector2Int hexPosition, int spriteLayer, Sprite sprite)
    {
        Vector3 worldPosition = GetWorldPosition(hexPosition.x, hexPosition.y);
        return CreateSpriteRenderer(spriteLayer, sprite, worldPosition);
    }

    public GameObject CreateSpriteRenderer(int index, Sprite sprite, Vector3 objectPosition)
    {
        SpriteNode currentSpriteNode = spriteGrid.GetGridObject(objectPosition);
        SpriteRenderer ownSpriteRenderer = Instantiate(resourceManager.spriteHolder, objectPosition, new Quaternion(0, 0, 0, 1f));
        spriteGrid.GetXY(objectPosition, out int x, out int y);
        ownSpriteRenderer.sortingOrder = terrain[x, y].sprite.sortingOrder + 3;
        ownSpriteRenderer.transform.parent = transform;
        currentSpriteNode.sprites[index] = ownSpriteRenderer;
        currentSpriteNode.sprites[index].sprite = sprite;
        return ownSpriteRenderer.gameObject;
    }

    public void ActivateCombatAttackUI(Unit targetUnit, List<AttackDataUI> attackData, Vector3 newPosition)
    {
        combatUI.SetDataAttackUi(targetUnit, attackData, newPosition);
    }

    public void ResetCombatAttackUI()
    {
        Debug.Log("ResetCombatAttackUI");
        combatUI.ResetDataAttackUI();
    }

    public void CreateGrid(int mapWidth, int mapHeight, int amountOfTerrainLevels, float cellSize, Vector3 defaultGridAdjustment)
    {
        spriteGrid = new GridHex<SpriteNode>(mapWidth, mapHeight, cellSize, defaultGridAdjustment, (GridHex<SpriteNode> g, int x, int y) => new SpriteNode(g, x, y), true);
        terrainTilePositions =  new List<List<Vector2Int>>();
        terrain = new TerrainHolder[mapWidth, mapHeight];
        for (int i = 0; i < amountOfTerrainLevels; i++)
        {
            terrainTilePositions.Add(new List<Vector2Int>());
        }

        elevationOfHexes = new int[mapWidth, mapHeight];
        terrainSprites = new int[mapWidth, mapHeight];
        terrainIsChangingElevation = new bool[mapWidth, mapHeight];
        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                elevationOfHexes[j, i] = combatGameManager.defaultElevation;
                terrainSprites[j, i] = 1;
                terrainIsChangingElevation[j, i] = false;
            }
        }

        for(int i  = 0; i < allGroundTerrainHolders.Count; i++)
        {
            TerrainHolder tempTerrainHolder = allGroundTerrainHolders[i];
            terrain[tempTerrainHolder.x, tempTerrainHolder.y] =  tempTerrainHolder;
        }
        currentViewingElevation = terrainTilePositions.Count - 1;
    }

    public Vector3 GetWorldPosition(Vector2Int hexPosition)
    {
        return GetWorldPosition(hexPosition.x, hexPosition.y);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        Vector3 hexPosition =  spriteGrid.GetWorldPosition(x, y);
        hexPosition += new Vector3(0, combatGameManager.terrainHeightDifference * (elevationOfHexes[x, y] - combatGameManager.defaultElevation));
        return hexPosition;
    }

    public void SetViewElevation(int newViewElevation)
    {
        if (newViewElevation >= terrainTilePositions.Count || newViewElevation < 0)
        {
            return;
        }
        currentViewingElevation = newViewElevation;
        for (int currentElevation = 0; currentElevation < terrainTilePositions.Count; currentElevation++)
        {
            for (int tilePositionIndex = 0; tilePositionIndex < terrainTilePositions[currentElevation].Count; tilePositionIndex++)
            {
                Vector2Int tilePosition = terrainTilePositions[currentElevation][tilePositionIndex];
                TerrainHolder terrainHex = terrain[tilePosition.x, tilePosition.y];
                if (currentElevation <= currentViewingElevation)
                {
                    terrainHex.transform.position = spriteGrid.GetWorldPosition(tilePosition.x, tilePosition.y)  + 
                        new Vector3(0, combatGameManager.terrainHeightDifference * (currentElevation - combatGameManager.defaultElevation));
                    for (int k = 0; k < terrainHex.walls.Count; k++)
                    {
                        terrainHex.walls[k].enabled = true;
                    }
                }
                else
                {
                    int elevationDifference = currentElevation - currentViewingElevation;
                    terrainHex.transform.position = spriteGrid.GetWorldPosition(tilePosition.x, tilePosition.y) +
                        new Vector3(0, combatGameManager.terrainHeightDifference * (currentElevation - combatGameManager.defaultElevation - elevationDifference));
                    for (int k = 0; k < elevationDifference; k++)
                    {
                        if(terrainHex.walls.Count - 1 - k < 0)
                        {
                            break;
                        }
                        terrainHex.walls[terrainHex.walls.Count - 1 - k].enabled = false;
                    }
                }
            }
        }
    }

    public void ChangeElevation(int x, int y, int elevationChangeAmount, bool playAnimation = false, bool partOfAGroup = false)
    {
        if (playAnimation)
        {
            int initialElevationOfHex = elevationOfHexes[x, y];
            int newElevation = initialElevationOfHex + elevationChangeAmount;
            if (newElevation < 0)
            {
                newElevation = 0;
            }
            else if (newElevation > terrainTilePositions.Count - 1)
            {
                newElevation = terrainTilePositions.Count - 1;
            }
            int realElevationChangeAmount = newElevation - initialElevationOfHex;
            if (realElevationChangeAmount == 0)
            {
                return;
            }

            terrainIsChangingElevation[x, y] = true;
            TerrainElevationChangeAnimation changeElevation = Instantiate(ChangeElevationAnimation);
            Vector3 currenthexPosition = spriteGrid.GetWorldPosition(x, y);
            changeElevation.SetParameters(combatGameManager, currenthexPosition, (currenthexPosition +
                (realElevationChangeAmount * new Vector3(0, combatGameManager.terrainHeightDifference))),
                x, y, initialElevationOfHex, newElevation, partOfAGroup);
            int terrainTileIndex = terrainTilePositions[initialElevationOfHex].IndexOf(new Vector2Int(x, y));
            Vector2Int newTileHex = terrainTilePositions[initialElevationOfHex][terrainTileIndex];
            terrainTilePositions[initialElevationOfHex].RemoveAt(terrainTileIndex);
            terrainTilePositions[newElevation].Add(newTileHex);
        }
        else
        {
            TerrainHolder currentTerrain = terrain[x, y];
            int initialElevationOfHex = elevationOfHexes[x, y];
            int newElevation = initialElevationOfHex + elevationChangeAmount;
            if (newElevation > combatGameManager.defaultElevation)
            {
                currentTerrain.transform.position = spriteGrid.GetWorldPosition(x, y) + new Vector3(0, combatGameManager.terrainHeightDifference * (newElevation -
                    combatGameManager.defaultElevation));
                for(int i = 0; i < newElevation - combatGameManager.defaultElevation; i++)
                {
                    TerrainHolder newWall = UseOpenWall();
                    newWall.transform.position = currentTerrain.transform.position -
                        new Vector3(0, combatGameManager.terrainHeightDifference * (i));
                    newWall.x = x;
                    newWall.y = y;
                    newWall.transform.parent = currentTerrain.gameObject.transform;
                    newWall.sprite.sortingOrder = currentTerrain.sprite.sortingOrder;
                    currentTerrain.walls.Add(newWall);
                }                
            }
            //Decrease Elevation
            else if (newElevation < combatGameManager.defaultElevation)
            {
                currentTerrain.transform.position = spriteGrid.GetWorldPosition(x, y) + new Vector3(0, combatGameManager.terrainHeightDifference * (newElevation -
                    combatGameManager.defaultElevation));

                Vector3Int cubePos = spriteGrid.OffsetToCube(x, y);
                Vector3Int TLCube = cubePos + spriteGrid.cubeDirectionVectors[4];
                Vector3Int TCube = cubePos + spriteGrid.cubeDirectionVectors[5];
                Vector3Int TRCube = cubePos + spriteGrid.cubeDirectionVectors[0];

                Vector2Int TlOffset = spriteGrid.CubeToOffset(TLCube);
                Vector2Int TOffset = spriteGrid.CubeToOffset(TCube);
                Vector2Int TROffset = spriteGrid.CubeToOffset(TRCube);

                if (ValidGridPlacement(TlOffset) && elevationOfHexes[TlOffset.x, TlOffset.y] > newElevation)
                {
                    PlaceWallIfBelowGround(TlOffset, newElevation);
                }

                if (ValidGridPlacement(TOffset) && elevationOfHexes[TOffset.x, TOffset.y] > newElevation)
                {
                    PlaceWallIfBelowGround(TOffset, newElevation);
                }

                if (ValidGridPlacement(TROffset) && elevationOfHexes[TROffset.x, TROffset.y] > newElevation)
                {
                    PlaceWallIfBelowGround(TROffset, newElevation);
                }
            }
        }
    }

    public bool ValidGridPlacement(Vector2Int offset)
    {
        return offset.x >= 0 && offset.y >= 0 && offset.x < spriteGrid.GetWidth() && offset.y < spriteGrid.GetHeight();
    }

    public void PlaceWallIfBelowGround(Vector2Int hexPosition, int newElevation)
    {
        int newTerrainHeight = elevationOfHexes[hexPosition.x, hexPosition.y];
        int topHeight = newTerrainHeight;
        if (newElevation > combatGameManager.defaultElevation)
        {
            topHeight = combatGameManager.defaultElevation;
        }

        TerrainHolder newTerrain = terrain[hexPosition.x, hexPosition.y];
        for (int i = 0; i < topHeight - newElevation; i++)
        {
            if (newTerrain.walls.Count >= topHeight - newElevation)
            {
                return;
            }
            TerrainHolder newWall = UseOpenWall();
            newWall.transform.position = newTerrain.transform.position -
                new Vector3(0, combatGameManager.terrainHeightDifference * (newTerrain.walls.Count));
            newWall.x = hexPosition.x;
            newWall.y = hexPosition.y;
            newWall.transform.parent = newTerrain.gameObject.transform;
            newWall.sprite.sortingOrder = newTerrain.sprite.sortingOrder;
            newTerrain.walls.Add(newWall);
        }
    }

    public TerrainHolder UseOpenWall()
    {
        if (inactiveWalls.Count <= 0)
        {
            TerrainHolder newWall = Instantiate(wallPrefab);
            newWall.transform.position = new Vector3(-20, -20);
            newWall.transform.parent = wallHolder.transform;
            inactiveWalls.Add(newWall);
        }

        TerrainHolder openWall = inactiveWalls[0];
        inactiveWalls.RemoveAt(0);
        activeWalls.Add(openWall);
        return openWall;
    }

    public void DisableWall(TerrainHolder wall)
    {
        activeWalls.Remove(wall);
        inactiveWalls.Add(wall);
        wall.transform.parent = wall.transform;
        wall.transform.position = new Vector3(-20, -20);
    }

    public GameObject UseOpenHighlightedHex()
    {
        if (inactiveHighlightedHexes.Count <= 0)
        {
            GameObject newHex = Instantiate(highlightedHexPrefab);
            newHex.transform.position = new Vector3(-20, -20);
            newHex.transform.parent = highlightedHexHolder.transform;
            inactiveHighlightedHexes.Add(newHex);
        }

        GameObject openHighlightedHex = inactiveHighlightedHexes[0];
        inactiveHighlightedHexes.RemoveAt(0);
        activeHighlightedHexes.Add(openHighlightedHex);
        return openHighlightedHex;
    }

    // Disables with TargetingSystems
    public void DisableHighlightedHex(GameObject hex)
    {
        activeHighlightedHexes.Remove(hex);
        inactiveHighlightedHexes.Add(hex);
        hex.transform.position = new Vector3(-20, -20);
    }

    public GameObject UseOpenTargetHex()
    {
        if (inactiveTargetHexes.Count <= 0)
        {
            GameObject newHex = Instantiate(targetHexPrefab);
            newHex.transform.position = new Vector3(-20, -20);
            newHex.transform.parent = targetHexHolder.transform;
            inactiveTargetHexes.Add(newHex);
        }

        GameObject openHighlightedHex = inactiveTargetHexes[0];
        inactiveTargetHexes.RemoveAt(0);
        activeTargetHexes.Add(openHighlightedHex);
        return openHighlightedHex;
    }

    // Disables with TargetingSystems
    public void DisableTargetHex(GameObject hex)
    {
        activeTargetHexes.Remove(hex);
        inactiveTargetHexes.Add(hex);
        hex.transform.position = new Vector3(-20, -20);
    }

    public SpriteHolder UseOpenSpriteHolder()
    {
        if (inactiveSpriteHolders.Count <= 0)
        {
            Debug.Log("Create SpriteHolder");
            SpriteHolder newSpriteHolder = Instantiate(spriteHolderPrefab);
            newSpriteHolder.transform.position = new Vector3(-20, -20);
            newSpriteHolder.transform.parent = spriteHolderHolder.transform;
            inactiveSpriteHolders.Add(newSpriteHolder);
        }

        SpriteHolder openSpriteHolder = inactiveSpriteHolders[0];
        inactiveSpriteHolders.RemoveAt(0);
        activeSpriteHolders.Add(openSpriteHolder);
        return openSpriteHolder;
    
    }

    public void DisableSpriteHolder(SpriteHolder hex)
    {
        activeSpriteHolders.Remove(hex);
        inactiveSpriteHolders.Add(hex);
        hex.transform.position = new Vector3(-20, -20);
        hex.spriteRenderer.sprite = null;
        hex.transform.parent = spriteHolderHolder.transform;
    }

    public SpriteHolder UseOpenTargetingSpriteHolder()
    {
        if (inactivetargetingPassiveSpriteHolders.Count <= 0)
        {
            Debug.Log("Create SpriteHolder");
            SpriteHolder newSpriteHolder = Instantiate(targetSpriteHolderPrefab);
            newSpriteHolder.transform.position = new Vector3(-20, -20);
            newSpriteHolder.transform.parent = targetSpriteHolderHolder.transform;
            inactivetargetingPassiveSpriteHolders.Add(newSpriteHolder);
        }

        SpriteHolder openSpriteHolder = inactivetargetingPassiveSpriteHolders[0];
        inactivetargetingPassiveSpriteHolders.RemoveAt(0);
        activetargetingPassiveSpriteHolders.Add(openSpriteHolder);
        return openSpriteHolder;
    }

    //Disables with Targeting System
    public void DisableTargetingSpriteHolder(SpriteHolder hex)
    {
        activetargetingPassiveSpriteHolders.Remove(hex);
        inactivetargetingPassiveSpriteHolders.Add(hex);
        hex.transform.position = new Vector3(-20, -20);
        hex.spriteRenderer.sprite = null;
        hex.transform.parent = targetSpriteHolderHolder.transform;
    }

    public void UseTriangle(Vector2Int trianglePos)
    {
        if (inactiveTriangles.Count <= 0)
        {
            GameObject newSpriteHolder = Instantiate(trianglePrefab);
            newSpriteHolder.transform.position = new Vector3(-20, -20);
            newSpriteHolder.transform.parent = triangleHolder.transform;
            inactiveTriangles.Add(newSpriteHolder);
        }

        GameObject openTriangle = inactiveTriangles[0];
        inactiveTriangles.RemoveAt(0);
        activeTriangles.Add(openTriangle);
        int x = trianglePos.x / 3;
        int yAdjustment = 0;
        if (x % 2 == 1)
        {
            yAdjustment = 1;
        }
        int y = trianglePos.y - yAdjustment;
        y = y / 2;

        int xValue = trianglePos.x - (x * 3);
        int yValue = trianglePos.y - (y  * 2) - yAdjustment;
        int triValue =  xValue + (yValue * 3);
        Debug.Log("TriValue: " + trianglePos + ", " +  triValue + ", " + x + ", " + y); 
        if(triValue > 5 || triValue < 0)
        {
            return;
        }

        switch (triValue)
        {
            case 0:
                openTriangle.transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0, 0, 60, 0));
                break;
            case 1:
                openTriangle.transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0, 0, 0, 0));
                break;
            case 2:
                openTriangle.transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0, 0, 60, 0));
                break;
            case 3:
                openTriangle.transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0, 0, 0, 0));
                break;
            case 4:
                openTriangle.transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0, 0, 60, 0));
                break;
            case 5:
                openTriangle.transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0, 0, 0, 0));
                break;
        }

        openTriangle.gameObject.transform.position = spriteGrid.GetWorldPosition(x, y) + (Vector3) triangleAdjustmenst[triValue];
    }

    public void ResetTriangle()
    {
        triangleLineOne.SetLine(new List<Vector3>());
        triangleLineTwo.SetLine(new List<Vector3>());
        for (int i = 0; i < activeTriangles.Count; i++)
        {
            GameObject triangle = activeTriangles[0];
            inactiveTriangles.Add(activeTriangles[0]);
            activeTriangles.RemoveAt(0);
            triangle.transform.position = new Vector3(-20, -20);
            triangle.transform.parent = triangleHolder.transform;
            i--;
        }
    }

    public void SetTriangleLine(int lineIndex, Vector2Int startTrainglePos, Vector2Int endTrianglePos)
    {

        int x = startTrainglePos.x / 3;
        int yAdjustment = 0;
        if (x % 2 == 1)
        {
            yAdjustment = 1;
        }
        int y = startTrainglePos.y - yAdjustment;
        y = y / 2;

        int xValue = startTrainglePos.x - (x * 3);
        int yValue = startTrainglePos.y - (y * 2) - yAdjustment;
        int triValue = xValue + (yValue * 3);
        Debug.Log("TriValue: " + startTrainglePos + ", " + triValue + ", " + x + ", " + y);
        if (triValue > 5 || triValue < 0)
        {
            return;
        }
        Vector3 startPosition = spriteGrid.GetWorldPosition(x, y) + (Vector3)triangleAdjustmenst[triValue];

        x = endTrianglePos.x / 3;
        yAdjustment = 0;
        if (x % 2 == 1)
        {
            yAdjustment = 1;
        }
        y = endTrianglePos.y - yAdjustment;
        y = y / 2;

        xValue = endTrianglePos.x - (x * 3);
        yValue = endTrianglePos.y - (y * 2) - yAdjustment;
        triValue = xValue + (yValue * 3);
        Debug.Log("TriValue: " + endTrianglePos + ", " + triValue + ", " + x + ", " + y);
        if (triValue > 5 || triValue < 0)
        {
            return;
        }
        Vector3 endPosition = spriteGrid.GetWorldPosition(x, y) + (Vector3)triangleAdjustmenst[triValue];

        if (lineIndex == 0)
        {
            triangleLineOne.SetLine(new List<Vector3> { startPosition, endPosition });
        }
        else
        {
            triangleLineTwo.SetLine(new List<Vector3> { startPosition, endPosition });
        }
    }
}
