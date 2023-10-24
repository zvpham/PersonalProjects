using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAndClick : MonoBehaviour
{
    public int startingX;
    public int startingY;

    public float offSet;
    private float xOffSet;
    private float yOffSet;

    public Vector3 mousePosition;
    public Vector3 prevMousePosition;

    public Vector3 targetPosition;

    public GameObject endPointMarkerPrefab;
    private GameObject endPointMarker;

    public AllDirections allDirections; 
    public InputManager inputManager;

    public event Action<Vector3> endPointFound;

    public void Start()
    {
        inputManager = InputManager.instance;
    }

    public void setParameters(Vector3 startPosition)
    {
        startingX = (int)startPosition.x;
        startingY = (int)startPosition.y;
        targetPosition = startPosition;
        prevMousePosition = UtilsClass.GetMouseWorldPosition();
    }

    // Update is called once per frame
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
            ClearMarkers();
            endPointMarker = Instantiate(endPointMarkerPrefab, targetPosition, new Quaternion(0, 0, 0, 1f));
        }
        else
        {
            for (int i = 0; i < allDirections.Directions.Length; i++)
            {
                if (inputManager.GetKeyDownTargeting(allDirections.Directions[i].directionName))
                {
                    targetPosition = targetPosition + allDirections.Directions[i].GetDirection();
                    ClearMarkers();
                    endPointMarker = Instantiate(endPointMarkerPrefab, targetPosition, new Quaternion(0, 0, 0, 1f));
                }
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            endPointFound?.Invoke(endPointMarker.transform.position);
        }
        prevMousePosition = mousePosition;
    }

    public void ClearMarkers()
    {
        try
        {
            Destroy(endPointMarker);
        }
        catch { }
    }
}
