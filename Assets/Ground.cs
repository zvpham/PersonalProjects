using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ground : MonoBehaviour
{
    public Tilemap groundTilemap;
    public GameManager gameManager;

    public void Start()
    {
        gameManager = GameManager.instance;
        gameManager.groundTilemap = groundTilemap;
    }
}
