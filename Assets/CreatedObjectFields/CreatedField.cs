using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public abstract class CreatedField : ScriptableObject
{
    public float createdFieldQuickness = 1;

    public int fieldDuration;
    public bool nonStandardDuration = false;

    public GameObject createdObjectPrefab;
    public Status[] createdObjectStatuses;

    public Grid<CreatedObjectStatus> grid;

    public bool affectFlying;

    public GameManager gameManager;

    abstract public void CreateGridOfObjects(GameManager gameManager, int fieldRadius, Vector3 originPosition);

    public void ApplyStatusOnCreation()
    {
        for (int i = 0; i < grid.GetHeight(); i++)
        {
            for (int j = 0; j < grid.GetWidth(); j++)
            {
                if (grid.GetGridObject(i, j).statuses != null)
                {
                    Vector3 location = grid.GetWorldPosition(i, j);
                    Unit ground = gameManager.grid.GetGridObject(location);
                    
                    if (ground != null)
                    {
                        foreach (Status status in grid.GetGridObject(i, j).statuses)
                        {
                            status.ApplyEffect(ground);
                        }

                    }

                    if (affectFlying)
                    {
                        Unit flying = gameManager.flyingGrid.GetGridObject(location);
                        if (flying != null)
                        {
                            foreach (Status status in grid.GetGridObject(i, j).statuses)
                            {
                                status.ApplyEffect(flying);
                            }
                        }
                    }
                }
            }
        }
    }

    public void RemoveStatusOnDeletion()
    {
        for (int i = 0; i < grid.GetHeight(); i++)
        {
            for (int j = 0; j < grid.GetWidth(); j++)
            {
                if (grid.GetGridObject(i, j).statuses != null)
                {
                    Vector3 location = grid.GetWorldPosition(i, j);
                    Unit ground = gameManager.grid.GetGridObject(location);

                    if (ground != null)
                    {
                        foreach (Status status in grid.GetGridObject(i, j).statuses)
                        {
                            if (ground.statuses.Contains(status))
                            {
                                status.RemoveEffect(ground);
                            }
                           
                        }

                    }

                    if (affectFlying)
                    {
                        Unit flying = gameManager.flyingGrid.GetGridObject(location);
                        if (flying != null)
                        {
                            foreach (Status status in grid.GetGridObject(i, j).statuses)
                            {
                                if (ground.statuses.Contains(status))
                                {
                                    status.RemoveEffect(flying);
                                }
                            }
                        }
                    }
                }
                Destroy(grid.GetGridObject(i, j).spriteObject);
            }
        }

        int index = gameManager.StatusFields.IndexOf(this);
        gameManager.StatusFields.Remove(this);
        gameManager.StatusObjectDuration.RemoveAt(index);
        gameManager.statusObjectPriority.RemoveAt   (index);
    }
}

