using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIClassPage : MonoBehaviour
{
    [SerializeField]
    private UIClassGroup UIClassGroupPrefab;

    [SerializeField]
    private UIClassStats UIClassStatsPrefab;

    [SerializeField] 
    private UIClassAction UIClassActionPrefab;

    [SerializeField]
    private RectTransform contentPanel;

    [SerializeField]
    private UIClassDescription classDescription;

    public List<UIClassGroup> commonClasses = new List<UIClassGroup>();
    public List<UIClassGroup> uncommonClasses = new List<UIClassGroup>();
    public List<UIClassGroup> rareClasses = new List<UIClassGroup>();
    public int currentIndex;


    public void AddClass(Class newClass)
    {
        if (newClass.classRarity == ClassRarity.Common)
        {

        }
        else if (newClass.classRarity == ClassRarity.Uncommon)
        {

        }
        else if (newClass.classRarity == ClassRarity.Rare)
        {

        }
    }

    public void HandleSettingClassDescription(string abilityDescription)
    {
        classDescription.ResetDescription();
        classDescription.SetDescription(abilityDescription);
    }
}
