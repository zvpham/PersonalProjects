
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bresenhams;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using CodeMonkey.Utils;
using UnityEngine.Rendering.Universal;
using System;
using UnityEngine.UIElements;

public static class LineOfSightAI
{
    public static bool careAboutRange;  
    public static int range;

    public static List<Vector3> path = new List<Vector3>();
    public static Vector3 position;


    public static List<Vector3> MakeLine(Vector3 startPosition, Vector3 endPosition, int projectileRange = 0, bool careAboutRangeGiven = true)
    {
        path = new List<Vector3>();
        range = projectileRange;
        careAboutRange = careAboutRangeGiven;
        BresenhamsAlgorithm.PlotFunction plotFunction = CheckSpaceAI;
        BresenhamsAlgorithm.Line((int)startPosition.x, (int)startPosition.y, (int)endPosition.x, (int)endPosition.y, plotFunction);

        return path;
    }

    public static bool CheckSpaceAI(int x, int y, int numberMarkers)
    {
        if (careAboutRange)
        {
            position.Set((float)x, (float)y, 0);
            path.Add(position);
            return true;
        }
        else
        {
            if (numberMarkers < range)
            {
                position.Set((float)x, (float)y, 0);
                path.Add(position);
                return true;
            }
            else
            {
                path = null;
                return false;
            }
        }
    }
}

