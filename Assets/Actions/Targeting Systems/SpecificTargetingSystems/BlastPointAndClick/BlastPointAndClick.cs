using Bresenhams;
using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastPointAndClick : MonoBehaviour
{
    public Vector3 startPositionTotal;
    public int startingX;
    public int startingY;
    public int endX;
    public int endY;

    public int blastRadius;
    //public Grid<BlastRadiusMarker> blastRadiusGrid;
    public List<GameObject> blastMarkerList;
    public GameObject blastRadiusMarker;
    public GameObject endPositionMarker;

    public float offSet;
    private float xOffSet;
    private float yOffSet;

    public bool careAboutRange;
    public int range;
    public List<GameObject> rangeMarkerList;
    public GameObject rangeMarker;

    public List<Vector3> path = new List<Vector3>();

    public AllDirections allDirections;

    public event Action<Vector3> endPointFound;

    public Vector3 prevMousePosition;
    public Vector3 mousePosition;
    public Vector3 targetPosition;

    public InputManager inputManager;

    public void Start()
    {
        inputManager = InputManager.instance;
    }

    public void setParameters(Vector3 startPosition, int blaseRadiusGiven, int projectileRange = 0, Sprite projectilBeingLaunched = null, int projectileSpeedWhenFired = int.MaxValue, int numSections = 1, bool careAboutRangeGiven = true, bool careAboutObstaclesGiven = false)
    {
        startPositionTotal = startPosition;
        startingX = (int)startPosition.x;
        startingY = (int)startPosition.y;
        range = projectileRange + 1;
        blastRadius = blaseRadiusGiven;
        careAboutRange = careAboutRangeGiven;
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
            MakeBlast();
        }
        else
        {
            for (int i = 0; i < allDirections.Directions.Length; i++)
            {
                if (inputManager.GetKeyDownTargeting(allDirections.Directions[i].directionName))
                {
                    targetPosition = targetPosition + allDirections.Directions[i].GetDirection();
                    MakeBlast();
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                endPointFound?.Invoke(targetPosition);
            }
        }
        prevMousePosition = mousePosition;
    }

    public void MakeBlast()
    {
        ClearMarkers();

        for (int i = 0; i < blastRadius * 2 + 1; i++)
        {
            for (int j = 0; j < blastRadius * 2 + 1; j++)
            {
                if (Vector3.Distance(targetPosition, targetPosition + new Vector3(-blastRadius, -blastRadius, 0) + new Vector3(j, i, 0)) <= blastRadius)
                {
                    blastMarkerList.Add(Instantiate(blastRadiusMarker, targetPosition + new Vector3(-blastRadius, -blastRadius, 0) + new Vector3(j, i, 0), new Quaternion(0, 0, 0, 1f)));
                }
            }
        }
        blastMarkerList.Add(Instantiate(endPositionMarker, targetPosition, new Quaternion(0, 0, 0, 1f)));

        if (Input.GetMouseButtonDown(0))
        {
            endPointFound?.Invoke(targetPosition);
        }
    }

    public void ClearMarkers()
    {
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

    public void DestroySelf()
    {
        if (rangeMarkerList != null)
        {
            foreach (GameObject marker in rangeMarkerList)
            {
                Destroy(marker);
            }
            rangeMarkerList.Clear();
        }

        if (blastMarkerList != null)
        {
            foreach(GameObject marker in blastMarkerList)
            {
                Destroy(marker);
            }
            blastMarkerList.Clear();
        }
        Destroy(this.gameObject);
    }
}
