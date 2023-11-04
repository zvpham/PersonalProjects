using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Obstacles : MonoBehaviour
{
    public Tilemap collisionTilemap;
    public GameManager gameManager;
    
    public void Start()
    {
        gameManager = GameManager.instance;
        gameManager.collisionTilemap = collisionTilemap;
    }
}

