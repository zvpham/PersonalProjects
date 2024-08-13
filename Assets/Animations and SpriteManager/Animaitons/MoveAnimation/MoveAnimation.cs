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
    public Vector2 movementAmount;
    public float moveSpeed;
    public int moveSpeedPartitions = 24;
    public int positionindex;

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

    public  void SetParameters(CombatGameManager gameManager, Vector2 originalPosition, Vector2 newPosition)
    {
        this.gameManager = gameManager;
        this.originalPosition = originalPosition;
        this.newPosition = newPosition;
        gameManager.spriteManager.AddAnimations(this, gameManager.spriteManager.animations.Count);
    }

    public override void EndAnimation()
    {
        gameManager.spriteManager.spriteGrid.GetGridObject(newPosition).sprites[0] = attackingSprite;
        attackingNode.sprites[0] = null;
        base.EndAnimation();
    }
}
