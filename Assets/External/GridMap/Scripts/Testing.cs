using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using CodeMonkey;

public class Testing : MonoBehaviour {

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

}
