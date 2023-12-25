using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public abstract class CreatedField : ScriptableObject
{
    public int createdFieldTypeIndex;
    public float createdFieldQuickness = 1;
    public int createdFieldPriority;
    public int currentCreatedFieldDuration;

    public Vector3 originPosition;
    public int fieldRadius;
    public List<Vector2> createdObjectPositions = new List<Vector2>();

    public bool nonStandardDuration = false;
    public bool fromAnimatedField = false;
    public bool createdWithBlastRadius = false;

    // Assigned in Scriptable Object Thus does not need to be saved
    public GameObject createdObjectPrefab;
    public Status[] createdObjectStatuses;
    public FullDamage damageCalculation;

    public Grid<CreatedObject> grid;

    public bool affectFlying;

    public GameManager gameManager;

    abstract public void CreateGridOfObjects(GameManager gameManager, Vector3 originPosition, int fieldRadius, int fieldDuration, bool onLoad = false);

    abstract public void CreateGridOfObjects(GameManager gameManager, Grid<CreatedObject> grid, int fieldDuration, bool onLoad = false);

    public void CreateGridofObjectsUsingGridPreset(GameManager gameManager, Grid<CreatedObject> grid, int fieldDuration, bool onLoad = false)
    {
        this.grid = grid;
        this.gameManager = gameManager;
        if (!onLoad)
        {
            gameManager.createdFields.Add(this);
            gameManager.mainGameManger.createdFields.Add(this);
            currentCreatedFieldDuration = fieldDuration;
            createdFieldPriority = (int)(gameManager.baseTurnTime * createdFieldQuickness) + gameManager.mainGameManger.least;
        }
        else
        {
            for (int i = 0; i < gameManager.createdFields.Count; i++)
            {
                if (gameManager.createdFields[i] == null)
                {
                    gameManager.createdFields[i] = this;
                    return;
                }
            }
            Debug.LogError("Didn't find an open createdFields SLot");
        }
    }

    public void ApplyStatusOnCreation()
    {
        for (int i = 0; i < grid.GetHeight(); i++)
        {
            for (int j = 0; j < grid.GetWidth(); j++)
            {
                grid.GetGridObject(i, j).ApplyObject(1, gameManager);
            }
        }
    }

    public void RemoveStatusOnDeletion()
    {
        for (int i = 0; i < grid.GetHeight(); i++)
        {
            for (int j = 0; j < grid.GetWidth(); j++)
            {
                grid.GetGridObject(i, j).RemoveObject(gameManager, affectFlying);
            }
        }

        int index = gameManager.createdFields.IndexOf(this);
        gameManager.createdFields.RemoveAt(index);

        index =  gameManager.mainGameManger.createdFields.IndexOf(this);
        gameManager.mainGameManger.createdFields.RemoveAt(index);
    }

    abstract public CreatedObject CreateCreatedObject(Grid<CreatedObject> g, int x, int y, List<Vector3> validLocations);

    abstract public void ApplyObject(float applyPercentage, GameManager gameManager, Vector3 Location);
}

