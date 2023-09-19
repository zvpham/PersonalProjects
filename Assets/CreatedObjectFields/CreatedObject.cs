using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CreatedObject : MonoBehaviour
{
    public Grid<CreatedObject> grid;
    public int x;
    public int y;
    public CreatedObject createdObject;

    abstract public CreatedObject CreateObject(Grid<CreatedObject> grid, int x, int y, List<Vector3> validLocations);
    abstract public void ApplyObject(float applyPercentage, GameManager gameManager);

    void Update()
    {
        
    }
}
