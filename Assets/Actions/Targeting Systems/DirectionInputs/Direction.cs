using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Direction")]
public  class Direction : ScriptableObject
{
    public DirectionName directionName;
    public Vector3 directionValue;
     public Vector3 GetDirection()
    {
        return directionValue;
    }

}
