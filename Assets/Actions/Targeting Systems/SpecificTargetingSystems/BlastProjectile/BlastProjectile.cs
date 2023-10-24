using Bresenhams;
using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BlastProjectile: MonoBehaviour
{
    public Vector3 startPositionTotal;
    public int startingX;
    public int startingY;
    public int endX;
    public int endY;

    public int projectileSpeed;
    public int projectileRangeSections;
    public Sprite projectile;

    public GameObject endPositionMarker;
    public GameObject linePrefab;
    private GameObject finalMarker;
    private List<GameObject> markerList = new List<GameObject>();

    public int blastRadius;
    //public Grid<BlastRadiusMarker> blastRadiusGrid;
    public List<GameObject> blastMarkerList;
    public GameObject blastRadiusMarker;

    public Sprite collisionMarkerSprite;
    public bool careAboutObstacles;
    public bool hitObstacle;
    public int numhitObstacle = -1;

    public float offSet;
    private float xOffSet;
    private float yOffSet;

    private bool isreturn = false;

    public bool careAboutRange;
    public int range;
    public List<GameObject> rangeMarkerList;
    public GameObject rangeMarker;

    private Vector3 position = new Vector3();
    private Quaternion rotation = new Quaternion(0, 0, 0, 1f);

    public List<Vector3> path = new List<Vector3>();

    public AllDirections allDirections;

    public event Action<List<Vector3>> lineMade;

    public Vector3 prevMousePosition;
    public Vector3 mousePosition;

    public Vector3 targetPosition;

    public GameManager gameManager;
    public InputManager inputManager;

    public void Start()
    {
        gameManager = GameManager.instance;
        inputManager = InputManager.instance;
    }

    public void setParameters(Vector3 startPosition, int blaseRadiusGiven, int projectileRange = 0, Sprite projectilBeingLaunched = null, int projectileSpeedWhenFired = int.MaxValue, int numSections = 1, bool careAboutRangeGiven = true, bool careAboutObstaclesGiven = false)
    {
        startPositionTotal = startPosition;
        startingX = (int)startPosition.x;
        startingY = (int)startPosition.y;
        range = projectileRange + 1;
        projectile = projectilBeingLaunched;
        projectileSpeed = projectileSpeedWhenFired;
        projectileRangeSections = numSections;;
        blastRadius = blaseRadiusGiven;
        careAboutRange = careAboutRangeGiven;
        careAboutObstacles = careAboutObstaclesGiven;

        if (range != 1)
        {
            for (int i = 0; i < range * 2 - 1; i++)
            {
                for (int j = 0; j < range * 2 - 1; j++)
                {
                    rangeMarkerList.Add(Instantiate(rangeMarker, prevMousePosition + new Vector3(-range, -range, 0) + new Vector3(j, i, 0), new Quaternion(0, 0, 0, 1f)));
                }
            }
        }

        targetPosition = startPosition;
        prevMousePosition = UtilsClass.GetMouseWorldPosition();
    }



    void Update()
    {
        mousePosition = UtilsClass.GetMouseWorldPosition();
        xOffSet = 0;
        yOffSet = 0;
        if ((startingX - mousePosition.x) % 1 <= .5 && (startingX - mousePosition.x) % 1 >= 0)
        {
            xOffSet = offSet;
        }
        else if ((startingX - mousePosition.x) % 1 <= -.5)
        {
            xOffSet = offSet;
        }

        if ((startingY - mousePosition.y) % 1 <= .5 && (startingY - mousePosition.y) % 1 >= 0)
        {
            yOffSet = offSet;
        }
        else if ((startingY - mousePosition.y) % 1 <= -.5)
        {
            yOffSet = offSet;
        }

        if (mousePosition != prevMousePosition && new Vector3((int)(mousePosition.x + xOffSet), (int)(mousePosition.y + yOffSet), 0) != targetPosition)
        {
            targetPosition = new Vector3((int)(mousePosition.x + xOffSet), (int)(mousePosition.y + yOffSet), 0);
            MakeBlastProjectile();
        }
        else
        {
            for (int i = 0; i < allDirections.Directions.Length; i++)
            {
                if (inputManager.GetKeyDownTargeting(allDirections.Directions[i].directionName))
                {
                    targetPosition = targetPosition + allDirections.Directions[i].GetDirection();
                    MakeBlastProjectile();
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                foreach (GameObject marker in markerList)
                {
                    path.Add(marker.transform.position);
                }
                ClearMarkers();
                lineMade?.Invoke(path);
            }
        }
        prevMousePosition = mousePosition;
    }

    public void MakeBlastProjectile()
    {
        ClearMarkers();

        if (Input.GetMouseButtonDown(0))
        {
            isreturn = true;
        }
        markerList.Add(Instantiate(endPositionMarker, targetPosition, new Quaternion(0, 0, 0, 1f)));
        BresenhamsAlgorithm.PlotFunction plotFunction = createDot;
        BresenhamsAlgorithm.Line(startingX, startingY, (int)targetPosition.x, (int)targetPosition.y, plotFunction);

        if (projectileRangeSections != 1)
        {
            if (markerList.Count % 2 == 1 && markerList.Count >= 3)
            {
                projectileSpeed = (markerList.Count / projectileRangeSections) + 1;
            }
            else
            {
                projectileSpeed = markerList.Count / projectileRangeSections;
            }
        }

        if (careAboutObstacles && projectile != null && (markerList.Count >= 2 && (numhitObstacle == -1 || !(projectileSpeed - 1 >= numhitObstacle))))
        {
            markerList[projectileSpeed - 1].GetComponent<SpriteRenderer>().sprite = projectile;

            Color tmp = markerList[projectileSpeed - 1].GetComponent<SpriteRenderer>().color;
            tmp.a = 0.5f;
            markerList[projectileSpeed - 1].GetComponent<SpriteRenderer>().color = tmp;

        }
        if (numhitObstacle == -1)
        {
            numhitObstacle = markerList.Count - 1;
        }

        for (int i = 0; i < blastRadius * 2 + 1; i++)
        {
            for (int j = 0; j < blastRadius * 2 + 1; j++)
            {
                if (Vector3.Distance(markerList[numhitObstacle].transform.position, markerList[numhitObstacle].transform.position + new Vector3(-blastRadius, -blastRadius, 0) + new Vector3(j, i, 0)) <= blastRadius)
                {
                    blastMarkerList.Add(Instantiate(blastRadiusMarker, markerList[numhitObstacle].transform.position + new Vector3(-blastRadius, -blastRadius, 0) + new Vector3(j, i, 0), new Quaternion(0, 0, 0, 1f)));
                }
            }
        }

        if (isreturn)
        {
            foreach (GameObject marker in markerList)
            {
                path.Add(marker.transform.position);
            }
            ClearMarkers();
            lineMade?.Invoke(path);
        }
    }

    public void ClearMarkers()
    {

        try
        {
            foreach (GameObject marker in markerList)
            {
                Destroy(marker);
            }
        }
        catch
        {

        }

        try
        {
            foreach (GameObject marker in blastMarkerList)
            {
                Destroy(marker);
            }
            blastMarkerList.Clear();
        }
        catch
        {

        }
    }

    public bool createDot(int x, int y, int numberMarkers)
    {
        if (numberMarkers < range)
        {
            position.Set((float)x, (float)y, 0);
            finalMarker = Instantiate(linePrefab, position, rotation);

            if (numberMarkers == 0)
            {
                markerList.Clear();
            }

            if (careAboutObstacles)
            {
                Debug.Log("Hit Obstacle");
                if (numberMarkers == 0)
                {
                    finalMarker.GetComponent<SpriteRenderer>().color = linePrefab.GetComponent<SpriteRenderer>().color;
                    hitObstacle = false;
                    numhitObstacle = -1;
                }

                if (hitObstacle)
                {
                    finalMarker.GetComponent<SpriteRenderer>().color = Color.red;
                }
                else
                {
                    Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(position);
                    if (gameManager.collisionTilemap.HasTile(gridPosition))
                    {
                        Debug.Log("Hit WAll");
                        hitObstacle = true;
                        numhitObstacle = numberMarkers;
                        finalMarker.GetComponent<SpriteRenderer>().sprite = collisionMarkerSprite;
                        finalMarker.GetComponent<SpriteRenderer>().color = Color.red;
                    }
                }
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
