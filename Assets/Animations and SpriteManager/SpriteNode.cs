using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteNode
{
    public GridHex<SpriteNode> grid;
    public int x;
    public int y;
    public GameManager gameManager;
    // [ Unit]
    public SpriteRenderer[] sprites = new SpriteRenderer[1];
    public SpriteNode(GridHex<SpriteNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }
}
