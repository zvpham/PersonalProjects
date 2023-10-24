using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MeleeTargeting : MonoBehaviour
{
    public int startingX;
    public int startingY;
    public Vector3 startPosition;
    public Vector3 endPosition;
    public AllDirections allDirections;
    public InputManager inputManager;

    public GameManager gameManager;

    public event Action<Vector3> foundTarget;
    public event Action<Vector3> hitWall;
    public event Action<bool> Canceled;
    public void Start()
    {
        gameManager = GameManager.instance;
        inputManager = InputManager.instance;
    }

    public void setParameters(Vector3 startPosition)
    {
        this.startPosition = startPosition;
    }

    public void Update()
    {
        for (int i = 0; i < allDirections.Directions.Length; i++)
        {
            if (inputManager.GetKeyDownTargeting(allDirections.Directions[i].directionName))
            {
                Debug.Log("Pressed Button");
                endPosition = startPosition + allDirections.Directions[i].GetDirection();
                Unit unit  = gameManager.grid.GetGridObject(endPosition);
                if (unit != null)
                {
                    foundTarget?.Invoke(endPosition);
                }

                Vector3Int gridPosition = gameManager.groundTilemap.WorldToCell(endPosition);
                if (gameManager.collisionTilemap.HasTile(gridPosition))
                {
                    hitWall?.Invoke(gridPosition);
                }

                Canceled?.Invoke(false);
            }
        }
    }
}
