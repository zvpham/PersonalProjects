using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackAnimation : Animation
{
    public Unit attackingUnit;
    public Vector2 originalPosition;
    public Vector2 newPosition;
    public Vector2 movementAmount;
    public float moveSpeed;
    public int moveSpeedPartitions = 24;
    public int positionindex;

    // Start is called before the first frame update
    void Start()
    {
        moveSpeed =  totalTime / moveSpeedPartitions;
        movementAmount = (newPosition - originalPosition) / moveSpeedPartitions;
        positionindex = 0;

    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= moveSpeed)
        {
            attackingUnit.transform.position += (Vector3) movementAmount;
            positionindex += 1;
            if(positionindex >= moveSpeedPartitions)
            {
                EndAnimation();
            }
        }
    }

    public override void EndAnimation()
    {
        attackingUnit.transform.position = originalPosition;
        base.EndAnimation();
    }
}
