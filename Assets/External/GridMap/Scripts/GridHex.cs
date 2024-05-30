/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using UnityEngine;
using CodeMonkey.Utils;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using System.Collections.Generic;
using System.Linq;

public class GridHex<TGridObject>
{

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private float cellSize;
    public Vector3 originPosition;
    private TGridObject[,] gridArray;
    // TR, BR, B, BL, TL, T 
    public Vector3Int[] cubeDirectionVectors = new Vector3Int[6]
    {new Vector3Int(+1, 0, -1), new Vector3Int(+1, -1, 0), new Vector3Int(0, -1, +1),
    new Vector3Int(-1, 0, +1), new Vector3Int(-1, +1, 0), new Vector3Int(0, +1, -1) };
    public GridHex(int width, int height, float cellSize, Vector3 originPosition, Func<GridHex<TGridObject>, int, int, TGridObject> createGridObject, bool showthisSpecificGrid = false)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        bool showDebug = true;
        if (showDebug && showthisSpecificGrid)
        {
            TextMesh[,] debugTextArray = new TextMesh[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    debugTextArray[x, y] = UtilsClass.CreateWorldText(gridArray[x, y]?.ToString(), null, GetWorldPosition(x, y), 30, Color.white, TextAnchor.MiddleCenter);
                    Vector3 cellPosition = GetWorldPosition(x, y);
                    //Draw Top 3 Sides of a Hexagon
                    Debug.DrawLine(cellPosition + new Vector3(-cellSize / 2, 0, 0), cellPosition + new Vector3(-cellSize / 4, (cellSize / 4) * (float)Math.Sqrt(3), 0), Color.white, 100f);
                    Debug.DrawLine(cellPosition + new Vector3(-cellSize / 4, (cellSize / 4) * (float)Math.Sqrt(3), 0), cellPosition + new Vector3(cellSize / 4, (cellSize / 4) * (float)Math.Sqrt(3), 0), Color.white, 100f);
                    Debug.DrawLine(cellPosition + new Vector3(cellSize / 4, (cellSize / 4) * (float)Math.Sqrt(3), 0), cellPosition + new Vector3(cellSize / 2, 0, 0), Color.white, 100f);
                }
            }

            for(int x = 0; x < gridArray.GetLength(0); x++)
            {
                Vector3 cellPosition = GetWorldPosition(x, 0);
                Debug.DrawLine(cellPosition + new Vector3(-cellSize / 4, -(cellSize / 4) * (float)Math.Sqrt(3), 0), cellPosition + new Vector3(cellSize / 4, -(cellSize / 4) * (float)Math.Sqrt(3), 0), Color.white, 100f);
                if(x % 2 == 0)
                {
                    Debug.DrawLine(cellPosition + new Vector3(-cellSize / 2, 0, 0), cellPosition + new Vector3(-cellSize / 4, -(cellSize / 4) * (float)Math.Sqrt(3), 0), Color.white, 100f);
                    Debug.DrawLine(cellPosition + new Vector3(cellSize / 4, -(cellSize / 4) * (float)Math.Sqrt(3), 0), cellPosition + new Vector3(cellSize / 2, 0, 0), Color.white, 100f);
                }
            }

            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                Vector3 cellPosition = GetWorldPosition(0, y);
                Debug.DrawLine(cellPosition + new Vector3(-cellSize / 2, 0, 0), cellPosition + new Vector3(-cellSize / 4, -(cellSize / 4) * (float)Math.Sqrt(3), 0), Color.white, 100f);
                cellPosition = GetWorldPosition(gridArray.GetLength(1) - 1, y);
                Debug.DrawLine(cellPosition + new Vector3(cellSize / 4, -(cellSize / 4) * (float)Math.Sqrt(3), 0), cellPosition + new Vector3(cellSize / 2, 0, 0), Color.white, 100f);
            }

            OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) => {
                debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };
        }
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public Vector3 GetWorldPosition(Vector2Int hex)
    {
        return GetWorldPosition(hex.x, hex.y);
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        float xPos = x * .75f * cellSize;
        float yPos;
        if(x % 2 == 0)
        {
            yPos = y  * cellSize * ((float)Math.Sqrt(3) / 2);
        }
        else
        {
            yPos = (y + 0.5f) * cellSize * (float)Math.Sqrt(3) / 2;
        }
        return new Vector3(xPos, yPos) + originPosition;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        int roughX = Mathf.RoundToInt((worldPosition - originPosition).x / (cellSize * .75f));
        int roughY;
        if (roughX % 2 == 0)
        {
            roughY = Mathf.RoundToInt((worldPosition - originPosition).y / (cellSize * ((float)Math.Sqrt(3) / 2)));
        }
        else
        {
            roughY = Mathf.RoundToInt(((worldPosition - originPosition).y - 0.5f ) / (cellSize * ((float)Math.Sqrt(3) / 2)));
        }
        List<Vector3Int> neighborNodes = GetNeighborNodesLimited(roughX, roughY);

        float leastDistance  = Vector3.Distance(worldPosition, GetWorldPosition(roughX, roughY));
        x = roughX;
        y = roughY;
        for(int i =  0; i < neighborNodes.Count; i++)
        {
            float distance = Vector3.Distance(worldPosition, GetWorldPosition(neighborNodes[i].x, neighborNodes[i].y));
            if(distance < leastDistance)
            {
                leastDistance = distance;
                x = neighborNodes[i].x;
                y = neighborNodes[i].y;
            }
        }
    }

    public List<Vector3Int> GetNeighborNodes(int x, int y)
    {
        List<Vector3Int> neighborNodes = new List<Vector3Int>();
        bool oddRow = x % 2 == 1;
        Vector3Int currentPosition = new Vector3Int(x, y, 0);
        neighborNodes.Add(currentPosition + new Vector3Int(0, 1, 0));
        neighborNodes.Add(currentPosition + new Vector3Int(0, -1, 0));

        neighborNodes.Add(currentPosition + new Vector3Int(-1, oddRow ? 1 : -1, 0));
        neighborNodes.Add(currentPosition + new Vector3Int(-1, 0, 0));
        neighborNodes.Add(currentPosition + new Vector3Int(1, oddRow? 1 : -1, 0));
        neighborNodes.Add(currentPosition + new Vector3Int(1, 0, 0));
        return neighborNodes;
    }

    public List<Vector3Int> GetNeighborNodesLimited(int x, int y)
    {
        List<Vector3Int> neighborNodes = new List<Vector3Int>();
        bool oddRow = x % 2 == 1;
        Vector3Int currentPosition = new Vector3Int(x, y, 0);
        neighborNodes.Add(currentPosition + new Vector3Int(0, 1, 0));
        neighborNodes.Add(currentPosition + new Vector3Int(0, -1, 0));

        neighborNodes.Add(currentPosition + new Vector3Int(-1, oddRow ? 1 : -1, 0));
        neighborNodes.Add(currentPosition + new Vector3Int(-1, 0, 0));
        neighborNodes.Add(currentPosition + new Vector3Int(1, oddRow ? 1 : -1, 0));
        neighborNodes.Add(currentPosition + new Vector3Int(1, 0, 0));
        return neighborNodes;
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, value);
    }

    public void SetGridObject(Vector2Int targetHex, TGridObject value)
    {
        SetGridObject(targetHex.x, targetHex.y, value);
    }

    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }
    }

    public TGridObject GetGridObject(Vector2Int targetHex)
    {
        return GetGridObject(targetHex.x, targetHex.y);
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }

    // Conversions To Cube Coordinates

    public Vector3Int OffsetToCube(int x, int y)
    {
        int q = x;
        int r = y - (x + (x % 1)) / 2;
        return new Vector3Int(q, r, -q-r);
    }

    public Vector2Int CubeToOffset(Vector3Int cube)
    {
        int x = cube.x;
        int y = cube.y + (cube.x + (cube.y % 1)) / 2;
        return new Vector2Int(x, y);
    }

    public Vector3Int CubeDirection(int direction)
    {
        return cubeDirectionVectors[direction];
    }

    public Vector3Int CubeAdd(Vector3Int cube, Vector3Int vector)
    {
        return cube + vector;
    }
    
    public Vector3Int CubeNeighbor(Vector3Int cube, int direction)
    {
        return CubeAdd(cube, cubeDirectionVectors[direction]);
    }

    // Get Nodes in a Spiral

    public Vector3Int CubeScale(Vector3Int cube, int factor)
    {
        return cube * factor;
    }
    public List<TGridObject> GetGridObjectsInRing(int x, int y, int radius)
    {
        Vector3Int centerCube = OffsetToCube(x, y);
        return GetGridObjectsInRing(centerCube, radius);
    }

    public List<TGridObject> GetGridObjectsInRing(Vector3Int centerCubePosition, int radius)
    {
        Vector3Int centerCube = centerCubePosition;
        List<Vector3Int> results = new List<Vector3Int>();
        Vector3Int hex = CubeAdd(centerCube, CubeScale(CubeDirection(4), radius));
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < radius; j++)
            {
                results.Add(hex);
                hex = CubeNeighbor(hex, i);
            }
        }

        List<TGridObject> hexes = new List<TGridObject>();
        for (int i = 0; i < results.Count; i++)
        {
            Vector2Int offsetPosition = CubeToOffset(results[i]);
            TGridObject hexObject = GetGridObject(offsetPosition.x, offsetPosition.y);
            if (hexObject != null)
            {
                hexes.Add(hexObject);
            }
        }
        return hexes;
    }


    public Vector3Int CubeSubtract(Vector3Int cube1, Vector3Int cube2)
    {
        return cube1 - cube2;
    }

    public int CubeDistance(Vector3Int cube1, Vector3Int cube2)
    {
        Vector3Int vector = CubeSubtract(cube1, cube2);
        return (Math.Abs(vector.x) + Math.Abs(vector.y) + Math.Abs(vector.z)) / 2;
    }

    public Vector3Int CubeRound(Vector3 cube)
    {
        int q = (int) Math.Round(cube.x);
        int r = (int)Math.Round(cube.y);
        int s = (int)Math.Round(cube.z);

        float qDiff = Math.Abs(q - cube.x);
        float rDiff = Math.Abs(r - cube.y);
        float sDiff = Math.Abs(s - cube.z);

        if(qDiff > rDiff && qDiff > sDiff)
        {
            q = -r - s;
        }
        else if (rDiff > sDiff)
        {
            r = -q - s;
        }
        else
        {
            s = -q - r;
        }
        return new Vector3Int(q, r, s);
    }

    public float CubeLerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    public Vector3 CubeHexLerp(Vector3Int a, Vector3Int b, float t)
    {
        return new Vector3(CubeLerp(a.x, b.x, t),
            CubeLerp(a.y, b.y, t),
            CubeLerp(a.z, b.z, t));
             
    }
    

    public List<Vector3Int> CubeLineDraw(Vector3Int a, Vector3Int b)
    {
        int hexDistance = CubeDistance(a, b);
        List<Vector3Int> hexes = new List<Vector3Int>();
        for(int i = 0; i < hexDistance; i++)
        {
            hexes.Add(CubeRound(CubeHexLerp(a, b, 1.0f / hexDistance * i)));
        }
        hexes.Add(b);
        return hexes;
    }
}