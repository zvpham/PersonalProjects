using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class MovementStatus : Status
{
    public bool hitFlying = true;
    public bool hitGround = true;
    public List<Vector2> forcedMovementPath;
    public ForcedMovement forcedMovement;

    public void MovementStatusPreset(Unit self, float forcedMovementSpeed, List<Vector2> forcedMovementPath, bool isFlying)
    {
        GameObject temp = Instantiate(self.gameManager.resourceManager.forcedMovementPrefab);
        ForcedMovement tempForcedMovement = temp.GetComponent<ForcedMovement>();
        ForcedMovement.OnHitUnit onHitUnit = OnHitUnit;
        ForcedMovement.OnHitWall onHItWall = OnHitWall;
        ForcedMovement.EndForcedMovement endForcedMovement = EndForcedMovement;
        forcedMovement = tempForcedMovement;
        forcedMovement.animationEnd += AnimationEnd;
        tempForcedMovement.CreateForcedMovement(self, this, forcedMovementSpeed, forcedMovementPath, isFlying, hitFlying, hitGround,
            onHitUnit, onHItWall, endForcedMovement);
    }

    public void LoadMovementStatus(ForcedMovement forcedMovement)
    {
        ForcedMovement.OnHitUnit onHitUnit = OnHitUnit;
        ForcedMovement.OnHitWall onHItWall = OnHitWall;
        ForcedMovement.EndForcedMovement endForcedMovement = EndForcedMovement;
        forcedMovement.onHitUnit = onHitUnit;
        forcedMovement.onHitWall = onHItWall;
        forcedMovement.endForcedMovement = endForcedMovement;
        this.forcedMovement = forcedMovement;
    }

    public abstract bool OnHitUnit(Unit self, Unit target);


    public abstract bool OnHitWall(Unit self, Wall wall);


    public abstract void EndForcedMovement(Unit self);


}

