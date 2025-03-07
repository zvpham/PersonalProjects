using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackedAnimation : CustomAnimations
{
    public CombatGameManager gameManager;
    public SpriteRenderer targetSprite;
    public SpriteNode targetNode;
    public Vector3 originalPositionAttackingUnit;
    public Vector3 originalpositionTargetUnit;
    public Vector3 targetMovementAmount;
    public float moveSpeed;
    public int moveSpeedPartitions = 24;
    public int positionindex;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        GridHex<GridPosition> map = gameManager.grid;
        map.GetXY(originalPositionAttackingUnit, out int x, out int y);
        Vector3Int orginalCube =  map.OffsetToCube(x, y);

        map.GetXY(originalpositionTargetUnit, out x, out y);
        Vector3Int targetCube = map.OffsetToCube(x, y);
        List<Vector3Int> cubePath =  map.CubeLineDraw(orginalCube, targetCube);

        Vector2Int targetUnitMovementhex = map.CubeToOffset(cubePath[cubePath.Count - 2]);
        Vector3 targetUnitAttackPosition = map.GetWorldPosition(targetUnitMovementhex);




        moveSpeed = totalTime / moveSpeedPartitions;
        targetMovementAmount = (originalpositionTargetUnit - targetUnitAttackPosition) / moveSpeedPartitions;
        targetMovementAmount = targetMovementAmount  / 4;
        positionindex = 0;

    }

    public override void PlayAnimation()
    {
        base.PlayAnimation();
        targetNode = gameManager.spriteManager.spriteGrid.GetGridObject(originalpositionTargetUnit);
        targetSprite = targetNode.sprites[0];
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= moveSpeed)
        {
            targetSprite.transform.position += (Vector3)targetMovementAmount;
            positionindex += 1;
            if (positionindex >= moveSpeedPartitions)
            {
                EndAnimation();
            }
            currentTime = 0;
        }
    }

    public void SetParameters(Unit actingUnit, Vector2 attackingPosition, Vector2 targetPosition)
    {
        this.actingUnit = actingUnit;
        this.gameManager = actingUnit.gameManager;
        this.originalPositionAttackingUnit = attackingPosition;
        this.originalpositionTargetUnit = targetPosition;
        gameManager.spriteManager.AddAnimations(this, gameManager.spriteManager.animations.Count - 1);
    }

    public override void EndAnimation()
    {
        base.EndAnimation();
        targetSprite.transform.position = originalpositionTargetUnit;
    }
}
