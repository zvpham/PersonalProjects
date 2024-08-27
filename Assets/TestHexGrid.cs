using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TestHexGrid : MonoBehaviour
{
    GridHex<GridPosition> hexgrid;
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
    }

    // Start is called before the first frame update
    void Start()
    {
        //hexgrid =  new GridHex<GridPosition>(10, 10, 1, new Vector3(-0.5f, -0.5f, 0), (GridHex<GridPosition> Grid, int x, int y) => new GridPosition(Grid, x, y, 3), true);
        CreateGrid(32, 32, 7);
        //map =  new DijkstraMap(10, 10, new Vector3(-0.5f, -0.5f, 0));
    }

    // Update is called once per frame
    void Update()
    {
        SelectMouseHex();


        int initalElevation = gameManager.spriteManager.elevationOfHexes[currentlySelectedHex.x, currentlySelectedHex.y];
        // Raise Elevation
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ChangeElevation(currentlySelectedHex.x, currentlySelectedHex.y, 2, true);
        }
        // Drop Elevation
        else if (Input.GetKeyDown(KeyCode.E))
        {
            ChangeElevation(currentlySelectedHex.x, currentlySelectedHex.y, - 2, true);
        }
        else if(Input.GetKeyDown(KeyCode.PageUp))
        {
            gameManager.spriteManager.SetViewElevation(gameManager.spriteManager.currentViewingElevation + 1);
        }
        else if (Input.GetKeyDown(KeyCode.PageDown))
        {
            gameManager.spriteManager.SetViewElevation(gameManager.spriteManager.currentViewingElevation - 1);
        }
        else if (Input.GetMouseButtonDown(1))
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
                    ChangeElevation(offSetPosition.x, offSetPosition.y, 2, true, false);
                    /*
                    if(i == 0)
                    {
                        Vector2Int offSetPosition = gameManager.spriteManager.spriteGrid.CubeToOffset(cubePath[i]);
                        ChangeElevation(offSetPosition.x, offSetPosition.y, 2, true, false);
                    }
                    else
                    {
                        Vector2Int offSetPosition = gameManager.spriteManager.spriteGrid.CubeToOffset(cubePath[i]);
                        ChangeElevation(offSetPosition.x, offSetPosition.y, 2, true, true);
                    }
                    */
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
    /*
    public void CreateElevationSprite(int x, int y, int elevation, bool playAnimation)
    {

        if(playAnimation)
        {
            TerrainHolder newHex = null;

            newHex = Instantiate(newGroundHexPrefab);
            Vector3 originalHexPosition = hexgrid.GetWorldPosition(x, y);
            newHex.transform.position = originalHexPosition;
            newHex.elevation = elevation;
            newHex.x = x;
            newHex.y = y;
            Debug.Log("Set Hex: " + elevation + ", " + x + ", " + y);
            gameManager.spriteManager.terrain[x, y] = newHex;


            int terrainTileIndex = gameManager.spriteManager.terrainTilePositions[defaultElevation].IndexOf(new Vector2Int(x, y));

            if (terrainTileIndex == -1)
            {
                Debug.LogError("Attempted to ceate a new Elevation Terrain Sprite while sprite for the position exists: " + new Vector2Int(x, y));
            }
            Vector3Int currentNodePosition = new Vector3Int(y, x);
            newHex.sprite.sprite = tileMap.GetSprite(currentNodePosition);
            gameManager.spriteManager.terrain[x, y] = newHex;

        }
        else
        {
            TerrainHolder newHex = null;
            if (elevation == defaultElevation)
            {
                return;
            }
            else if (elevation > defaultElevation)
            {
                newHex = Instantiate(newGroundHexPrefab);
                Vector3 originalHexPosition = hexgrid.GetWorldPosition(x, y);
                newHex.transform.position = originalHexPosition;
                newHex.elevation = elevation;
                newHex.x = x;
                newHex.y = y;
                Debug.Log("Set Hex: " + elevation + ", " + x + ", " + y);
                gameManager.spriteManager.terrain[x, y] = newHex;
            }

            int terrainTileIndex = gameManager.spriteManager.terrainTilePositions[defaultElevation].IndexOf(new Vector2Int(x, y));

            if (terrainTileIndex == -1)
            {
                Debug.LogError("Attempted to ceate a new Elevation Terrain Sprite while sprite for the position exists: " + new Vector2Int(x, y));
            }
            else
            {
                Vector2Int newTileHex = gameManager.spriteManager.terrainTilePositions[defaultElevation][terrainTileIndex];
                gameManager.spriteManager.terrainTilePositions[defaultElevation].RemoveAt(terrainTileIndex);
                gameManager.spriteManager.terrainTilePositions[elevation].Add(newTileHex);
            }
            float newHexHeight = newHex.transform.position.y + ((elevation - defaultElevation) * terrainHeightDifference);
            newHex.transform.position = new Vector3(newHex.transform.position.x, newHexHeight);
            Vector3Int currentNodePosition = new Vector3Int(y, x);
            newHex.sprite.sprite = tileMap.GetSprite(currentNodePosition);
            gameManager.spriteManager.terrain[x, y] = newHex;
        }
    }
    */
    public void ChangeElevation(int x, int y, int elevationChangeAmount, bool playAnimation =  false, bool partOfAGroup = false)
    {
        int initialElevationOfHex = gameManager.spriteManager.elevationOfHexes[x, y];
        int newElevation = initialElevationOfHex + elevationChangeAmount;
        if (newElevation < 0)
        {
            newElevation = 0;
        }
        else if (newElevation > gameManager.spriteManager.terrainTilePositions.Count - 1)
        {
            newElevation = gameManager.spriteManager.terrainTilePositions.Count - 1;
        }
        int realElevationChangeAmount = newElevation - initialElevationOfHex;
        if (realElevationChangeAmount == 0)
        {
            return;
        }
        /*
        if (initialElevationOfHex == defaultElevation && gameManager.spriteManager.terrain[x, y] == null)
        {
            CreateElevationSprite(x, y, newElevation, playAnimation);
        }
        */

        gameManager.spriteManager.terrainIsChangingElevation[x,y] = true;
        TerrainElevationChangeAnimation changeElevation = Instantiate(ChangeElevationAnimation);
        Vector3 currenthexPosition = gameManager.spriteManager.spriteGrid.GetWorldPosition(x,y);
        changeElevation.SetParameters(gameManager, currenthexPosition, (currenthexPosition + (realElevationChangeAmount * new Vector3(0, terrainHeightDifference))), 
            x, y, initialElevationOfHex, newElevation, partOfAGroup);
        int terrainTileIndex = gameManager.spriteManager.terrainTilePositions[initialElevationOfHex].IndexOf(new Vector2Int(x, y));
        Vector2Int newTileHex = gameManager.spriteManager.terrainTilePositions[initialElevationOfHex][terrainTileIndex];
        gameManager.spriteManager.terrainTilePositions[initialElevationOfHex].RemoveAt(terrainTileIndex);
        gameManager.spriteManager.terrainTilePositions[newElevation].Add(newTileHex);

    }

    //public void 
}
