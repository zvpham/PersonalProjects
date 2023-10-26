using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapseObject : MonoBehaviour
{
    public Grid<WaveFunctionCollapseObject> grid;
    public int x;
    public int y;
    public List<WaveFunctionCollapseStates> possibilityStates;
    public WaveFunctionCollapseObject (Grid<WaveFunctionCollapseObject> grid, int x, int y, List<WaveFunctionCollapseStates> possibilityStates)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.possibilityStates = possibilityStates; 
    }
}
