using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;



public class PeripheralGameManager : GameManager
{
    public override void Awake()
    {
        grid = new Grid<Unit>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, defaultGridPosition, (Grid<Unit> g, int x, int y) => null);
        flyingGrid = new Grid<Unit>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, defaultGridPosition, (Grid<Unit> g, int x, int y) => null);
        itemgrid = new Grid<List<Item>>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, defaultGridPosition, (Grid<List<Item>> g, int x, int y) => null);
        obstacleGrid = new Grid<Wall>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, defaultGridPosition + new Vector3(-0.5f, -0.5f, 0), (Grid<Wall> g, int x, int y) => null);
        spriteGrid = new Grid<SpriteNode>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, defaultGridPosition + new Vector3(-0.5f, -0.5f, 0), (Grid<SpriteNode> g, int x, int y) => new SpriteNode(g, x, y));
        initalRenderLocations = new List<Vector3>();
        finalRenderLocations = new List<Vector3>();
        tileVisibilityStates = new int[mainGameManger.mapWidth, mainGameManger.mapHeight];
    }

    public override void Start()
    {

    }

    public void StartPeripheral()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].inPeripheralGameManager = true;
        }
    }

    // Update is called once per frame 
    void Update()
    {
        
    }
}