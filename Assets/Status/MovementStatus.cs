using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class MovementStatus : Status
{
    public List<Vector2> forcedMovementPath;

    public void MovementStatusPreset(Unit self, float forcedMovementSpeed, List<Vector2> forcedMovementPath, bool startFlying, bool isFlying)
    {
        GameObject temp = Instantiate(self.gameManager.resourceManager.forcedMovementPrefab);
        ForcedMovement tempForcedMovement = temp.GetComponent<ForcedMovement>();
        ForcedMovement.OnHitUnit onHitUnit = OnHitUnit;
        ForcedMovement.OnHitWall onHItWall = OnHitWall;
        ForcedMovement.EndForcedMovement endForcedMovement = EndForcedMovement;
        tempForcedMovement.CreateForcedMovement(self, this, forcedMovementSpeed, forcedMovementPath, startFlying, isFlying,
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
    }

    public abstract bool OnHitUnit(Unit self, Unit target);


    public abstract bool OnHitWall(Unit self, Wall wall);


    public abstract void EndForcedMovement(Unit self);
}

