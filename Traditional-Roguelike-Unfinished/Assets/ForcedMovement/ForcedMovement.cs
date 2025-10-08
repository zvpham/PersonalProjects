using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.GraphicsBuffer;

public class ForcedMovement : MonoBehaviour
{
    public float secEmenateSpeed;
    public float currentTime;

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
    public bool hitFlying;
    public bool hitGrounded;
    public bool pauseMovement;
    public bool unitMovementComplete;
    public bool wallMovementComplete;
    public OnHitUnit onHitUnit;
    public OnHitWall onHitWall;
    public EndForcedMovement endForcedMovement;

    public event UnityAction animationEnd;

    // If True then DeactivateForcedMovement
    public delegate bool OnHitUnit(Unit self, Unit target);

    public delegate bool OnHitWall(Unit self, Wall wall);

    // Let This Handle Final Position
    public delegate void EndForcedMovement(Unit self);

    public void CreateForcedMovement(Unit unit, Status status, float forcedMovementSpeed, List<Vector2> forcedMovementPath, bool isFlying, bool hitFlying, bool hitGround, OnHitUnit onHitUnit, OnHitWall onHitWall, EndForcedMovement endForcedMovement)
    {
        this.unit = unit;
        this.status = status;
        this.forcedMovementSpeed = forcedMovementSpeed;
        this.forcedMovementPath = forcedMovementPath;
        this.isFlying = isFlying;
        this.hitFlying = hitFlying;
        this.hitGrounded = hitGround;
        this.onHitUnit = onHitUnit;
        this.onHitWall = onHitWall;
        this.endForcedMovement = endForcedMovement;
        forcedPathIndex = 0;

         unit.gameManager.ChangeUnits(unit.gameObject.transform.position, null);

        Ivalue = 0;
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
        enabled = true;
        if (Ivalue > forcedMovementSpeed)
        {
            excessForcedMovementSpeed -= previousForcedMovementIterrationRate;
        }
        Unit target = unit.gameManager.grid.GetGridObject(forcedMovementPath[forcedPathIndex]);


        //Debug.LogWarning("Target: " + target + ", hitFlying: " + hitFlying + ", hitGrounded: " + hitGrounded + "Position: " + forcedMovementPath[forcedPathIndex]);
        if (target != null && hitFlying && target.flying && onHitUnit(unit, target))
        {
            Deactivate();
            return;
        }
        else if (target != null && hitGrounded && target.flying == false && onHitUnit(unit, target))
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

        unit.UnitMovement(unit.transform.position, forcedMovementPath[forcedPathIndex]);
        if ((Vector2)unit.transform.position == forcedMovementPath[forcedMovementPath.Count - 1])
        {
            Deactivate();
            return;
        }

        forcedPathIndex += 1;
        previousForcedMovementIterrationRate = 1 * unit.timeFlow;
        Ivalue += 1 * unit.timeFlow;
    }

    public void Update()
    {
        if (Ivalue < forcedMovementSpeed + excessForcedMovementSpeed)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= secEmenateSpeed)
            {
                if (Ivalue > forcedMovementSpeed)
                {
                    excessForcedMovementSpeed -= previousForcedMovementIterrationRate;
                }
                Unit target = unit.gameManager.grid.GetGridObject(forcedMovementPath[forcedPathIndex]);

                //Debug.LogWarning("Target: " + target + ", hitFlying: " + hitFlying + ", hitGrounded: " + hitGrounded + "Position: " + forcedMovementPath[forcedPathIndex]);
                if (target != null && hitFlying && target.flying && onHitUnit(unit, target))
                {
                    Deactivate();
                    return;
                }
                else if (target != null && hitGrounded && target.flying == false && onHitUnit(unit, target))
                {
                    Deactivate();
                    return;
                }
                unitMovementComplete = true;

                Wall wall = unit.gameManager.obstacleGrid.GetGridObject(forcedMovementPath[forcedPathIndex]);
                if (wall != null && onHitWall(unit, wall))
                {
                    Deactivate();
                    return;
                }
                wallMovementComplete = true;

                if(unit == null)
                {
                    Deactivate();
                    return;
                }

                unit.UnitMovement(unit.transform.position, forcedMovementPath[forcedPathIndex]);
                if ((Vector2)unit.transform.position == forcedMovementPath[forcedMovementPath.Count - 1])
                {
                    Deactivate();
                    return;
                }

                forcedPathIndex += 1;
                previousForcedMovementIterrationRate = 1 * unit.timeFlow;
                Ivalue += 1 * unit.timeFlow;
                currentTime = 0;
            }
        }
        else
        {
            excessForcedMovementSpeed += Ivalue - forcedMovementSpeed;
            currentPathIndex = forcedPathIndex - 1;
            Ivalue = 0;
            AnimationEnd();
        }
    }

    public void AnimationEnd()
    {
        enabled = false;
        animationEnd?.Invoke();
    }

    public void Deactivate()
    {
        if(unit == null || unit.gameManager.forcedMovements.IndexOf(this) == -1)
        {
            return;
        }

        endForcedMovement(unit);
        int forcedMovementIndex = unit.gameManager.forcedMovements.IndexOf(this);
        unit.gameManager.mainGameManger.forcedMovements.Remove(this);
        unit.gameManager.forcedMovements.Remove(this);
        unit.gameManager.expectedLocationChangeList.RemoveAt(forcedMovementIndex);
        unit.forcedMovement = null;
        status.RemoveEffect(unit);
        Destroy(this);
        Destroy(gameObject);
        AnimationEnd();
    }

    public void Load(ForcedMovementPathData pathData, Unit unit, MovementStatus status)
    {
        List<Vector2> adjustedPath = new List<Vector2>();
        Vector2 gameManagerAdjustment = unit.gameManager.defaultGridPosition;
        for (int j = 0; j < pathData.forcedMovementPath.Count; j++)
        {
            adjustedPath.Add(pathData.forcedMovementPath[j] + gameManagerAdjustment);
        }
        forcedMovementPath = adjustedPath;
        forcedMovementSpeed = pathData.forcedMovementSpeed;
        excessForcedMovementSpeed = pathData.excessForcedMovementSpeed;
        previousForcedMovementIterrationRate = pathData.previousForcedMovementIterrationRate;
        forcedPathIndex = pathData.forcedPathIndex;
        currentPathIndex = pathData.currentPathIndex;
        forcedMovmentPriority = pathData.forcedPathPriority;
        speed = 1f;
        isFlying = pathData.isFlying;

        this.unit = unit;
        unit.forcedMovement = this;
        this.status = status;
        status.LoadMovementStatus(this);

        unit.gameManager.forcedMovements.Add(this);
        unit.gameManager.mainGameManger.forcedMovements.Add(this);
        unit.gameManager.expectedLocationChangeList.Add(0);
    }
}
