using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEditor;
using UnityEngine;

public class PlayerWorldMap : MonoBehaviour
{
    public WorldMap worldMap;
    public GameObject[,] tileobs;
    public int gridSize = 1;
    public GameObject tiles;
    public ResourceManager resourceManager;
    public void Clear()
    {
        DestroyImmediate(tiles);
    }

    public void Load()
    {
        if(worldMap.width % 3 != 0)
        {
            Debug.LogError("World Map width need to be divisible by 3");
            return;
        }

        if (worldMap.height % 3 != 0)
        {
            Debug.LogError("World Map height need to be divisible by 3");
            return;
        }

        int playerWorldMapLength = worldMap.width / 3;
        int playerWorldMapHeight = worldMap.height / 3;
        tileobs =  new GameObject[playerWorldMapLength, playerWorldMapHeight];

        Clear();
        GameObject o;
        if (tiles == null)
        {
            tiles = new GameObject("tiles");
            tiles.transform.parent = this.gameObject.transform;
            tiles.transform.localPosition = new Vector3();
        }

        // TIleINfO - <x, y, TilePrefabIndex>
        Vector3Int tileInfo;
        int xIndex = 0;
        for (int i = worldMap.width + 1; i < worldMap.tilesInspectorUse.Count; i += 3)
        {
            tileInfo = worldMap.tilesInspectorUse[i];
            o = Instantiate(resourceManager.tileBasePrefabs[tileInfo.z], new Vector3(), new Quaternion(0, 0, 0, 1f));
            o.transform.parent = tiles.transform;
            o.transform.localPosition = (new Vector3(tileInfo.x / 3, tileInfo.y / 3, 0) * gridSize);
            tileobs[tileInfo.x / 3, tileInfo.y / 3] = o;
            if (xIndex == playerWorldMapLength - 1)
            {
                i += worldMap.width * 2;
                xIndex = 0;
            }
            else
            {
                xIndex += 1;
            }
        }
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerWorldMap))]
public class PlayerWorldMapEditor : Editor
{

    public override void OnInspectorGUI()
    {
        PlayerWorldMap me = (PlayerWorldMap)target;
        if (GUILayout.Button("LOAD"))
        {
            me.Load();
        }
        DrawDefaultInspector();
    }

    void DrawEvents()
    {
        Handles.BeginGUI();
        Handles.EndGUI();
    }
}
#endif

