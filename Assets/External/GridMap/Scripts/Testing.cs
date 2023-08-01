using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using CodeMonkey;

public class Testing : MonoBehaviour {
    /*
    [SerializeField] private TilemapVisual tilemapVisual;
    private TilemapSetUp tilemap;
    private TilemapSetUp.TilemapObject.TilemapSprite tilemapSprite;

    private void Start() {
        tilemap = new TilemapSetUp(20, 10, 20f, Vector3.zero);

        tilemap.SetTilemapVisual(tilemapVisual);
    }

    private void Update() {
        if (Input.GetMouseButton(0)) {
            Vector3 mouseWorldPosition = UtilsClass.GetMouseWorldPosition();
            tilemap.SetTilemapSprite(mouseWorldPosition, tilemapSprite);
        }
        
        if (Input.GetKeyDown(KeyCode.T)) {
            tilemapSprite = TilemapSetUp.TilemapObject.TilemapSprite.None;
            CMDebug.TextPopupMouse(tilemapSprite.ToString());
        }
        if (Input.GetKeyDown(KeyCode.Y)) {
            tilemapSprite = TilemapSetUp.TilemapObject.TilemapSprite.Ground;
            CMDebug.TextPopupMouse(tilemapSprite.ToString());
        }
        if (Input.GetKeyDown(KeyCode.U)) {
            tilemapSprite = TilemapSetUp.TilemapObject.TilemapSprite.Path;
            CMDebug.TextPopupMouse(tilemapSprite.ToString());
        }
        if (Input.GetKeyDown(KeyCode.I)) {
            tilemapSprite = TilemapSetUp.TilemapObject.TilemapSprite.Dirt;
            CMDebug.TextPopupMouse(tilemapSprite.ToString());
        }

        
        if (Input.GetKeyDown(KeyCode.P)) {
            tilemap.Save();
            CMDebug.TextPopupMouse("Saved!");
        }
        if (Input.GetKeyDown(KeyCode.L)) {
            tilemap.Load();
            CMDebug.TextPopupMouse("Loaded!");
        }
    }
    */
    private AStarPathfinding pathfinding;
    private void Start()
    {
         pathfinding = new AStarPathfinding(10, 10);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPosition = UtilsClass.GetMouseWorldPosition();
            pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
            List<AStarPathNode> path = pathfinding.FindPath(0,0, x, y);
            Debug.Log(path);
            if(path != null)
            {
                for(int i = 0; i < path.Count - 1; i++)
                {
                    Debug.Log("test");
                    Debug.DrawLine(new Vector3(path[i].x, path[i].y) * 10f + Vector3.one * 5f, new Vector3(path[i + 1].x, path[i + 1].y) * 10f + Vector3.one * 5f, Color.green);
                }
            }

        }
    }
}
