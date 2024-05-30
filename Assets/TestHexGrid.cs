using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestHexGrid : MonoBehaviour
{
    GridHex<GridPosition> hexgrid;
    AStarPathfindingHex path;
    DijkstraMap map;
    public List<Vector3Int> highlightedHexws = new List<Vector3Int>();
    public Vector3Int cubeStartHex;
    public Vector3Int cubeEndHex;
    public Vector3 currentMouse;
    public Vector3 prevMouse;
    public Tilemap tileMap;
    public ResourceManager resourceManager;
    public LineController lineController;
    // Start is called before the first frame update
    void Start()
    {
        hexgrid =  new GridHex<GridPosition>(10, 10, 1, new Vector3(-0.5f, -0.5f, 0), (GridHex<GridPosition> Grid, int x, int y) => new GridPosition(Grid, x, y), true);
        //map =  new DijkstraMap(10, 10, new Vector3(-0.5f, -0.5f, 0));
    }

    // Update is called once per frame
    void Update()
    {
        currentMouse = UtilsClass.GetMouseWorldPosition();
        if (Input.GetMouseButtonDown(0))
        {
            hexgrid.GetXY(UtilsClass.GetMouseWorldPosition(), out int x, out int y);
            cubeStartHex = hexgrid.OffsetToCube(x, y);
        }

        if(currentMouse != prevMouse)
        {
            prevMouse = currentMouse;
            hexgrid.GetXY(UtilsClass.GetMouseWorldPosition(), out int x, out int y);
            Vector2Int tempCubeOffset = new Vector2Int(x, y);
            cubeEndHex =  hexgrid.OffsetToCube(x,y);
            List<Vector3Int> hexPath = hexgrid.CubeLineDraw(cubeStartHex, cubeEndHex);
            
            for(int i = 0; i < highlightedHexws.Count; i++)
            {
                tileMap.SetTile(highlightedHexws[i], resourceManager.BaseTile[0]);
            }
            highlightedHexws.Clear();

            for(int i = 0; i < hexPath.Count; i++)
            {
                Vector2Int currentHex = hexgrid.CubeToOffset(hexPath[i]);
                Vector3Int adjustedHex =  new Vector3Int(currentHex.y, currentHex.x, 0);
                highlightedHexws.Add(adjustedHex);
                tileMap.SetTile(adjustedHex, resourceManager.BaseTile[1]);
            }

            List<Vector3> line = new List<Vector3>();
            line.Add(hexgrid.GetWorldPosition(cubeStartHex.x, cubeStartHex.y));
            line.Add(hexgrid.GetWorldPosition(tempCubeOffset.x, tempCubeOffset.y));
            lineController.SetLine(line);
        }
    }
}
