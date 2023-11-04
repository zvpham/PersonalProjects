using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameManager gameManager;
    public Grid<NoiseMapObject> noiseMap;

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

    public void Start()
    {
        gameManager = GameManager.instance;
    }

    public void WaveFunctionCollapse()
    {
       
    }

    public void CreateNoiseMap()
    {
        noiseMap = new Grid<NoiseMapObject>(gameManager.mapWidth, gameManager.mapHeight,
            1f, new Vector3(-0.5f, -0.5f, 0f), (Grid<NoiseMapObject> g, int x, int y) =>
            new NoiseMapObject(g, x, y, Mathf.PerlinNoise(x * 1f, y * 1f)));
    }

    public void RandomizeNoseMap()
    {
        if(noiseMap == null)
        {
            Debug.LogError("Yo Something is Wack YO");
        }
        for (int i = 0; i < noiseMap.GetHeight(); i++)
        {
            for (int j = 0; j < noiseMap.GetWidth(); j++)
            {
                noiseMap.GetGridObject(j, i).value = Mathf.PerlinNoise(j * 1f, i * 1f);
            }
        }
    }

    public void GenerateTile(TileBase tileBase)
    {   

    }
}
