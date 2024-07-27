using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatAttackUI : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    public TMP_Text unitName;
    public Image armorBar;
    public TMP_Text armorValue;
    public Image healthBar;
    public TMP_Text healthValue;
    public TMP_Text mainAttackValue;

    public void Start()
    {
        Deactivate();
    }

    public void SetData(Unit targetUnit, List<AttackDataUI> attackDatas, Vector3 newPosition)
    {
        gameObject.SetActive(true);
        enabled = true;
        unitName.text = targetUnit.name;
        if (targetUnit.maxArmor > 0)
        {
            armorBar.fillAmount = targetUnit.currentArmor / targetUnit.maxArmor;
            armorValue.text = targetUnit.currentArmor.ToString() + "/" + targetUnit.maxArmor.ToString();
        }
        else
        {
            armorBar.fillAmount = 0;
            armorValue.text = "0/0";
        }

        if (targetUnit.maxHealth > 0)
        {
            healthBar.fillAmount = targetUnit.currentHealth / targetUnit.maxHealth;
            healthValue.text = targetUnit.currentHealth.ToString() + "/" + targetUnit.maxHealth.ToString();
        }
        else
        {
            healthBar.fillAmount = 0;
            healthValue.text = "0/0";
        }

        if(attackDatas.Count == 0)
        {
            Debug.LogError("Called an attackUI without providing any attack data");
        }

        mainAttackValue.text = attackDatas[0].data.ToString();

        for(int i = 1; i < attackDatas.Count; i++)
        {

        }
        transform.position = newPosition;
    }

    public void Deactivate()
    {
        enabled = false;
        gameObject.SetActive(false);
    }
}
