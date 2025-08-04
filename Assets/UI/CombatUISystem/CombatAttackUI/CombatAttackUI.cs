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
    public Image armorChangeBar;
    public TMP_Text armorValue;
    public Image healthBar;
    public Image healthChangeBar;
    public TMP_Text healthValue;
    public TMP_Text mainAttackValue;

    public GameObject content;
    public RectTransform rectTransform;
    public float defaultHeight;
    public TMP_Text modifierTextPrefab;
    List<TMP_Text> modifierTexts = new List<TMP_Text>();

    public Unit unit;
    public bool readyToReset = true;

    public void Start()
    {
        defaultHeight = rectTransform.rect.height;
        Deactivate();
    }

    public void SetData(Unit targetUnit, List<AttackDataUI> attackDatas, Vector3 newPosition)
    {
        //Reset Data
        for(int i = 0; i < modifierTexts.Count; i++)
        {
            Destroy(modifierTexts[i].gameObject);
        }

        //Set Unit HP and ARmor values
        gameObject.SetActive(true);
        enabled = true;
        unit = targetUnit;
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


        // Set Main Attack Data
        string mainAttackString = "";
        for(int i = 0; i < attackDatas.Count; i++)
        {
            if (attackDatas[i].attackDataType == attackDataType.Main)
            {
                mainAttackString += attackDatas[0].data.ToString() + " ";
            }
        }
        mainAttackValue.text = mainAttackString;


        //Set Modifier Data
        modifierTexts = new List<TMP_Text>();
        for(int i = 0; i < attackDatas.Count; i++)
        {
            if (attackDatas[i].attackDataType == attackDataType.Modifier)
            {
                TMP_Text modifierText = Instantiate(modifierTextPrefab, content.transform);
                modifierText.text = attackDatas[i].data.ToString();
                switch (attackDatas[i].attackState)
                {
                    case attackState.Benificial:
                        modifierText.color = Color.green;
                        break;
                    case attackState.Benediction:
                        modifierText.color = Color.red;
                        break;
                    case attackState.Benign:
                        modifierText.color = Color.gray;
                        break;
                }
                modifierTexts.Add(modifierText);
                modifierText.transform.SetSiblingIndex(content.transform.childCount - 2);
            }
            rectTransform.rect.Set(rectTransform.rect.x, rectTransform.rect.y, rectTransform.rect.width, defaultHeight + 
                (modifierTextPrefab.rectTransform.rect.height * attackDatas.Count - 1));
        }
        transform.position = newPosition;
    }

    public void SetAnimationData(Unit targetUnit, int initialArmor, int initialhealth)
    {
        gameObject.SetActive(true);
        enabled = true;
        unit = targetUnit;

        if (targetUnit.maxArmor > 0)
        {
            armorBar.fillAmount = (float) targetUnit.currentArmor / (float) targetUnit.maxArmor;
            armorChangeBar.fillAmount  = (float)initialArmor / (float)targetUnit.maxArmor;
            armorValue.text = targetUnit.currentArmor.ToString() + "/" + targetUnit.maxArmor.ToString();
        }
        else
        {
            armorBar.fillAmount = 0;
            armorChangeBar.fillAmount = 0;
            armorValue.text = "0/0";
        }

        if (targetUnit.maxHealth > 0)
        {
            healthBar.fillAmount = (float) targetUnit.currentHealth / (float) targetUnit.maxHealth;
            healthChangeBar.fillAmount = (float) initialhealth / (float) targetUnit.maxHealth;
            healthValue.text = targetUnit.currentHealth.ToString() + "/" + targetUnit.maxHealth.ToString();
        }
        else
        {
            healthBar.fillAmount = 0;
            healthChangeBar.fillAmount = 0;
            healthValue.text = "0/0";
        }
    }
    public void Deactivate()
    {
        enabled = false;
        gameObject.SetActive(false);

        rectTransform.rect.Set(rectTransform.rect.x, rectTransform.rect.y, rectTransform.rect.width, defaultHeight);

    }
}
