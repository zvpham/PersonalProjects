using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bresenhams;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using CodeMonkey.Utils;
using UnityEngine.Rendering.Universal;
using System;

public class LineOfSight : MonoBehaviour
{
    public int startingX;
    public int startingY;
    public int endX;
    public int endY;

    public int projectileSpeed;
    public int projectileRangeSections;
    public Sprite projectile;

    public bool CareAboutPath;
    public GameObject linePrefab;
    public GameObject collisionPrefab;
    private GameObject finalMarker;
    private List<GameObject> markerList = new List<GameObject>();


    public float offSet;
    private float xOffSet;
    private float yOffSet;

    public bool careAboutObstacles;
    public bool hitObstacle;
    public int numhitObstacle = -1;

    private bool isreturn = false;

    public bool careAboutRange;
    public int range;
    public Grid<RangeMarker> rangeGrid;
    public GameObject rangeMarker;

    public int blastRadius;
    public Grid<BlastRadiusMarker> blastRadiusGrid;
    public GameObject blastRadiusMarker;

    private Vector3 position = new Vector3();
    private Quaternion rotation = new Quaternion(0, 0, 0, 1f);

    private GameManager gameManager;

    public List<Vector3> path  = new List<Vector3>();

    public event Action< List<Vector3> > lineMade;
    public event Action<Vector3> endPointFound;

    private void Start()
    {
        gameManager = GameManager.instance;
    }
        
    public void setParameters(Vector3 startPosition,  int projectileRange = 0, Sprite projectilBeingLaunched = null, int projectileSpeedWhenFired = int.MaxValue, int numSections = 1, int blaseRadiusGiven = 0, bool careAboutPathGiven = true, bool careAboutRangeGiven = true)
    {
        startingX = (int)startPosition.x;
        startingY = (int)startPosition.y;
        range = projectileRange + 1;
        projectile = projectilBeingLaunched;
        projectileSpeed = projectileSpeedWhenFired; 
        projectileRangeSections = numSections;
        CareAboutPath = careAboutPathGiven;
        blastRadius = blaseRadiusGiven;
        careAboutRange = careAboutRangeGiven;

        if (range ==  1)
        {
            rangeGrid = new Grid<RangeMarker>(range * 2 - 1, range * 2 - 1, 1f, startPosition + new Vector3(-range, -range, 0), (Grid<RangeMarker> g , int x, int y) => new RangeMarker(g,x,y, rangeMarker, startPosition + new Vector3(-range + 1, -range + 1, 0)));
        }
    }
        
    

    void Update()
    {
        xOffSet = 0;
        yOffSet = 0;
        Vector3 mousePosition = UtilsClass.GetMouseWorldPosition();
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

        if (Input.GetMouseButtonDown(0))
        {
            isreturn = true;
        }

        BresenhamsAlgorithm.PlotFunction plotFunction = createDot;
        BresenhamsAlgorithm.Line(startingX, startingY, (int)(mousePosition.x + xOffSet), (int)(mousePosition.y + yOffSet), plotFunction);
        if (careAboutRange)
        {
            if(blastRadius > 0)
            {
                blastRadiusGrid = new Grid<BlastRadiusMarker>(blastRadius * 2 + 1, blastRadius * 2 + 1, 1f, markerList[markerList.Count - 1].transform.position + new Vector3(-blastRadius, -blastRadius, 0), (Grid<BlastRadiusMarker> g, int x, int y) => new BlastRadiusMarker(g, x, y, blastRadiusMarker, markerList[markerList.Count - 1].transform.position + new Vector3(-blastRadius, -blastRadius, 0), blastRadius));
            }
                
            if (CareAboutPath)
            {
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

                if (markerList.Count >= 2 && (numhitObstacle == -1 || !(projectileSpeed - 1 >= numhitObstacle)))
                {
                    markerList[projectileSpeed - 1].GetComponent<SpriteRenderer>().sprite = projectile;

                    Color tmp = markerList[projectileSpeed - 1].GetComponent<SpriteRenderer>().color;
                    tmp.a = 0.5f;
                    markerList[projectileSpeed - 1].GetComponent<SpriteRenderer>().color = tmp;

                }

                if (isreturn)
                {
                    lineMade?.Invoke(path);
                }
            }
            
            if (isreturn)
            {
                endPointFound?.Invoke(markerList[markerList.Count - 1].transform.position);
            }
        }
        else if(isreturn && range == 1)
        {
            endPointFound?.Invoke(new Vector3 ((int)(mousePosition.x + xOffSet), (int)(mousePosition.y + yOffSet), 0));
        }
    }

    public bool createDot(int x, int y, int numberMarkers)
    {
        if(numberMarkers < range)
        {
            position.Set((float)x, (float)y, 0);
            finalMarker = Instantiate(linePrefab, position, rotation);

            if(numberMarkers == 0)
            {
                markerList.Clear();
            }

            markerList.Add(finalMarker);

            if(isreturn)
            {
                path.Add(position);
            }
            if (careAboutObstacles)
            {
                if(numberMarkers == 0)
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
                        GameObject collisionMarker = Instantiate(collisionPrefab, position, rotation);
                        collisionMarker.GetComponent<SpriteRenderer>().color = Color.red;
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

    public void DestroySelf()
    {
        if (rangeGrid != null) 
        {
            for (int i = 0; i < rangeGrid.GetHeight(); i++)
            {
                for (int j = 0; j < rangeGrid.GetWidth(); j++)
                {
                    rangeGrid.GetGridObject(i, j).DestroySelf();
                }
            }
        }
        Destroy(this.gameObject);
    }
}


