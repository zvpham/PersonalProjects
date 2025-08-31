using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatUIProfile : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    public TMP_Text unitName;
    public TMP_Text className;
    public Slider armorBar;
    public TMP_Text armorValue;
    public Slider healthBar;
    public TMP_Text healthValue;

    public void Start()
    {
        Deactivate();
    }

    private void Update()
    {
        //StopScrollLocationChangeOnSwap();
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform
            , Input.mousePosition
            , canvas.worldCamera
            , out position);
        transform.position = canvas.transform.TransformPoint(position);
    }

    public void SetData(Unit unit)
    {
        gameObject.SetActive(true);
        enabled = true;
        unitName.text = unit.name;
        className.text = AllText.languages[AllText.currentDictionary][unit.unitClass.classNameKey];
        if(unit.maxArmor > 0)
        {
            armorBar.value = unit.currentArmor / unit.maxArmor;
            armorValue.text = unit.currentArmor.ToString() + "/" + unit.maxArmor.ToString();
        }
        else
        {
            armorBar.value = 0;
            armorValue.text = "0/0";
        }

        if (unit.maxHealth > 0)
        {
            healthBar.value = unit.currentHealth / unit.maxHealth;
            healthValue.text = unit.currentHealth.ToString() + "/" + unit.maxHealth.ToString();
        }
        else
        {
            healthBar.value = 0;
            healthValue.text = "0/0";
        }
    }

    public void Deactivate()
    {
        enabled = false;
        gameObject.SetActive(false);
    }
}
