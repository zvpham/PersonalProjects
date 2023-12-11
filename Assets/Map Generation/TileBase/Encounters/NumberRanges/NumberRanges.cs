using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "NumberRanges")]
public class NumberRanges : ScriptableObject
{
    public int minNumber;
    public int maxNumber;

    public int GetRandomNumberInRange()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        return Random.Range(minNumber, maxNumber + 1);
    }
}
