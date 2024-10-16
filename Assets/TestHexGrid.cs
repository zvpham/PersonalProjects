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

public class TestHexGrid : MonoBehaviour
{
    GridHex<GridPosition> hexgrid;
    public PrefabTerrain prefabTerrain;
    public PrefabUnitTerrain prefabUnitTerrain;
    public float cellSize;
    public Vector3 defaultGridAdjustment;
    //AStarPathfindingHex path;
    //DijkstraMap map;
    public List<Vector3Int> highlightedHexws = new List<Vector3Int>();
    public Vector2Int cubeStartHex;
    public Vector2Int cubeEndHex;
    public Vector3 currentMouse;
    public Vector3 prevMouse;
    public Tilemap tileMap;
    public ResourceManager resourceManager;
    public LineController lineController;
    public int index = 0;
    public float firstAngle;
    public float lastAngle;
    public int mouseIndex;
    public Vector2Int startHex;
    public Vector2Int endHex;

    public int defaultElevation = 3;
    public float terrainHeightDifference = 0.16125f;
    public TerrainElevationChangeAnimation ChangeElevationAnimation;

    public CombatGameManager gameManager;
    public Vector2Int currentlySelectedHex;
    public GameObject currentlySelectedHexSprite;

    public TerrainHolder newGroundHexPrefab;

    // Start is called before the first frame update
    void Start()
    {
        //hexgrid =  new GridHex<GridPosition>(10, 10, 1, new Vector3(-0.5f, -0.5f, 0), (GridHex<GridPosition> Grid, int x, int y) => new GridPosition(Grid, x, y, 3), true);
        CreateGrid(32, 32, 7);
        LoadUnits();
    }


    public void CreateGrid(int mapWidth, int mapHeight, int amountOfTerrainLevels)
    {
        gameManager.spriteManager.CreateGrid(mapWidth, mapHeight, amountOfTerrainLevels, cellSize, defaultGridAdjustment);
        hexgrid = new GridHex<GridPosition>(mapWidth, mapHeight, cellSize, defaultGridAdjustment, (GridHex<GridPosition> g, int x, int y) =>
            new GridPosition(g, x, y, defaultElevation), false);
        for (int i = 0; i < prefabTerrain.terrainElevation.Count; i++)
        {
            Vector3Int terrainHexData = prefabTerrain.terrainElevation[i];
            gameManager.spriteManager.elevationOfHexes[terrainHexData.x, terrainHexData.y] = terrainHexData.z;
        }


        Debug.Log(gameManager.spriteManager.terrainTilePositions.Count);
        for (int i = 0; i < prefabTerrain.terrainElevation.Count; i++)
        {
            Vector3Int terrainHexData = prefabTerrain.terrainElevation[i];
            gameManager.spriteManager.terrainTilePositions[terrainHexData.z].Add(new Vector2Int(terrainHexData.x, terrainHexData.y));
            gameManager.spriteManager.ChangeElevation(terrainHexData.x, terrainHexData.y, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        SelectMouseHex();


        int initalElevation = gameManager.spriteManager.elevationOfHexes[currentlySelectedHex.x, currentlySelectedHex.y];
        // Raise Elevation
        if (Input.GetKeyDown(KeyCode.O))
        {
            gameManager.spriteManager.ChangeElevation(currentlySelectedHex.x, currentlySelectedHex.y, 1, true);
        }
        // Drop Elevation
        else if (Input.GetKeyDown(KeyCode.P))
        {
            gameManager.spriteManager.ChangeElevation(currentlySelectedHex.x, currentlySelectedHex.y, - 1, true);
        }
        else if(Input.GetKeyDown(KeyCode.Z))
        {
            gameManager.spriteManager.SetViewElevation(gameManager.spriteManager.currentViewingElevation + 1);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            gameManager.spriteManager.SetViewElevation(gameManager.spriteManager.currentViewingElevation - 1);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if(mouseIndex == 0)
            {
                startHex = currentlySelectedHex;
                mouseIndex = 1;
            }
            else
            {
                endHex = currentlySelectedHex;

                Vector3Int startCube = gameManager.spriteManager.spriteGrid.OffsetToCube(startHex.x, startHex.y);
                Vector3Int endCube = gameManager.spriteManager.spriteGrid.OffsetToCube(endHex.x, endHex.y);

                List<Vector3Int> cubePath = gameManager.spriteManager.spriteGrid.CubeLineDraw(startCube, endCube);

                for(int i = 0; i < cubePath.Count; i++)
                {
                    Vector2Int offSetPosition = gameManager.spriteManager.spriteGrid.CubeToOffset(cubePath[i]);
                    gameManager.spriteManager.ChangeElevation(offSetPosition.x, offSetPosition.y, 2, true, false);
                }
                mouseIndex = 0;
            }


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
            hexgrid.GetXY(worldMousePosition, out int endX, out int endY);
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
        currentlySelectedHexSprite.transform.position = hexgrid.GetWorldPosition(currentlySelectedHex) + new Vector3(0, terrainHeightDifference * hexElevation);
    }

    private void LoadUnits()
    {
        int positionIndex = 0;
        for(int i = 0; i < prefabUnitTerrain.unitTeam.Count; i++)
        {
            battleLineData newBattleLineUnit = prefabUnitTerrain.units[i];
            // Load Player Unit
            if (prefabUnitTerrain.unitTeam[i] == 0)
            {
                LoadPlayerUnitSuperGroupData(newBattleLineUnit);
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
                    newHero.gameManager = gameManager;
                    newHero.team = Team.Team4;
                    newHero.transform.SetParent(gameObject.transform);
                    newHero.transform.position = gameManager.spriteManager.GetWorldPosition(prefabUnitTerrain.unitPositions[positionIndex]);
                    positionIndex += 1;
                }
            }
        }
    }

    private UnitSuperClass LoadPlayerUnitSuperGroupData(battleLineData backLineUnit)
    {
        if (backLineUnit.unitGroupData.mercenaryIndex != -1)
        {
            UnitGroup newUnitGroup = Instantiate(resourceManager.mercenaries[backLineUnit.unitGroupData.mercenaryIndex]);
            newUnitGroup.gameManager = gameManager;
            for (int j = 0; j < newUnitGroup.transform.childCount; j++)
            {
                Unit childUnit = newUnitGroup.transform.GetChild(j).GetComponent<Unit>();
                childUnit.team = Team.Player;
                childUnit.gameManager = gameManager;
                childUnit.group = newUnitGroup;
                newUnitGroup.units.Add(childUnit);
            }
            newUnitGroup.team = Team.Player;
            return (newUnitGroup);
        }
        else
        {
            unitLoadoutData hero = backLineUnit.unitData;
            Unit newHero = Instantiate(resourceManager.emptyHero);
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
            return (newHero);
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
