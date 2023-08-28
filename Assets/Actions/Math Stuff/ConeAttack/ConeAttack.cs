using CodeMonkey.Utils;
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

    public Grid<ConeAttackMarker> coneGrid;

    public GameObject coneMarkerPrefab;
    public List<GameObject> markerList;

    public int DebugTimes = 81;
    public int currentDebugTImes = 0;


    public void setParameters(Vector3 startPosition, int range, float angle)
    {
        this.startPositionTotal = startPosition;
        this.gridOriginPosition = startPosition + new Vector3(-range, -range, 0);

        this.startingX = (int)startPosition.x;
        this.startingY = (int)startPosition.y;
        this.range = range + 1;
        this.angle = angle;

        coneGrid = new Grid<ConeAttackMarker>(range * 2 + 1, range * 2 + 1, 1f, this.gridOriginPosition, (Grid<ConeAttackMarker> g, int x, int y) => new ConeAttackMarker(g, x, y));

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
            Debug.Log("MOUSE MOVING");
            prevMousePosition = new Vector3((int)(mousePosition.x + xOffSet), (int)(mousePosition.y + yOffSet), 0);
            ClearMarkers();
            currentDebugTImes = 0;
            Vector3 dirTowardsMouse = (prevMousePosition - startPositionTotal).normalized;
            for (int i = 0; i < coneGrid.GetHeight(); i++)
            {
                for (int j = 0; j < coneGrid.GetWidth(); j++)
                {
                    Vector3 dirTowardsOtherObject = (coneGrid.GetWorldPosition(j, i) - startPositionTotal).normalized;
                    float dotProduct = Vector3.Dot(dirTowardsOtherObject, dirTowardsMouse);
                    if (dotProduct > this.angle && Vector3.Distance(startPositionTotal, this.gridOriginPosition + new Vector3(j, i, 0)) <= range) // our threx`shold is 0.1
                    {
                         markerList.Add(Instantiate(coneMarkerPrefab, this.gridOriginPosition + new Vector3(j, i, 0), new Quaternion(0, 0, 0, 1f)));
                    }
                }
                if (currentDebugTImes >= DebugTimes)
                {
                    break;
                }
            }
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
}
