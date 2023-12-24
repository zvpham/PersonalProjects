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
    public override void Start()
    {
        grid = new Grid<Unit>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, defaultGridPosition, (Grid<Unit> g, int x, int y) => null);
        flyingGrid = new Grid<Unit>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, defaultGridPosition, (Grid<Unit> g, int x, int y) => null);
        itemgrid = new Grid<List<Item>>(mainGameManger.mapWidth, mainGameManger.mapHeight, 1f, defaultGridPosition, (Grid<List<Item>> g, int x, int y) => null);

        for(int i = 0; i < units.Count; i++)
        {
            units[i].inPeripheralGameManager = true;
        }
    }

    // Update is called once per frame 
    void Update()
    {
        
    }
}