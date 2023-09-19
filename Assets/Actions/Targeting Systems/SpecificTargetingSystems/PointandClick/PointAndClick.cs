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

    public Vector3 prevMousePosition;
    public Vector3 mousePosition;

    public GameObject endPointMarkerPrefab;
    private GameObject endPointMarker;

    public event Action<Vector3> endPointFound;

    public void setParameters(Vector3 startPosition)
    {
        startingX = (int)startPosition.x;
        startingY = (int)startPosition.y;
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

        if (new Vector3((int)(mousePosition.x + xOffSet), (int)(mousePosition.y + yOffSet), 0) != prevMousePosition)
        {
            prevMousePosition = new Vector3((int)(mousePosition.x + xOffSet), (int)(mousePosition.y + yOffSet), 0);
            ClearMarkers();
            endPointMarker = Instantiate(endPointMarkerPrefab, new Vector3((int)(mousePosition.x + xOffSet), (int)(mousePosition.y + yOffSet), 0), new Quaternion(0, 0, 0, 1f));
        }

        if (Input.GetMouseButtonDown(0))
        {
            endPointFound?.Invoke(endPointMarker.transform.position);
        }

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
