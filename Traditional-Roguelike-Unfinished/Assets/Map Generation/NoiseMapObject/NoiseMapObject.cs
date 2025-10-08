using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapObject
{
    public Grid<NoiseMapObject> grid;
    public int x;
    public int y;
    public float value;
    public NoiseMapObject(Grid<NoiseMapObject> grid, int x, int y, float value)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.value = value;
    }
}
