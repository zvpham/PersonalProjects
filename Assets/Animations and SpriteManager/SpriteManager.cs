using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Events;
using UnityEngine.WSA;
using UnityEngine.UIElements;

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
    public TerrainHolder[,] tempTerrain;
    public TerrainHolder[,] terrain;
    public List<List<Vector2Int>> terrainTilePositions;
    public List<TerrainHolder> inactiveWalls;
    public List<TerrainHolder> activeWalls;
    public List<GameObject> inactiveMasks;
    public List<GameObject> activeMasks;
    public TerrainHolder newGroundprefab;
    public TerrainHolder wallPrefab;
    public GameObject maskPrefab;
    public TerrainElevationChangeAnimation ChangeElevationAnimation;

    public int currentViewingElevation;

    public MovementTargeting movementTargeting;
    public MeleeTargeting meleeTargeting;
    public RangedTargeting rangedTargeting;
    public ConeTargeting coneTargeting;

    public ActionConfirmationMenu actionConfirmationMenu;
    public GameObject spriteHolderPrefab;

    // 0 - Ground Level, 1 GroundLevelHighlights, 2 GrondLevelTargetingHighlights, 3 Ground Level Units
    [SerializeField] private List<Tilemap> tileMaps;

    [SerializeField] private TargetingSystem activeTargetingSystems;

    public List<List<CustomAnimations>> animations = new List<List<CustomAnimations>>();
    public bool playNewAnimation;
    public bool DebugAnimations = false;
    public List<float> timeBetweenAnimations = new List<float>();
    public float currentTime;
    public string debugWord;

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
        inputManager.TargetPositionMoved += movementTargeting.SelectNewPosition;
        inputManager.FoundPosition += movementTargeting.EndTargeting;
        activeTargetingSystems = movementTargeting;
        movementTargeting.SetParameters(movingUnit, targetFriendly, actionPointsLeft);
        movementTargeting.SelectNewPosition(UtilsClass.GetMouseWorldPosition());
    }

    public void ActivateMeleeAttackTargeting(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int actionPointUseAmount, int meleeRange,
        MeleeTargeting.CalculateAttackData calculateAttackData)
    {
        combatUI.OnActivateTargetingSystem();
        inputManager.TargetPositionMoved += meleeTargeting.SelectNewPosition;
        inputManager.FoundPosition += meleeTargeting.EndTargeting;
        activeTargetingSystems = meleeTargeting;
        meleeTargeting.SetParameters(movingUnit, targetFriendly, actionPointsLeft, actionPointUseAmount, meleeRange, calculateAttackData);
        meleeTargeting.SelectNewPosition(UtilsClass.GetMouseWorldPosition());
    }
    public void ActivateRangedTargeting(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int actionPointUseAmount, int range,
   AttackData attackData, List<EquipableAmmoSO> unitAmmo)
    {
        combatUI.OnActivateTargetingSystem();
        inputManager.TargetPositionMoved += rangedTargeting.SelectNewPosition;
        inputManager.FoundPosition += rangedTargeting.EndTargeting;
        activeTargetingSystems = rangedTargeting;
        rangedTargeting.SetParameters(movingUnit, targetFriendly, actionPointsLeft, actionPointUseAmount, range, attackData ,unitAmmo);
        rangedTargeting.SelectNewPosition(UtilsClass.GetMouseWorldPosition());
    }
    public void ActivateConeTargeting(Unit movingUnit, bool targetFriendly, int actionPointsLeft, int range, int coneRange)
    {
        combatUI.OnActivateTargetingSystem();
        inputManager.TargetPositionMoved += coneTargeting.SelectNewPosition;
        inputManager.FoundPosition += coneTargeting.EndTargeting;
        activeTargetingSystems = coneTargeting;
        coneTargeting.SetParameters(movingUnit, targetFriendly, actionPointsLeft, range, coneRange);
        coneTargeting.SelectNewPosition(UtilsClass.GetMouseWorldPosition());
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
            inputManager.TargetPositionMoved = null;
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
        Vector3 worldPosition = combatGameManager.grid.GetWorldPosition(hexPosition.x, hexPosition.y);
        return CreateSpriteRenderer(spriteLayer, -7, sprite, worldPosition);
    }

    public GameObject CreateSpriteRenderer(int index, int sortingOrder, Sprite sprite, Vector3 objectPosition)
    {
        SpriteNode currentSpriteNode = spriteGrid.GetGridObject(objectPosition);
        SpriteRenderer ownSpriteRenderer = Instantiate(resourceManager.spriteHolder, objectPosition, new Quaternion(0, 0, 0, 1f));
        ownSpriteRenderer.sortingOrder = sortingOrder;
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
        combatUI.ResetDataAttackUI();
    }

    public void CreateGrid(int mapWidth, int mapHeight, int amountOfTerrainLevels, float cellSize, Vector3 defaultGridAdjustment)
    {
        spriteGrid = new GridHex<SpriteNode>(mapWidth, mapHeight, cellSize, defaultGridAdjustment, (GridHex<SpriteNode> g, int x, int y) => new SpriteNode(g, x, y), false);
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
                terrain[j, i] = tempTerrain[j, i];
                terrainTilePositions[tempTerrain[j, i].elevation].Add(new Vector2Int(j, i));
            }
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
        int topHeight = newElevation;
        if (newElevation > combatGameManager.defaultElevation)
        {
            topHeight = combatGameManager.defaultElevation;
        }

        TerrainHolder newTerrain = terrain[hexPosition.x, hexPosition.y];
        Debug.Log("Attemptin TO Place NEw Wall: " + topHeight + ", " + newElevation); 
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
        wall.transform.parent = null;
        wall.transform.position = new Vector3(-20, -20);
    }

    public GameObject UseOpenMask()
    {
        if (inactiveMasks.Count <= 0)
        {
            GameObject newWall = Instantiate(maskPrefab);
            newWall.transform.position = new Vector3(-20, -20);
            inactiveMasks.Add(newWall);
        }

        GameObject openWall = inactiveMasks[0];
        inactiveMasks.RemoveAt(0);
        activeMasks.Add(openWall);
        return openWall;
    }

    public void DisableMask(GameObject mask)
    {
        activeMasks.Remove(mask);
        inactiveMasks.Add(mask);
        mask.transform.parent = null;
        mask.transform.position = new Vector3(-20, -20);
    }
}
