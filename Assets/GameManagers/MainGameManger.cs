using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameManger : MonoBehaviour
{
    public AStarPathfinding path;
    public Grid<Wall>[,] walls;
    public Grid<Unit>[,] units;
    public int mapWidth;
    public int mapHeight;

    public static MainGameManger Instance;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Found more than ONe Data Persistence Manager in the Scence");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }
    
    public void CreateAStarPathing(int width, int height, int initialMapWidth, int initialMapHeight, Grid<Wall>[,] initialWalls, Grid<Unit>[,] initialUnits)
    {
        walls = initialWalls;
        units = initialUnits;
        mapWidth = initialMapWidth;
        mapHeight = initialMapHeight;
        path = new AStarPathfinding(width, height, mapWidth, mapHeight, walls, units, Vector3.zero);
    }

    public void UpdatePathFindingGrid(Vector3 worldPosition, Grid<Wall> wall, Grid<Unit> unit)
    {
        Grid<AStarPathNode> AStarGrid = path.GetGrid();
        int gridXIndex = (int)(worldPosition.x / mapWidth);
        int gridYIndex = (int)(worldPosition.y / mapHeight);
        walls[gridXIndex, gridYIndex] = wall;
        units[gridXIndex, gridYIndex] = unit;
        int x;
        int y;
        AStarGrid.GetXY(worldPosition, out x, out y);
        AStarPathNode node = new AStarPathNode(AStarGrid, x, y, mapWidth, mapHeight, walls, units);
        AStarGrid.SetGridObject(worldPosition, node);
    }
}
