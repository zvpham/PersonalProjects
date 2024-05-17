using CodeMonkey.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHexGrid : MonoBehaviour
{
    GridHex<GridPosition> hexgrid;
    AStarPathfindingHex path;
    DijkstraMap map;
    // Start is called before the first frame update
    void Start()
    {
        hexgrid =  new GridHex<GridPosition>(10, 10, 1, new Vector3(-0.5f, -0.5f, 0), (GridHex<GridPosition> Grid, int x, int y) => new GridPosition(Grid, x, y), true);
        //map =  new DijkstraMap(10, 10, new Vector3(-0.5f, -0.5f, 0));
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            /*
            map.getGrid().GetXY(UtilsClass.GetMouseWorldPosition(), out int x, out int y);
            List<Vector2Int> goal = new List<Vector2Int>();
            goal.Add(new Vector2Int(x, y));
            map.SetGoals(goal);
            */
            hexgrid.GetXY(UtilsClass.GetMouseWorldPosition(), out int x, out int y);
            Debug.Log(x + " " + y);
            List<GridPosition> grids = hexgrid.GetGridObjectsInRing(x, y, 2);
            for(int i = 0; i < grids.Count; i++)
            {
                Debug.Log(grids[i]);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            map.ResetMap();
        }
    }
}
