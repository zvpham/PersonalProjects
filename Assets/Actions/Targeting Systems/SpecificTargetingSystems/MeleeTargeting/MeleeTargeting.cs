using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using UnityEngine.UIElements;
using Bresenhams;

public class MeleeTargeting : MonoBehaviour
{
    public int startingX;
    public int startingY;
    public Vector3 startPosition;
    public Vector3 endPosition;
    public AllDirections allDirections;
    public InputManager inputManager;
    public GameManager gameManager;

    public float offSet;
    private float xOffSet;
    private float yOffSet;

    private bool isreturn = false;

    private Vector3 position = new Vector3();
    private Quaternion rotation = new Quaternion(0, 0, 0, 1f);

    public GameObject endPositionMarkerPrefab;
    public GameObject endPositionMarker;
    public GameObject linePrefab;
    private GameObject finalMarker;
    private List<GameObject> markerList = new List<GameObject>();

    public bool careAboutRange;
    public List<GameObject> rangeMarkerList;
    public GameObject rangeMarker;

    public event Action<Vector3> foundTarget;
    public event Action<Vector3> hitWall;
    public event Action<bool> Canceled;

    public Vector3 prevMousePosition;
    public Vector3 mousePosition;

    public Vector3 targetPosition;
    public void Start()
    {
        gameManager = GameManager.instance;
        inputManager = InputManager.instance;
    }

    public void setParameters(Vector3 startPosition)
    {
        this.startPosition = startPosition;
        startingX = (int)startPosition.x;
        startingY = (int)startPosition.y;
    }

    public void Update()
    {
        mousePosition = UtilsClass.GetMouseWorldPosition();
        xOffSet = 0;
        yOffSet = 0;
        if ((startingX - mousePosition.x) % 1 <= .5 && (startingX - mousePosition.x) % 1 >= 0)
        {
            xOffSet = offSet;
        }
        else
        {
            xOffSet = offSet;
        }

        if ((startingY - mousePosition.y) % 1 <= .5 && (startingY - mousePosition.y) % 1 >= 0)
        {
            yOffSet = offSet;
        }
        else
        {
            yOffSet = offSet;
        }

        if (mousePosition != prevMousePosition && new Vector3((int)(mousePosition.x + xOffSet), (int)(mousePosition.y + yOffSet), 0) != targetPosition)
        {
            targetPosition = new Vector3((int)(mousePosition.x + xOffSet), (int)(mousePosition.y + yOffSet), 0);
            ClearMarkers();
            if (Input.GetMouseButtonDown(0))
            {
                isreturn = true;
            }

            MakeLine();
        }
        else
        {
            for (int i = 0; i < allDirections.Directions.Length; i++)
            {
                if (inputManager.GetKeyDownTargeting(allDirections.Directions[i].directionName))
                {
                    Debug.Log("Pressed Button");
                    endPosition = startPosition + allDirections.Directions[i].GetDirection();
                    Unit unit = gameManager.grid.GetGridObject(endPosition);
                    if (unit != null)
                    {
                        foundTarget?.Invoke(endPosition);
                    }

                    Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(endPosition);
                    if (gameManager.obstacleGrid.GetGridObject(endPosition) != null)
                    {
                        hitWall?.Invoke(gridPosition);
                    }

                    Canceled?.Invoke(false);
                }
            }

            ClearMarkers();
            if (Input.GetMouseButtonDown(0))
            {
                isreturn = true;
            }

            MakeLine();
        }
    }

    public void MakeLine()
    {
        BresenhamsAlgorithm.PlotFunction plotFunction = createDot;
        BresenhamsAlgorithm.Line(startingX, startingY, (int)targetPosition.x, (int)targetPosition.y, plotFunction);
        if(markerList.Count == 2)
        {
            Destroy(markerList[0]);
            markerList[0] = null;
            endPosition = markerList[1].gameObject.transform.position;
        }

        if (isreturn)
        {
            ClearMarkers();

            Unit unit = gameManager.grid.GetGridObject(endPosition);
            if (unit != null)
            {
                foundTarget?.Invoke(endPosition);
            }

            Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(endPosition);
            if (gameManager.obstacleGrid.GetGridObject(endPosition) != null)
            {
                hitWall?.Invoke(gridPosition);
            }

            Canceled?.Invoke(false);
        }
    }

    public void ClearMarkers()
    {
        if (markerList != null || markerList.Count > 0)
        {
            foreach (GameObject marker in markerList)
            {
                Destroy(marker);
            }
            Destroy(endPositionMarker);
        }
    }

    public bool createDot(int x, int y, int numberMarkers)
    {
        if (numberMarkers < 2)
        {
            position.Set((float)x, (float)y, 0);
            finalMarker = Instantiate(linePrefab, position, rotation);

            if (numberMarkers == 0)
            {
                markerList.Clear();
            }

            markerList.Add(finalMarker);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void DestroySelf()
    {
        if (markerList != null)
        {
            foreach (GameObject marker in rangeMarkerList)
            {
                Destroy(marker);
            }
            rangeMarkerList.Clear();
        }
        Destroy(this.gameObject);
    }
}
