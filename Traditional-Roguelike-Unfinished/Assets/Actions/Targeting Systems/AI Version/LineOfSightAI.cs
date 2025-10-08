
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

    public static List<Vector2> path = new List<Vector2>();
    public static Vector2 position;


    public static List<Vector2> MakeLine(Vector2 startPosition, Vector2 endPosition, int projectileRange = 0, bool careAboutRangeGiven = true)
    {
        path = new List<Vector2>();
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
            position.Set((float)x, (float)y);
            path.Add(position);
            return true;
        }
        else
        {
            if (numberMarkers < range)
            {
                position.Set((float)x, (float)y);
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

