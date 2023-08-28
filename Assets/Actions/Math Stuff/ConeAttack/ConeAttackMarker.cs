using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeAttackMarker : MonoBehaviour
{
    public Grid<ConeAttackMarker> grid;
    public int x;
    public int y;
    //public GameObject coneMarker;
    public ConeAttackMarker(Grid<ConeAttackMarker> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;

    }
}
