using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bresenhams;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using CodeMonkey.Utils;
using UnityEngine.Rendering.Universal;
using System;

public class LineOfSightAI : MonoBehaviour
{
    public int startingX;
    public int startingY;
    public int endX;
    public int endY;

    public int projectileSpeed;
    public int projectileRangeSections;

    public GameObject linePrefab;
    private List<GameObject> markerList = new List<GameObject>();
    private GameObject finalMarker;

    public bool careAboutObstacles;
    public bool hitObstacle;
    public int numhitObstacle = -1;

    private bool isreturn = false;

    public int range;

    private Vector3 position = new Vector3();
    private Quaternion rotation = new Quaternion(0, 0, 0, 1f);

    private GameManager gameManager;

    public List<Vector3> path = new List<Vector3>();

    public event Action<List<Vector3>> lineMade;

    private void Start()
    {
        gameManager = GameManager.instance;
    }

    public void setParameters(Vector3 startPosition, int projectileRange, int projectileSpeedWhenFired = int.MaxValue, int numSections = 1)
    {
        startingX = (int)startPosition.x;
        startingY = (int)startPosition.y;
        range = projectileRange;
        projectileSpeed = projectileSpeedWhenFired;
        projectileRangeSections = numSections;
    }



    void Update()
    {
        BresenhamsAlgorithm.PlotFunction plotFunction = GetPath;
        BresenhamsAlgorithm.Line(startingX, startingY, endX, endY, plotFunction);

        if (projectileRangeSections != 1)
        {
            if (markerList.Count != 2)
            {
                projectileSpeed = (markerList.Count / projectileRangeSections) + 1;
            }
            else
            {
                projectileSpeed = markerList.Count / projectileRangeSections;   
            }
        }

        if (numhitObstacle == -1 || !(projectileSpeed - 1 >= numhitObstacle))
        {

        }
        if (isreturn)
        {
            lineMade?.Invoke(path);
        }
    }

    public bool GetPath(int x, int y, int numberMarkers)
    {
        if (numberMarkers < range)
        {
            position.Set((float)x, (float)y, 0);
            finalMarker = Instantiate(linePrefab, position, rotation);

            if (numberMarkers == 0)
            {
                markerList.Clear();
            }

            markerList.Add(finalMarker);

            if (isreturn)
            {
                path.Add(position);
            }
            if (careAboutObstacles)
            {
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
                        finalMarker.GetComponent<SpriteRenderer>().color = Color.red;
                    }
                }
            }

            return true;
        }
        else
        {
            return false;
        }
    }
}


