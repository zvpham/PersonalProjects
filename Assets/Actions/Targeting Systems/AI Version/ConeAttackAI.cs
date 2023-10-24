using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConeAttackAI
{
    public static Tuple<List<Vector3>, Vector3> GetCone(Vector3 startPosition, Vector3 endPosition, int range, float angle)
    {
        List<Vector3> possiblePosition = new List<Vector3>();
        Vector3 gridOriginPosition = startPosition + new Vector3(-range, -range, 0);

        Vector3 dirTowardsTarget = (endPosition - startPosition).normalized;

        for (int i = 0; i < range * 2; i++)
        {
            for (int j = 0; j < range * 2; j++)
            {
                Vector3 dirTowardsOtherObject = (gridOriginPosition + new Vector3(j, i, 0) - startPosition).normalized;
                float dotProduct = Vector3.Dot(dirTowardsOtherObject, dirTowardsTarget);
                if (dotProduct > angle && Vector3.Distance(startPosition, gridOriginPosition + new Vector3(j, i, 0)) < range + 1) // our threx`shold is 0.1
                {
                    possiblePosition.Add(gridOriginPosition + new Vector3(j, i, 0));
                }
            }
        }

        return  new Tuple<List<Vector3>, Vector3>  (possiblePosition, dirTowardsTarget);
    }       
}
