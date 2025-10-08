using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteNode
{
    public Grid<SpriteNode> grid;
    public int x;
    public int y;
    // [Darkness, Ground, Unit, Wall, item]
    public SpriteRenderer[] sprites = new SpriteRenderer[5];
    public SpriteNode(Grid<SpriteNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }
}
