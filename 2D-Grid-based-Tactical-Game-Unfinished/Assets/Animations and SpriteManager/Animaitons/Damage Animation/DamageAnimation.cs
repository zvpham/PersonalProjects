using UnityEngine;

public class DamageAnimation : CustomAnimations
{
    CombatAttackUI combatAttackUI;

    public float alphaChangeSpeed;
    public float alphaChangeAmount;
    public int changePartitions = 24;
    public int changeIndex;

    public bool armorDamaged;
    public bool healthDamaged;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        alphaChangeSpeed = totalTime / changePartitions;
        alphaChangeAmount = 1f / changePartitions;
        changeIndex = 0;

    }

    public override void PlayAnimation()
    {
        base.PlayAnimation();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= alphaChangeSpeed)
        {
            changeIndex += 1;
            combatAttackUI.armorChangeBar.color = new Color(1, 1, 1, 1 - (alphaChangeAmount * changeIndex));
            combatAttackUI.healthChangeBar.color = new Color(1, 1, 1, 1 - (alphaChangeAmount * changeIndex));
            if (changeIndex >= changePartitions)
            {
                EndAnimation();
            }
            currentTime = 0;
        }
    }

    public void SetParameters(CombatAttackUI damage, Unit actingUnit, int initialArmor, int initialHealth)
    {
        this.actingUnit = actingUnit;
        damage.readyToReset = false;
        damage.SetAnimationData(actingUnit, initialArmor, initialHealth);
        combatAttackUI = damage;
        actingUnit.gameManager.spriteManager.AddAnimations(this, actingUnit.gameManager.spriteManager.animations.Count - 1);
        combatAttackUI.armorChangeBar.color = new Color(1, 1, 1, 1);
        combatAttackUI.healthChangeBar.color = new Color(1, 1, 1, 1);
    }

    public override void EndAnimation()
    {
        base.EndAnimation();
        combatAttackUI.readyToReset = true;
        this.spriteManager.ResetCombatAttackUI();
    }
}