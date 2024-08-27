using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainHolder : MonoBehaviour
{
    public int elevation;
    public SpriteRenderer sprite;
    public int x, y;
    public List<TerrainHolder> walls =  new List<TerrainHolder>();
    public bool isGround;
    public SpriteManager spriteManager;
    public void Awake()
    {
        if (isGround)
        {
            if (spriteManager.tempTerrain == null)
            {
                spriteManager.tempTerrain = new TerrainHolder[32, 32];
            }

            spriteManager.tempTerrain[x,y] = this;
        }
    }
}
