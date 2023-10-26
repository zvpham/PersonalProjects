using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;
    
    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Found more than MapGenerator in the Scence");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void WaveFunctionCollapse()
    {
       
    }

    public void CreateNoiseMap()
    {

    }

    public void RandomizeNoseMap()
    {

    }
}
