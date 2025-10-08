using System;
using System.IO;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider))]
public class PrefabMapTilePainter : MonoBehaviour
{
    public ResourceManager resourceManager;
    public TilePrefabBase tilePremade;
    public GameObject emptyPrefab;
    public GameObject roomPrefab;
    public int gridsize = 1;
    public int width = 20;
    public int height = 20;
    public GameObject tiles;
    private bool _changed = true;
    public Vector3 cursor;
    public bool focused = false;
    public GameObject[,] tileobs;
    public List<Tuple<int, int, int>> finalRenderLocations = new List<Tuple<int, int, int>>();
    public List<WallStates> finalRenderStates = new List<WallStates>();


    int colidx = 0;
    public List<UnityEngine.Object> palette = new List<UnityEngine.Object>();
    public UnityEngine.Object color = null;
    Quaternion color_rotation;


    public GameObject GetTile(int x, int y)
    {
        return tileobs[x, y];
    }

#if UNITY_EDITOR

    private static bool IsAssetAFolder(UnityEngine.Object obj)
    {
        string path = "";
        if (obj == null) { return false; }
        path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
        if (path.Length > 0)
        {
            if (Directory.Exists(path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }


    public void Encode()
    {

    }

    static GameObject CreatePrefab(UnityEngine.Object fab, Vector3 pos, Quaternion rot)
    {
        GameObject o = PrefabUtility.InstantiatePrefab(fab as GameObject) as GameObject;
        if (o == null)
        {
            Debug.Log(IsAssetAFolder(fab));
            return o;
        }
        o.transform.position = pos;
        o.transform.rotation = rot;
        return o;
    }

    public void Restore()
    {

        Transform palt = transform.Find("palette");
        if (palt != null) { GameObject.DestroyImmediate(palt.gameObject); }
        GameObject pal = new GameObject("palette");
        pal.hideFlags = HideFlags.HideInHierarchy;
        BoxCollider bc = pal.AddComponent<BoxCollider>();
        bc.size = new Vector3(palette.Count * gridsize, gridsize, 0f);
        bc.center = new Vector3((palette.Count - 1f) * gridsize * 0.5f, 0f, 0f);

        pal.transform.parent = this.gameObject.transform;
        pal.transform.localPosition = new Vector3(0f, -gridsize * 2, 0f);
        pal.transform.rotation = transform.rotation;



        int palette_folder = -1;

        for (int i = 0; i < palette.Count; i++)
        {
            UnityEngine.Object o = palette[i];
            if (IsAssetAFolder(o))
            {
                palette_folder = i;
            }
            else
            {
                if (o != null)
                {
                    GameObject g = CreatePrefab(o, new Vector3(), transform.rotation);
                    g.transform.parent = pal.transform;
                    g.transform.localPosition = new Vector3(i * gridsize, 0f, 0f);
                }
            }
        }

        if (palette_folder != -1)
        {
            string path = AssetDatabase.GetAssetPath(palette[palette_folder].GetInstanceID());
            path = path.Trim().Replace("Assets/Resources/", "");
            palette.RemoveAt(palette_folder);
            UnityEngine.Object[] contents = (UnityEngine.Object[])Resources.LoadAll(path);
            foreach (UnityEngine.Object o in contents)
            {
                if (!palette.Contains(o)) { palette.Add(o); }
            }
            Restore();
        }

        tileobs = new GameObject[width, height];
        if (tiles == null)
        {
            tiles = new GameObject("tiles");
            tiles.transform.parent = this.gameObject.transform;
            tiles.transform.localPosition = new Vector3();
        }
        int cnt = tiles.transform.childCount;
        List<GameObject> trash = new List<GameObject>();
        for (int i = 0; i < cnt; i++)
        {
            GameObject tile = tiles.transform.GetChild(i).gameObject;
            Vector3 tilepos = tile.transform.localPosition;
            int X = (int)(tilepos.x / gridsize);
            int Y = (int)(tilepos.y / gridsize);
            if (ValidCoords(X, Y))
            {
                tileobs[X, Y] = tile;
            }
            else
            {
                trash.Add(tile);
            }
        }
        for (int i = 0; i < trash.Count; i++)
        {
            if (Application.isPlaying) { Destroy(trash[i]); } else { DestroyImmediate(trash[i]); }
        }

        if (color == null)
        {
            if (palette.Count > 0)
            {
                color = palette[0];
            }
        }
    }

    public void Resize()
    {
        transform.localScale = new Vector3(1, 1, 1);
        if (_changed)
        {
            _changed = false;
            Restore();
        }
    }

    public void Awake()
    {
        Restore();
    }

    public void OnEnable()
    {
        Restore();
    }

    void OnValidate()
    {
        _changed = true;
        BoxCollider bounds = this.GetComponent<BoxCollider>();
        bounds.center = new Vector3((width * gridsize) * 0.5f - gridsize * 0.5f, (height * gridsize) * 0.5f - gridsize * 0.5f, 0f);
        bounds.size = new Vector3(width * gridsize, (height * gridsize), 0f);
    }

    public Vector3 GridV3(Vector3 pos)
    {
        Vector3 p = transform.InverseTransformPoint(pos) + new Vector3(gridsize * 0.5f, gridsize * 0.5f, 0f);
        return new Vector3((int)(p.x / gridsize), (int)(p.y / gridsize), 0);
    }

    public bool ValidCoords(int x, int y)
    {
        if (tileobs == null) { return false; }

        return (x >= 0 && y >= 0 && x < tileobs.GetLength(0) && y < tileobs.GetLength(1));
    }


    public void CycleColor()
    {
        colidx += 1;
        if (colidx >= palette.Count)
        {
            colidx = 0;
        }
        color = (UnityEngine.Object)palette[colidx];
    }

    public void Turn()
    {
        if (this.ValidCoords((int)cursor.x, (int)cursor.y))
        {
            GameObject o = tileobs[(int)cursor.x, (int)cursor.y];
            if (o != null)
            {
                o.transform.Rotate(0f, 0f, 90f);
            }
        }
    }

    public Vector3 Local(Vector3 p)
    {
        return this.transform.TransformPoint(p);
    }

    public UnityEngine.Object PrefabSource(GameObject o)
    {
        if (o == null)
        {
            return null;
        }
        UnityEngine.Object fab = PrefabUtility.GetCorrespondingObjectFromSource(o);
        if (fab == null)
        {
            Debug.Log("Test1");
            fab = Resources.Load(o.name);
        }
        if (fab == null)
        {
            Debug.Log("Test1");
            fab = palette[0];
        }
        return fab;
    }

    public void Drag(Vector3 mouse, PrefabTileLayerEditor.TileOperation op)
    {
        Resize();
        if (tileobs == null) { Restore(); }
        if (this.ValidCoords((int)cursor.x, (int)cursor.y))
        {
            if (op == PrefabTileLayerEditor.TileOperation.Sampling)
            {
                UnityEngine.Object s = PrefabSource(tileobs[(int)cursor.x, (int)cursor.y]);
                Debug.Log(s);
                if (s != null)
                {
                    color = s;
                    color_rotation = tileobs[(int)cursor.x, (int)cursor.y].transform.localRotation;
                }
            }
            else
            {
                DestroyImmediate(tileobs[(int)cursor.x, (int)cursor.y]);
                if (op == PrefabTileLayerEditor.TileOperation.Drawing)
                {
                    if (color == null) { return; }
                    GameObject o = CreatePrefab(color, new Vector3(), color_rotation);
                    o.transform.parent = tiles.transform;
                    o.transform.localPosition = (cursor * gridsize);
                    o.transform.localRotation = color_rotation;
                    tileobs[(int)cursor.x, (int)cursor.y] = o;
                }
            }
        }
        else
        {
            if (op == PrefabTileLayerEditor.TileOperation.Sampling)
            {
                if (cursor.y == -1 && cursor.x >= 0 && cursor.x < palette.Count)
                {
                    color = palette[(int)cursor.x];
                    color_rotation = Quaternion.identity;
                }
            }
        }
    }

    public void Clear()
    {
        tileobs = new GameObject[width, height];
        DestroyImmediate(tiles);
        tiles = new GameObject("tiles");
        tiles.transform.parent = gameObject.transform;
        tiles.transform.localPosition = new Vector3();
    }

    public void Compile()
    {
        List<Vector3Int> walls =  new List<Vector3Int> ();
        string debugWord = "";
        List<string> debugWorld = new List<string>();
        for (int i = 0; i < height; i++)
        {
            debugWord = "";
            for (int j = 0; j < width; j++)
            {
                PrefabMapTile tempTile = tileobs[j, i].GetComponent<PrefabMapTile>();
                walls.Add(new Vector3Int(j, i, tempTile.wallIndex));
                // Case - Empty
                if(tempTile.wallIndex == 0) 
                {
                    debugWord += "E ";
                } 
                // Case - Room
                else if(tempTile.wallIndex == 1)
                {
                    debugWord += "R ";
                }
                // Case  - Wall
                else
                {
                    // The negative 2 Accounts for the Empty and Room Tile in Resource Manager
                    RenderWall(j, i, tempTile.wallIndex - 2);
                    debugWord += "W ";
                } 
            }
            debugWorld.Add(debugWord);
        }
        FinalRender();
        for (int i = debugWorld.Count - 1; i >= 0; i--)
        {
            //Debug.Log(debugWorld[i]);
        }
        tilePremade.mapTiles = walls;
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(tilePremade);
        AssetDatabase.SaveAssets(); 
    }

    public void LoadPrefabMapTile()
    {
        Clear();
        GameObject o;
        // TIleINfO - <x, y, WallIndex>
        Vector3Int tileInfo;
        for (int i = 0; i < tilePremade.mapTiles.Count; i++)
        {
            tileInfo = tilePremade.mapTiles[i];
            o = CreatePrefab(resourceManager.walls[tileInfo.z], new Vector3(), new Quaternion(0, 0, 0, 1f));
            o.transform.parent = tiles.transform;
            o.transform.localPosition = (new Vector3(tileInfo.x, tileInfo.y, 0) * gridsize);
            o.transform.localRotation = color_rotation;
            tileobs[tileInfo.x, tileInfo.y] = o;
        }
    }
    
    public void BlankCanvas()
    {
        Clear();
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                GameObject o;
                o = CreatePrefab(resourceManager.walls[0], new Vector3(), new Quaternion(0, 0, 0, 1f));
                o.transform.parent = tiles.transform;
                o.transform.localPosition = (new Vector3(j, i, 0) * gridsize);
                o.transform.localRotation = color_rotation;
                tileobs[j, i] = o;
            }
        }
    }

    public void FinalRender()
    {
        int x = -1;
        int y = -1;
        int wallIndex = -1;
        for (int i = 0; i < finalRenderLocations.Count; i++)
        {
            x = finalRenderLocations[i].Item1;
            y = finalRenderLocations[i].Item2;
            wallIndex = finalRenderLocations[i].Item3;
            switch (tileobs[x, y].GetComponent<PrefabMapTile>().wallState)
            {
                case (WallStates.EastWall):
                    if(tileobs[x - 1, y].GetComponent<PrefabMapTile>().wallState == WallStates.CenterHorzontalWall ||
                       tileobs[x - 1, y].GetComponent<PrefabMapTile>().wallState == WallStates.LeftHorzontalWall)
                    {
                        tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].EastThreeWay;
                    }
                    else if (tileobs[x - 1, y + 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty)
                    {
                        tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].EastModifiedLowerConnector;
                    }
                    else if (tileobs[x - 1, y - 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty)
                    {
                        tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].EastModifiedUpperConnector;
                    }
                    break;
                case (WallStates.WestWall):
                    if (tileobs[x + 1, y].GetComponent<PrefabMapTile>().wallState == WallStates.CenterHorzontalWall ||
                        tileobs[x + 1, y].GetComponent<PrefabMapTile>().wallState == WallStates.RightHorzontalWall)
                    {
                        tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].WestThreeWay;
                    }
                    else if(tileobs[x + 1, y + 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty)
                    {
                        tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].WestModifiedLowerConnector;
                    }
                    else if(tileobs[x + 1, y - 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty)
                    {
                        tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].WestModifiedUpperConnector;
                    }
                    break;
                case (WallStates.SouthWall):
                    if (tileobs[x + 1, y + 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty ||
                        tileobs[x - 1, y + 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty)
                    {
                        if(tileobs[x + 1, y + 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty &&
                        tileobs[x - 1, y + 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty)
                        {
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthWallThreeWay;
                        }
                        else if(tileobs[x + 1, y + 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty)
                        {
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthModifiedRightSmallConnector;
                        }
                        else
                        {
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthModifiedLeftSmallConnector;
                        }
                    }
                    break;
                case (WallStates.NorthWall):
                    if (tileobs[x + 1, y - 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty ||
                        tileobs[x - 1, y - 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty)
                    {
                        if (tileobs[x + 1, y - 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty &&
                        tileobs[x - 1, y - 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty)
                        {
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthWallThreeWay;
                        }
                        else if (tileobs[x + 1, y - 1].GetComponent<PrefabMapTile>().wallState == WallStates.Empty)
                        {
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthModifiedRightSmallConnector;
                        }
                        else
                        {
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthModifiedLeftSmallConnector;
                        }
                    }
                    break;
                case (WallStates.CenterWall):
                    int numWallConnections = 0;

                    if (tileobs[x + 1, y + 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                    {
                        //NorthEast Wall
                        numWallConnections += 1;
                    }
                    if (tileobs[x - 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                    {
                        //SouthWest Wall
                        numWallConnections += 2;
                    }
                    if (tileobs[x - 1, y + 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                    {
                        //is NorthWest Wall
                        numWallConnections += 4;
                    }
                    if (tileobs[x + 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                    {
                        //is SouthEast
                        numWallConnections += 8;
                    }
                    switch (numWallConnections)
                    {
                        case 0:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterFourWay;
                            break;
                        case 1:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthEastConnector;
                            break;
                        case 2:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterSouthWestConnector;
                            break;
                        case 3:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthEastToSouthWest;
                            break;
                        case 4:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthWestConnector;
                            break;
                        case 5:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthThreeWay;
                            break;
                        case 6:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterWestThreeWay;
                            break;
                        case 7:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthSmallRightConnectorWall;
                            break;
                        case 8:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterSouthEastConnector;
                            break;
                        case 9:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterEastThreeWay;
                            break;
                        case 10:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterSouthThreeWay;
                            break;
                        case 11:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterSouthSmallLeftConnectorWall;
                            break;
                        case 12:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthWestToSouthEast;
                            break;  
                        case 13:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterNorthSmallLeftConnectorWall;
                            break;
                        case 14:
                            tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterSouthSmallRightConnectorWall;
                            break;
                        case 15:
                            break;
                    }
                    break;

            }
        }

    }

    public void RenderWall(int x, int y, int wallIndex)
    {
        int numWallConnections = 0;

        if (x + 1 < width && tileobs[x + 1, y].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
        {
            //East Wall
            numWallConnections += 1;
        }
        if (x - 1 >= 0 && tileobs[x - 1, y].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
        {
            //West Wall
            numWallConnections += 2;
        }
        if (y + 1 < height && tileobs[x, y + 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
        {
            //is NorthWall
            numWallConnections += 4;
        }
        if (y - 1 >= 0 && tileobs[x, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
        {
            //is SouthWall
            numWallConnections += 8;
        }

        switch(numWallConnections)
        {
            case 0:
                tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SoleWall;
                tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.SoleWall;
                break;
            case 1:
                tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].LeftHorzontalWall;
                tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.LeftHorzontalWall;
                finalRenderLocations.Add(new Tuple<int, int, int>(x + 1, y, wallIndex));
                break;
            case 2:
                tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].RightHorzontalWall;
                tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.RightHorzontalWall;
                finalRenderLocations.Add(new Tuple<int, int, int>(x - 1, y, wallIndex));
                break;
            case 3:
                tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterHorzontalWall;
                tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.CenterHorzontalWall;
                finalRenderLocations.Add(new Tuple<int, int, int>(x - 1, y, wallIndex));
                finalRenderLocations.Add(new Tuple<int, int, int>(x + 1, y, wallIndex));
                break;
            case 4:
                tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].LowerVerticalWall;
                tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.LowerVerticalWall;
                finalRenderLocations.Add(new Tuple<int, int, int>(x, y + 1, wallIndex));
                break;
            case 5:
                if (tileobs[x + 1, y + 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                {
                    tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthWestWall;
                    tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.SouthWestWall;
                    if (y - 1 > 0 && tileobs[x + 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x + 1, y, wallIndex));
                    }
                }
                else
                {
                    tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthWestWallModified;
                    tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.SouthWestWallModified;
                    if (y - 1 > 0 && tileobs[x + 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x + 1, y, wallIndex));
                    }
                }
                break;
            case 6:
                if(tileobs[x - 1, y + 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                {
                    tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthEastWall;
                    tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.SouthEastWall;
                    if (y - 1 > 0 && tileobs[x - 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x - 1, y, wallIndex));
                    }

                }
                else
                {
                    tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthEastWallModified;
                    tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.SouthEastWallModified;
                    if (y - 1 > 0 && tileobs[x - 1, y - 1].GetComponent<PrefabMapTile>().wallState != WallStates.Empty)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x - 1, y, wallIndex));
                    }
                }
                break;
            case 7:
                tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].SouthWall;
                tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.SouthWall;
                if(y - 1 < 0)
                {
                    break;
                }
                if (tileobs[x + 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x + 1, y, wallIndex));
                }
                if (y + 1 < height && tileobs[x - 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x - 1, y, wallIndex));
                }
                break;
            case 8:
                tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].UpperVerticalWall;
                tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.UpperVerticalWall;
                finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                break;
            case 9:
                if (tileobs[x + 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                {
                    tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthWestWall;
                    tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.NorthWestWall;
                    if (x - 1 > 0 && tileobs[x - 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                    }
                }
                else
                {
                    tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthWestWallModified;
                    tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.NorthWestWallModified;
                    if (x - 1 > 0 && tileobs[x - 1, y - 1].GetComponent<PrefabMapTile>().wallState != WallStates.Empty)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                    }
                }
                break;
            case 10:
                if (tileobs[x - 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                {
                    tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthEastWall;
                    tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.NorthEastWall;
                    if (tileobs[x + 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState != MapGenerationStates.Wall)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                    }
                    if(y + 1 < height && tileobs[x - 1, y + 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x - 1, y, wallIndex));
                    }
                }
                else
                {
                    tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthEastWallModified;
                    tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.NorthEastWallModified;
                    if (x + 1 < width && tileobs[x + 1, y - 1].GetComponent<PrefabMapTile>().wallState != WallStates.Empty)
                    {
                        finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                    }
                }
                break;
            case 11:
                tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].NorthWall;
                tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.NorthWall;
                break;
            case 12:
                tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterVerticalWall;
                tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.CenterVerticalWall;
                if (x + 1 >= width || x - 1 < 0)
                {
                    break;
                }
                if (tileobs[x - 1, y + 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall &&
                    tileobs[x + 1, y + 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x, y + 1, wallIndex));
                }
                if (tileobs[x - 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall &&
                    tileobs[x + 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                }
                break;
            case 13:
                tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].WestWall;
                tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.WestWall;
                if (x - 1 > 0 && y - 1 > 0 && tileobs[x - 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                }
                if (tileobs[x + 1, y + 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Empty)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x, y, wallIndex));
                }
                break;
            case 14:
                tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].EastWall;
                tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.EastWall;
                if (x + 1 < width && y - 1 > 0 && tileobs[x - 1, y - 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Wall)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x, y - 1, wallIndex));
                }
                if (tileobs[x - 1, y + 1].GetComponent<PrefabMapTile>().mapGenerationState == MapGenerationStates.Empty)
                {
                    finalRenderLocations.Add(new Tuple<int, int, int>(x, y, wallIndex));
                }
                break;
            case 15:
                tileobs[x, y].GetComponent<SpriteRenderer>().sprite = resourceManager.wallSprites[wallIndex].CenterWall;
                tileobs[x, y].GetComponent<PrefabMapTile>().wallState = WallStates.CenterWall;
                break;
        }
    }   

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.matrix = transform.localToWorldMatrix;
        if (focused)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.6f);
            Gizmos.DrawRay((cursor * gridsize) + Vector3.forward * -49999f, Vector3.forward * 99999f);
            Gizmos.DrawRay((cursor * gridsize) + Vector3.right * -49999f, Vector3.right * 99999f);
            Gizmos.DrawRay((cursor * gridsize) + Vector3.up * -49999f, Vector3.up * 99999f);
            Gizmos.color = Color.yellow;
        }

        Gizmos.DrawWireCube(new Vector3((width * gridsize) * 0.5f - gridsize * 0.5f, (height * gridsize) * 0.5f - gridsize * 0.5f, 0f),
            new Vector3(width * gridsize, (height * gridsize), 0f));
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(PrefabMapTilePainter))]
public class PrefabTileLayerEditor : Editor
{
    public enum TileOperation { None, Drawing, Erasing, Sampling };
    private TileOperation operation;

    public override void OnInspectorGUI()
    {
        PrefabMapTilePainter me = (PrefabMapTilePainter)target;
        GUILayout.Label("Assign a prefab to the color property");
        GUILayout.Label("or the pallete array.");
        GUILayout.Label("drag        : paint tiles");
        GUILayout.Label("[s]+click  : sample tile color");
        GUILayout.Label("[x]+drag  : erase tiles");
        GUILayout.Label("[space]    : rotate tile");
        GUILayout.Label("[b]          : cycle color");
        if (GUILayout.Button("CLEAR"))
        {
            me.Clear();
        }
        if (GUILayout.Button("Blank Canvas"))
        {
            me.BlankCanvas();
        }
        if (GUILayout.Button("COMPILE"))
        {
            me.Compile();
        }
        if (GUILayout.Button("LOAD"))
        {
            me.LoadPrefabMapTile();
        }
        DrawDefaultInspector();
    }

    private bool AmHovering(Event e)
    {
        PrefabMapTilePainter me = (PrefabMapTilePainter)target;
        RaycastHit hit;
        if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit, Mathf.Infinity) &&
                hit.collider.GetComponentInParent<PrefabMapTilePainter>() == me)
        {
            me.cursor = me.GridV3(hit.point);
            me.focused = true;

            Renderer rend = me.gameObject.GetComponentInChildren<Renderer>();
            if (rend) EditorUtility.SetSelectedRenderState(rend, EditorSelectedRenderState.Wireframe);
            return true;
        }
        me.focused = false;
        return false;
    }

    public void ProcessEvents()
    {
        PrefabMapTilePainter me = (PrefabMapTilePainter)target;
        int controlID = GUIUtility.GetControlID(1778, FocusType.Passive);
        EditorWindow currentWindow = EditorWindow.mouseOverWindow;
        if (currentWindow && AmHovering(Event.current))
        {
            Event current = Event.current;
            bool leftbutton = (current.button == 0);
            switch (current.type)
            {
                case EventType.KeyDown:

                    if (current.keyCode == KeyCode.S) operation = TileOperation.Sampling;
                    if (current.keyCode == KeyCode.X) operation = TileOperation.Erasing;
                    current.Use();
                    return;
                case EventType.KeyUp:
                    operation = TileOperation.None;
                    if (current.keyCode == KeyCode.Space) me.Turn();
                    if (current.keyCode == KeyCode.B) me.CycleColor();
                    current.Use();
                    return;
                case EventType.MouseDown:
                    if (leftbutton)
                    {
                        if (operation == TileOperation.None)
                        {
                            operation = TileOperation.Drawing;
                        }
                        me.Drag(current.mousePosition, operation);

                        current.Use();
                        return;
                    }
                    break;
                case EventType.MouseDrag:
                    if (leftbutton)
                    {
                        if (operation != TileOperation.None)
                        {
                            me.Drag(current.mousePosition, operation);
                            current.Use();
                        }

                        return;
                    }
                    break;
                case EventType.MouseUp:
                    if (leftbutton)
                    {
                        operation = TileOperation.None;
                        current.Use();
                        return;
                    }
                    break;
                case EventType.MouseMove:
                    me.Resize();
                    current.Use();
                    break;
                case EventType.Repaint:
                    break;
                case EventType.Layout:
                    HandleUtility.AddDefaultControl(controlID);
                    break;
            }
        }
    }

    void OnSceneGUI()
    {
        ProcessEvents();
    }

    void DrawEvents()
    {
        Handles.BeginGUI();
        Handles.EndGUI();
    }
}
#endif

