using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bresenhams;
using Unity.VisualScripting;

public class LineOfSight : MonoBehaviour
{
    public int startingX;
    public int startingY;
    public GameObject linePrefab;
    


    void Update()
    {
        Vector3 targetPosition =  Input.mousePosition;
        BresenhamsAlgorithm.PlotFunction plotFunction = createDot;
        BresenhamsAlgorithm.Line(startingX, startingY, (int)targetPosition.x, (int)targetPosition.y, plotFunction);
    }

    public bool createDot(int x, int y)
    {
        Vector3 position = new Vector3();
        Quaternion rotation = new Quaternion(0,0, 0, 1f);
        position.Set((float) x, (float) y, 0);
        Instantiate(linePrefab, position, rotation);
        return true;
    }
}


