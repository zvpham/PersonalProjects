using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAnimation : CustomAnimations
{
    public CombatGameManager gameManager;
    public SpriteRenderer attackingSprite;
    public SpriteNode attackingNode;
    public Vector2 originalPosition;
    public Vector2 newPosition;
    public Vector2Int newXY;
    public Vector2 movementAmount;
    public float moveSpeed;
    public int moveSpeedPartitions = 24;
    public int positionindex;
    public int newSortingOrder;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        moveSpeed =  totalTime / moveSpeedPartitions;
        movementAmount = (newPosition - originalPosition) / moveSpeedPartitions;
        positionindex = 0;

    }

    public override void PlayAnimation()
    {
        base.PlayAnimation();
        attackingNode = gameManager.spriteManager.spriteGrid.GetGridObject(originalPosition);
        attackingSprite = attackingNode.sprites[0];
        attackingNode.sprites[0] = attackingNode.sprites[2];
        attackingNode.sprites[2] = null;
        newSortingOrder = spriteManager.terrain[newXY.x, newXY.y].sprite.sortingOrder + 3;
        if (attackingSprite.sortingOrder < newSortingOrder)
        {
            attackingSprite.sortingOrder = newSortingOrder;
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= moveSpeed)
        {
            attackingSprite.transform.position += (Vector3) movementAmount;
            positionindex += 1;
            if(positionindex >= moveSpeedPartitions)
            {
                EndAnimation();
            }
            currentTime = 0;
        }
    }

    public  void SetParameters(Unit actingUnit,  Vector2 originalPosition, Vector2 newPosition, Vector2Int newxy)
    {
        this.actingUnit = actingUnit;
        this.gameManager = actingUnit.gameManager;
        this.originalPosition = originalPosition;
        this.newPosition = newPosition;
        this.newXY = newxy;
        gameManager.spriteManager.AddAnimations(this, gameManager.spriteManager.animations.Count);
    }

    public override void EndAnimation()
    {
        SpriteNode currentSpriteNode = gameManager.spriteManager.spriteGrid.GetGridObject(newPosition);
        currentSpriteNode.sprites[2] = currentSpriteNode.sprites[0];
        currentSpriteNode.sprites[0] = attackingSprite;
        attackingSprite.sortingOrder = newSortingOrder;
        //attackingNode.sprites[0] = null;
        base.EndAnimation();
    }
}
