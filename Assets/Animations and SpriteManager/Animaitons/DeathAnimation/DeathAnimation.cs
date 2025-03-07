using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathAnimation : CustomAnimations
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

    }

    public override void PlayAnimation()
    {
        base.PlayAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        List<List<CustomAnimations>> animations = spriteManager.animations;
        for(int i = 0; i < animations.Count; i++)
        {
            for(int j = 0; j < animations[i].Count; j++)
            {
                if (animations[i][j].actingUnit == actingUnit)
                {
                    Destroy(animations[i][j].gameObject);
                    animations[i].RemoveAt(j);
                    j--;
                }
            }
            if (animations[i].Count == 0)
            {
                animations.RemoveAt(i);
                i--;
            }
        }
        Destroy(actingUnit.unitSpriteRenderer.gameObject);
        EndAnimation();
    }

    public void SetParameters( Unit actingUnit)
    {
        this.actingUnit = actingUnit;
        actingUnit.gameManager.spriteManager.AddAnimations(this, actingUnit.gameManager.spriteManager.animations.Count - 1);

    }

    public override void EndAnimation()
    {
        base.EndAnimation();
        this.spriteManager.ResetCombatAttackUI();
    }
}
