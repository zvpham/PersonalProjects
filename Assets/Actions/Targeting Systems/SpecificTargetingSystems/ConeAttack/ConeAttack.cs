using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeAttack : MonoBehaviour
{
    public float offSet;
    private float xOffSet;
    private float yOffSet;

    public int range;
    public float angle;
    public int startingX;
    public int startingY;

    public Vector3 startPositionTotal;
    public Vector3 gridOriginPosition;

    public Vector3 prevMousePosition;
    public Vector3 mousePosition;

    Vector3 dirTowardsMouse;

    public GameObject coneMarkerPrefab;
    public List<GameObject> markerList;

    public event Action<List<Vector3>, Vector3> foundTarget;

    public void setParameters(Vector3 startPosition, int range, float angle)
    {
        this.startPositionTotal = startPosition;
        this.gridOriginPosition = startPosition + new Vector3(-range, -range, 0);

        this.startingX = (int)startPosition.x;
        this.startingY = (int)startPosition.y;
        this.range = range + 1;
        this.angle = angle;

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

        if (new Vector3((int)(mousePosition.x + xOffSet), (int)(mousePosition.y + yOffSet), 0) != prevMousePosition)
        {
            prevMousePosition = new Vector3((int)(mousePosition.x + xOffSet), (int)(mousePosition.y + yOffSet), 0);
            ClearMarkers();
            dirTowardsMouse = (prevMousePosition - startPositionTotal).normalized;

            for(int i = 0; i < range * 2; i++)
            {
                for (int j = 0; j < range * 2; j++)
                {
                    Vector3 dirTowardsOtherObject = (this.gridOriginPosition + new Vector3(j, i, 0) - startPositionTotal).normalized;
                    float dotProduct = Vector3.Dot(dirTowardsOtherObject, dirTowardsMouse);
                    if (dotProduct > this.angle && Vector3.Distance(startPositionTotal, this.gridOriginPosition + new Vector3(j, i, 0)) < range) // our threx`shold is 0.1
                    {
                        markerList.Add(Instantiate(coneMarkerPrefab, this.gridOriginPosition + new Vector3(j, i, 0), new Quaternion(0, 0, 0, 1f)));
                    }
                }
            }

            if (Input.GetMouseButton(0))
            {
                List<Vector3> markerLocationList = new List<Vector3>();

                foreach (GameObject marker in markerList)
                {
                    markerLocationList.Add(marker.transform.position);
                }

                foundTarget?.Invoke(markerLocationList, dirTowardsMouse);
            }

        }
        
        if (Input.GetMouseButton(0))
        {
            List<Vector3> markerLocationList = new List<Vector3>();

            foreach(GameObject marker in markerList)
            {
                markerLocationList.Add(marker.transform.position);
            }

            foundTarget?.Invoke(markerLocationList, dirTowardsMouse);
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
        }
        markerList.Clear();
    }
    public void DestroySelf()
    {
        if (markerList != null)
        {
            foreach (GameObject marker in markerList)
            {
                Destroy(marker);
            }
            markerList.Clear();
        }
        Destroy(this.gameObject);
    }
}
