using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "NumberRanges")]
public class NumberRanges : ScriptableObject
{
    public int minNumber;
    public int maxNumber;

    public int GetRandomNumberInRange(int seed)
    {
        Random.InitState(seed);
        return Random.Range(minNumber, maxNumber + 1);
    }
}
