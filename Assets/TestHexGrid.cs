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
            cubeStartHex = new Vector2Int(x, y);
        }

        if (Input.GetMouseButtonDown(2))
        {
            Debug.Log(Mathf.DeltaAngle(firstAngle, lastAngle));
        }

        if(currentMouse != prevMouse && Input.GetMouseButtonDown(1))
        {
            prevMouse = currentMouse;
            hexgrid.GetXY(UtilsClass.GetMouseWorldPosition(), out int x, out int y);
            Vector2Int tempCubeOffset = new Vector2Int(x, y);
            cubeEndHex = new Vector2Int(x, y);

            Vector3 startPosition = hexgrid.GetWorldPosition(cubeStartHex);
            Vector3 endPosition = hexgrid.GetWorldPosition(cubeEndHex);

            Debug.Log("Angle: " +  (Mathf.Atan2(endPosition.y - startPosition.y, endPosition.x - startPosition.x) * Mathf.Rad2Deg))  ;

            if(index == 0)
            {
                firstAngle = (Mathf.Atan2(endPosition.y - startPosition.y, endPosition.x - startPosition.x) * Mathf.Rad2Deg);
                firstAngle = Mathf.RoundToInt(firstAngle);
                index = 1;
            }
            else if(index == 1)
            {
                lastAngle = (Mathf.Atan2(endPosition.y - startPosition.y, endPosition.x - startPosition.x) * Mathf.Rad2Deg);
                lastAngle = Mathf.RoundToInt(lastAngle);
                index = 0;
            }

            /*
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
            */
        }
    }
}
