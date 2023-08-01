using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bresenhams;
using Unity.VisualScripting;
using CodeMonkey.Utils;
using UnityEngine.Rendering.Universal;

public class LineOfSight : MonoBehaviour
{
    public int startingX;
    public int startingY;
    public int endX;
    public int endY;
    public GameObject linePrefab;
    public GameObject finalMarker;
    public float offSet;
    private float xOffSet;
    private float yOffSet;

    public bool careAboutObstacles;
    public bool hitObstacle;

    public int range;

    private Vector3 position = new Vector3();
    private Quaternion rotation = new Quaternion(0, 0, 0, 1f);


    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.instance;
    }
    void Update()
    {
        xOffSet = offSet;
        yOffSet = offSet;
        Vector3 mousePosition = UtilsClass.GetMouseWorldPosition();
        if(startingX - mousePosition.x > 0)
        {
            xOffSet *= -1;
        }
        if (startingY - mousePosition.y > 0)
        {
            yOffSet *= -1;
        }   
        BresenhamsAlgorithm.PlotFunction plotFunction = createDot;
        BresenhamsAlgorithm.Line(startingX, startingY, (int)(mousePosition.x + xOffSet), (int)(mousePosition.y + yOffSet), plotFunction);

    }

    public bool createDot(int x, int y, int numberMarkers)
    {
        if(numberMarkers < range)
        {
            position.Set((float)x, (float)y, 0);
            finalMarker = Instantiate(linePrefab, position, rotation);

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


