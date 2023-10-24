using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CreatedObject : ScriptableObject
{
    public Grid<CreatedObject> grid;
    public int x;
    public int y;
    public float timeflow = 1;
    public GameObject createdObjectSpritePrefab;
    public GameObject createdObjectSprite;
    abstract public CreatedObject CreateObject(Grid<CreatedObject> grid, int x, int y, List<Vector3> validLocations);
    abstract public void ApplyObject(float applyPercentage, GameManager gameManager);

    abstract public void ApplyObject(float applyPercentage, GameManager gameManager, Vector3 Location);

    abstract public void ApplyObject(Unit unit);

    abstract public void RemoveObject(GameManager gameManager, bool affectFlying);

    abstract public bool CheckStatus(Status status);
}
