using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TestHexGrid : MonoBehaviour
{
    GridHex<GridPosition> hexgrid;
    public PrefabTerrain prefabTerrain;
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

    // Start is called before the first frame update
    void Start()
    {
        //hexgrid =  new GridHex<GridPosition>(10, 10, 1, new Vector3(-0.5f, -0.5f, 0), (GridHex<GridPosition> Grid, int x, int y) => new GridPosition(Grid, x, y, 3), true);
        CreateGrid(32, 32, 7);
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
