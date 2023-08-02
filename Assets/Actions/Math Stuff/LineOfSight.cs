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
    public GameObject linePrefab;
    public GameObject collisionPrefab;
    private GameObject finalMarker;
    public float offSet;
    private float xOffSet;
    private float yOffSet;

    public bool careAboutObstacles;
    public bool hitObstacle;
    private bool isreturn = false;
    private bool isPlayer = false;

    public int range;

    private Vector3 position = new Vector3();
    private Quaternion rotation = new Quaternion(0, 0, 0, 1f);

    private GameManager gameManager;

    public List<Vector3> path  = new List<Vector3>();

    public event Action< List<Vector3> > lineMade;

    private void Start()
    {
        gameManager = GameManager.instance;
    }
        
    public void setParameters(Vector3 startPosition, bool isPlayerCharacter)
    {
        startingX = (int)startPosition.x;
        startingY = (int)startPosition.y;
        isPlayer = isPlayerCharacter;
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
        if(isreturn)
        {
            lineMade?.Invoke(path);
        }
    }

    public bool createDot(int x, int y, int numberMarkers)
    {
        if(numberMarkers < range)
        {
            position.Set((float)x, (float)y, 0);
            finalMarker = Instantiate(linePrefab, position, rotation);

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
}


