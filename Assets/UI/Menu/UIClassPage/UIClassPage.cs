using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;

public class UIClassPage : BaseUIPage
{
    [SerializeField]
    private UIClassGroup UIClassGroupPrefab;

    [SerializeField]
    private BaseGameUIObject UILockedAbilitiesPrefab;

    [SerializeField]
    private BaseGameUIObject UIUnlockedAbilitiesPrefab;

    [SerializeField]
    private UIClassStats UIClassStatsPrefab;

    [SerializeField] 
    private UIClassAction UIClassActionPrefab;

    [SerializeField]
    private BaseGameUIObject UIClassLevelPrefab;

    [SerializeField]
    private RectTransform contentPanel;
 
    [SerializeField]
    private BaseGameUIObject commonClass;

    [SerializeField]
    private BaseGameUIObject uncommonClass;

    [SerializeField]
    private BaseGameUIObject rareClass;

    [SerializeField]
    private UIClassDescription classDescription;

    [SerializeField]
    private ScrollRect scrollRect;

    public List<UIClassGroup> commonClasses = new List<UIClassGroup>();
    public List<UIClassGroup> uncommonClasses = new List<UIClassGroup>();
    public List<UIClassGroup> rareClasses = new List<UIClassGroup>();
    public List<BaseGameUIObject> activeUIObjects = new List<BaseGameUIObject>();
    public int maxUIObjectsVisibleOnScreen;
    public int scrollSpeed;
    public int currentIndex;
    public int topIndex;
    public int bottomIndex;

    public override void Start()
    {
        base.Start();
        commonClass.SetOriginalText(commonClass.GetText());
        uncommonClass.SetOriginalText(uncommonClass.GetText());
        rareClass.SetOriginalText(rareClass.GetText());

        classDescription.ResetDescription();
        commonClass.UIPage = this;
        uncommonClass.UIPage = this;
        rareClass.UIPage = this;
        UpdateBaseUIObjects();
        OpenMenu();
        topIndex = 0;
        bottomIndex = maxUIObjectsVisibleOnScreen;
    }

    public void UpdateOnScreeenUIObjects()
    {
        onScreenUIObjects = new List<BaseGameUIObject>();
        commonClass.GetActiveBaseUIOBjects(onScreenUIObjects);
        uncommonClass.GetActiveBaseUIOBjects(onScreenUIObjects);
        rareClass.GetActiveBaseUIOBjects(onScreenUIObjects);
        for (int i = 0; i < onScreenUIObjects.Count; i++)
        {
            string originalText = onScreenUIObjects[i].GetOriginalText();
            if (onScreenUIObjects[i].groupMembers.Count > 0)
            {
                if (onScreenUIObjects[i].isGroupActive())
                {
                    originalText += " (-)";
                }
                else
                {
                    originalText += " (+)";
                }
            }
            onScreenUIObjects[i].SetText("   " + onScreenUIObjects[i].AddIndents(selectionNames[i] + ") " + originalText));
        }
        activeUIObjects[currentIndex].SetText(selectedIcon + activeUIObjects[currentIndex].GetText().Substring(3));
    }
    public void UpdateBaseUIObjects()
    {
        activeUIObjects = new List<BaseGameUIObject>();
        commonClass.GetActiveBaseUIOBjects(activeUIObjects);
        uncommonClass.GetActiveBaseUIOBjects(activeUIObjects);
        rareClass.GetActiveBaseUIOBjects(activeUIObjects);
        UpdateOnScreeenUIObjects();
    }

    public void AddClass(Class newClass)
    {
        UIClassGroup newClassGroup = null;
        if (newClass.classRarity == ClassRarity.Common)
        {
            newClassGroup = Instantiate(UIClassGroupPrefab, contentPanel.transform);
            newClassGroup.transform.SetSiblingIndex(uncommonClass.transform.GetSiblingIndex());
            commonClass.groupMembers.Add(newClassGroup);

        }
        else if (newClass.classRarity == ClassRarity.Uncommon)
        {
            newClassGroup = Instantiate(UIClassGroupPrefab, contentPanel.transform);
            newClassGroup.transform.SetSiblingIndex(rareClass.transform.GetSiblingIndex());
            uncommonClass.groupMembers.Add(newClassGroup);

        }
        else if (newClass.classRarity == ClassRarity.Rare)
        {
            newClassGroup = Instantiate(UIClassGroupPrefab, contentPanel.transform);
            newClassGroup.transform.SetAsLastSibling();
            rareClass.groupMembers.Add(newClassGroup);
        }
        newClassGroup.UIPage = this;
        newClassGroup.SetOriginalText(newClass.className.ToString());
        newClassGroup.setAmountOfIndents(1);
        newClassGroup.SetText(newClassGroup.AddIndents(newClassGroup.GetOriginalText()));

        BaseGameUIObject unlockedAbilites = Instantiate(UIUnlockedAbilitiesPrefab, contentPanel.transform);
        unlockedAbilites.UIPage = this;
        unlockedAbilites.transform.SetSiblingIndex(newClassGroup.transform.GetSiblingIndex() + 1);
        unlockedAbilites.SetOriginalText(unlockedAbilites.GetText());
        unlockedAbilites.setAmountOfIndents(2);
        unlockedAbilites.SetText(unlockedAbilites.AddIndents(unlockedAbilites.GetOriginalText()));
        newClassGroup.groupMembers.Add(unlockedAbilites);

        BaseGameUIObject lockedAbilities = Instantiate(UILockedAbilitiesPrefab, contentPanel.transform);
        lockedAbilities.UIPage = this;
        lockedAbilities.transform.SetSiblingIndex(unlockedAbilites.transform.GetSiblingIndex() + 1);
        lockedAbilities.SetOriginalText(lockedAbilities.GetText());
        lockedAbilities.setAmountOfIndents(2);
        lockedAbilities.SetText(lockedAbilities.AddIndents(lockedAbilities.GetOriginalText()));
        newClassGroup.groupMembers.Add(lockedAbilities);

        int lockedAbilitiesIndex = lockedAbilities.transform.GetSiblingIndex() + 1;
        for (int i = 0; i < newClass.classLevels.Count; i++)
        {
            BaseGameUIObject newClassLevel = Instantiate(UIClassLevelPrefab, contentPanel.transform);
            newClassLevel.UIPage = this;
            newClassLevel.transform.SetSiblingIndex(lockedAbilitiesIndex);
            newClassLevel.SetOriginalText("Level " + (i + 1));
            newClassLevel.setAmountOfIndents(3);
            newClassLevel.SetText(newClassLevel.AddIndents(newClassLevel.GetOriginalText()));
            lockedAbilities.groupMembers.Add(newClassLevel);
            lockedAbilitiesIndex += 1;

            UIClassStats newStats = Instantiate(UIClassStatsPrefab, contentPanel.transform);
            newStats.ChangeStrength(newClass.classLevels[i].Strength);
            newStats.ChangeAgility(newClass.classLevels[i].Agility);
            newStats.ChangeEndurance(newClass.classLevels[i].Endurance);
            newStats.ChangeIntelligence(newClass.classLevels[i].Intelligence);
            newStats.ChangeWisdom(newClass.classLevels[i].Wisdom);
            newStats.ChangeCharisma(newClass.classLevels[i].Charisma);
            newStats.ChangeLuck(newClass.classLevels[i].Luck);
            if (!newStats.statline.Equals(""))
            {
                newStats.UIPage = this;
                newStats.SetOriginalText(newStats.statline);
                newStats.setAmountOfIndents(4);
                newStats.SetText(newStats.AddIndents(newStats.GetOriginalText()));
                newStats.transform.SetSiblingIndex(lockedAbilitiesIndex);
                lockedAbilitiesIndex += 1;
                newClassLevel.groupMembers.Add(newStats);
            }
            else
            {
                Destroy(newStats.gameObject);
            }
            for(int j = 0; j < newClass.classLevels[i].ActionList.Count; j++)
            {
                UIClassAction newAction = Instantiate(UIClassActionPrefab, contentPanel.transform);
                newAction.UIPage = this;
                newAction.SetActionDescription(newClass.classLevels[i].ActionList[j].description);
                newAction.transform.SetSiblingIndex(lockedAbilitiesIndex);
                newAction.SetOriginalText(newAction.GetText());
                newAction.setAmountOfIndents(4);
                newAction.SetText(newAction.AddIndents(newAction.GetOriginalText()));
                lockedAbilitiesIndex += 1;
                newClassLevel.groupMembers.Add(newAction);
            }
        }
        UpdateBaseUIObjects();
    }

    public void HandleChangeDescription(string description)
    {
        classDescription.SetDescription(description);
    }

    public override void UseUI()
    {
        activeUIObjects[currentIndex].UseUI();
        UpdateBaseUIObjects();
    }

    public void OpenMenu()
    {
        UpdateOnScreeenUIObjects();
    }

    public override void SelectMenuObject(int itemIndex)
    {
        classDescription.ResetDescription();
        BaseGameUIObject currentObject = activeUIObjects[currentIndex];
        base.SelectMenuObject(itemIndex);
        if(itemIndex < activeUIObjects.Count)
        {
            currentObject.SetText(currentObject.AddIndents(currentObject.GetOriginalText()));
            currentIndex = itemIndex;
            currentObject = activeUIObjects[currentIndex];
            currentObject.SetText(selectedIcon + currentObject.GetText());
            currentObject.UseUI();
        }
        UpdateOnScreeenUIObjects();
    }
    public override void IndexUp()
    {
        Debug.Log("hello");
        if(currentIndex - 1 >= 0)
        {
            classDescription.ResetDescription();
            if (currentIndex == topIndex && topIndex > 0)
            {
                topIndex -= 1;
                bottomIndex -= 1;
                contentPanel.anchoredPosition = new Vector2(0, 41 * topIndex);
            }
            currentIndex -= 1;
            activeUIObjects[currentIndex].SetText(activeUIObjects[currentIndex].AddIndents(activeUIObjects[currentIndex].GetOriginalText()));
            if (!activeUIObjects[currentIndex].isHeadOfGroup)
            {
                activeUIObjects[currentIndex].UseUI();
            }
        }
        UpdateOnScreeenUIObjects();
    }

    public override void IndexDown()
    {
        if (currentIndex + 1 < activeUIObjects.Count)
        {
            classDescription.ResetDescription();
            activeUIObjects[currentIndex].SetText(activeUIObjects[currentIndex].AddIndents(activeUIObjects[currentIndex].GetOriginalText()));
            currentIndex += 1;
            if (!activeUIObjects[currentIndex].isHeadOfGroup)
            {
                activeUIObjects[currentIndex].UseUI();
            }
            if (currentIndex == bottomIndex && bottomIndex < activeUIObjects.Count)
            {
                topIndex += 1;
                bottomIndex += 1;
                contentPanel.anchoredPosition = new Vector2(0, 41 * topIndex);
            }
        }
        UpdateOnScreeenUIObjects();
    }
}
