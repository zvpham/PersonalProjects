using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class ForcedMovement : MonoBehaviour
{
    public Unit unit;
    public Status status;
    public int forcedMovmentPriority;
    public float speed;

    public List<Vector2> forcedMovementPath;
    public float forcedMovementSpeed;
    public float excessForcedMovementSpeed;
    public float previousForcedMovementIterrationRate;
    public int forcedPathIndex;
    public int currentPathIndex;
    public float Ivalue;
    public bool isFlying;
    public OnHitUnit onHitUnit;
    public OnHitWall onHitWall;
    public EndForcedMovement endForcedMovement;

    // If True then DeactivateForcedMovement
    public delegate bool OnHitUnit(Unit self, Unit target);

    public delegate bool OnHitWall(Unit self, Wall wall);

    // Let This Handle Final Position
    public delegate void EndForcedMovement(Unit self);

    public void CreateForcedMovement(Unit unit, Status status, float forcedMovementSpeed, List<Vector2> forcedMovementPath, bool startFlying, bool isFlying, OnHitUnit onHitUnit, OnHitWall onHitWall, EndForcedMovement endForcedMovement)
    {
        this.unit = unit;
        this.status = status;
        this.forcedMovementSpeed = forcedMovementSpeed;
        this.forcedMovementPath = forcedMovementPath;
        this.isFlying = isFlying;
        this.onHitUnit = onHitUnit;
        this.onHitWall = onHitWall;
        this.endForcedMovement = endForcedMovement;
        forcedPathIndex = 0;

        if (startFlying)
        {
            unit.gameManager.ChangeUnits(unit.gameObject.transform.position, null, true);
        }
        else
        {
            unit.gameManager.ChangeUnits(unit.gameObject.transform.position, null);
        }
        speed = 1f;
        forcedMovmentPriority = (int) (unit.gameManager.baseTurnTime * speed) + unit.gameManager.mainGameManger.least;
        unit.gameManager.forcedMovements.Add(this);
        unit.gameManager.mainGameManger.forcedMovements.Add(this);
        unit.gameManager.expectedLocationChangeList.Add(0);
        unit.forcedMovement = this;
        Activate();
    }

    public void Activate()
    {
        for (float i = 0; i < forcedMovementSpeed + excessForcedMovementSpeed;)
        {
            if (i > forcedMovementSpeed)
            {
                excessForcedMovementSpeed -= previousForcedMovementIterrationRate;
            }
            Unit target = unit.gameManager.flyingGrid.GetGridObject(forcedMovementPath[forcedPathIndex]);
            if (isFlying && target != null && onHitUnit(unit, target))
            {
                Deactivate();
                return;
            }

            target = unit.gameManager.grid.GetGridObject(forcedMovementPath[forcedPathIndex]);
            if (!isFlying && target != null && onHitUnit(unit, target))
            {
                Deactivate();
                return;
            }
            Wall wall = unit.gameManager.obstacleGrid.GetGridObject(forcedMovementPath[forcedPathIndex]);
            if (wall != null && onHitWall(unit, wall))
            {
                Deactivate();
                return;
            }
            
            unit.UnitMovement(unit.transform.position, forcedMovementPath[forcedPathIndex], isFlying, isFlying);
            if ((Vector2)unit.transform.position == forcedMovementPath[forcedMovementPath.Count - 1])
            {
                Deactivate();
                return;
            }

            forcedPathIndex += 1;
            i += 1 * unit.timeFlow;
            previousForcedMovementIterrationRate = 1 * unit.timeFlow;
            Ivalue = i;
        }
        excessForcedMovementSpeed += Ivalue - forcedMovementSpeed;
        currentPathIndex = forcedPathIndex - 1;
    }

    public void Load(ForcedMovementPathData pathData, Unit unit, MovementStatus status)
    {
        forcedMovementPath = pathData.forcedMovementPath;
        forcedMovementSpeed = pathData.forcedMovementSpeed;
        excessForcedMovementSpeed = pathData.excessForcedMovementSpeed;
        previousForcedMovementIterrationRate = pathData.previousForcedMovementIterrationRate;
        forcedPathIndex = pathData.forcedPathIndex;
        currentPathIndex = pathData.currentPathIndex;
        forcedMovmentPriority = pathData.forcedPathPriority;
        speed = 1f;
        isFlying = pathData.isFlying;

        unit.flyOnLoad = isFlying;
        this.unit = unit;
        unit.forcedMovement = this;
        this.status = status;
        status.LoadMovementStatus(this);

        unit.gameManager.forcedMovements.Add(this);
        unit.gameManager.mainGameManger.forcedMovements.Add(this);
        unit.gameManager.expectedLocationChangeList.Add(0);
    }

    public void Deactivate()
    {
        endForcedMovement(unit);
        int forcedMovementIndex = unit.gameManager.forcedMovements.IndexOf(this);
        unit.gameManager.mainGameManger.forcedMovements.Remove(this);
        unit.gameManager.forcedMovements.Remove(this);
        unit.gameManager.expectedLocationChangeList.RemoveAt(forcedMovementIndex);
        status.RemoveEffect(unit);
        Destroy(this);
        Destroy(gameObject);
    }
}
