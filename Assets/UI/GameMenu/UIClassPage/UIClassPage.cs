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
    private UIClassLevel UIClassLevelPrefab;

    [SerializeField]
    private BaseGameUIObject commonClass;

    [SerializeField]
    private BaseGameUIObject uncommonClass;

    [SerializeField]
    private BaseGameUIObject rareClass;

    [SerializeField]
    private UIClassDescription classDescription;

    public List<UIClassGroup> allClasses = new List<UIClassGroup>();
    public List<UIClassGroup> commonClasses = new List<UIClassGroup>();
    public List<UIClassGroup> uncommonClasses = new List<UIClassGroup>();
    public List<UIClassGroup> rareClasses = new List<UIClassGroup>();

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
        topIndex = 0;
        bottomIndex = maxUIObjectsVisibleOnScreen;
        UpdateBaseUIObjects();
    }

    public void UpdateOnScreeenUIObjects()
    {
        onScreenUIObjects = new List<BaseGameUIObject>();
        int bottomOfMenu = bottomIndex;
        if (bottomIndex > activeUIObjects.Count)
        {
            bottomOfMenu = activeUIObjects.Count;
        }
        for (int i = topIndex; i < bottomOfMenu; i++)
        {
            onScreenUIObjects.Add(activeUIObjects[i]);
            string originalText = activeUIObjects[i].GetOriginalText();
            if (activeUIObjects[i].groupMembers.Count > 0)
            {
                if (activeUIObjects[i].isGroupActive())
                {
                    originalText += " (-)";
                }
                else
                {
                    originalText += " (+)";
                }
            }
            activeUIObjects[i].SetText("   " + activeUIObjects[i].AddIndents(selectionNames[i - topIndex] + ") " + originalText));
        }
        activeUIObjects[currentIndex].SetText(selectedIcon + activeUIObjects[currentIndex].GetText().Substring(3));
    }
    public override void UpdateBaseUIObjects()
    {
        activeUIObjects = new List<BaseGameUIObject>();
        commonClass.GetActiveBaseUIOBjects(activeUIObjects);
        uncommonClass.GetActiveBaseUIOBjects(activeUIObjects);
        rareClass.GetActiveBaseUIOBjects(activeUIObjects);
        UpdateOnScreeenUIObjects();
    }

    public void AddClass()
    {
        UIClassGroup newClassGroup = null;

        newClassGroup = Instantiate(UIClassGroupPrefab, contentPanel.transform);
        newClassGroup.transform.SetAsLastSibling();
        rareClass.groupMembers.Add(newClassGroup);
        
        allClasses.Add(newClassGroup);
        newClassGroup.UIPage = this;
        //newClassGroup.currentClass = newClass;
        //newClassGroup.SetOriginalText(newClass.className.ToString());
        newClassGroup.setAmountOfIndents(1);
        newClassGroup.SetText(newClassGroup.AddIndents(newClassGroup.GetOriginalText()));

        BaseGameUIObject unlockedAbilites = Instantiate(UIUnlockedAbilitiesPrefab, contentPanel.transform);
        unlockedAbilites.UIPage = this;
        unlockedAbilites.transform.SetSiblingIndex(newClassGroup.transform.GetSiblingIndex() + 1);
        unlockedAbilites.SetOriginalText(unlockedAbilites.GetText());
        unlockedAbilites.setAmountOfIndents(2);
        unlockedAbilites.SetText(unlockedAbilites.AddIndents(unlockedAbilites.GetOriginalText()));
        newClassGroup.groupMembers.Add(unlockedAbilites);
        newClassGroup.unlockedAbilities = unlockedAbilites;

        BaseGameUIObject lockedAbilities = Instantiate(UILockedAbilitiesPrefab, contentPanel.transform);
        lockedAbilities.UIPage = this;
        lockedAbilities.transform.SetSiblingIndex(unlockedAbilites.transform.GetSiblingIndex() + 1);
        lockedAbilities.SetOriginalText(lockedAbilities.GetText());
        lockedAbilities.setAmountOfIndents(2);
        lockedAbilities.SetText(lockedAbilities.AddIndents(lockedAbilities.GetOriginalText()));
        newClassGroup.groupMembers.Add(lockedAbilities);
        newClassGroup.lockedAbilities = lockedAbilities;

        int lockedAbilitiesIndex = lockedAbilities.transform.GetSiblingIndex() + 1;
        /*
        for (int i = 0; i < newClass.classLevels.Count; i++)
        {
            UIClassLevel newClassLevel = Instantiate(UIClassLevelPrefab, contentPanel.transform);
            newClassLevel.currentClass = newClass;
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
            for (int j = 0; j < newClass.classLevels[i].ActionList.Count; j++)
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
        */
        UpdateBaseUIObjects();
    }

    public void HandleChangeDescription(string description)
    {
        classDescription.SetDescription(description);
    }

    public override void UseUI()
    {
        activeUIObjects[currentIndex].UseUi();
        UpdateBaseUIObjects();
    }

    public override void UseHoverUI()
    {
        base.UseHoverUI();
        activeUIObjects[currentIndex].HoverUI();
        UpdateBaseUIObjects();
    }

    public void OpenMenu()
    {
        UpdateBaseUIObjects();
    }

    public override void SelectMenuObject(int itemIndex)
    {
        if (itemIndex >= onScreenUIObjects.Count)
        {
            return;
        }
        classDescription.ResetDescription();
        BaseGameUIObject currentObject = activeUIObjects[currentIndex];
        base.SelectMenuObject(itemIndex);
        if (itemIndex < activeUIObjects.Count)
        {
            currentObject.SetText(currentObject.AddIndents(currentObject.GetOriginalText()));
            currentIndex = activeUIObjects.IndexOf(onScreenUIObjects[itemIndex]);
            currentObject = activeUIObjects[currentIndex];
            currentObject.SetText(selectedIcon + currentObject.GetText());
            currentObject.HoverUI();
        }
        UpdateBaseUIObjects();
    }

    public override void IndexUp()
    {
        if (currentIndex - 1 >= 0)
        {
            classDescription.ResetDescription();
            if (currentIndex == topIndex && topIndex > 0)
            {
                topIndex -= 1;
                bottomIndex -= 1;
                contentPanel.anchoredPosition = new Vector2(0, contentPanel.GetComponent<GridLayoutGroup>().cellSize.y * topIndex);
            }
            currentIndex -= 1;
            activeUIObjects[currentIndex].SetText(activeUIObjects[currentIndex].AddIndents(activeUIObjects[currentIndex].GetOriginalText()));
            if (activeUIObjects[currentIndex].groupMembers.Count == 0)
            {
                activeUIObjects[currentIndex].HoverUI();
            }
        }
        UpdateBaseUIObjects();
    }

    public override void IndexDown()
    {
        if (currentIndex + 1 < activeUIObjects.Count)
        {
            classDescription.ResetDescription();
            activeUIObjects[currentIndex].SetText(activeUIObjects[currentIndex].AddIndents(activeUIObjects[currentIndex].GetOriginalText()));
            currentIndex += 1;
            if (activeUIObjects[currentIndex].groupMembers.Count == 0)
            {
                activeUIObjects[currentIndex].HoverUI();
            }
            if (currentIndex == bottomIndex && bottomIndex < activeUIObjects.Count)
            {
                topIndex += 1;
                bottomIndex += 1;
                contentPanel.anchoredPosition = new Vector2(0, contentPanel.GetComponent<GridLayoutGroup>().cellSize.y * topIndex);
            }
        }
        UpdateBaseUIObjects();
    }

    //Moves ClassLevel from Locked to Unlocked (Only Does one, so call again if you need to level multiple)
    public void UILevelUpClass()
    {
        UIClassGroup changingClassGroup = null;
        for (int i = 0; i < allClasses.Count; i++)
        {
            /*
            if (allClasses[i].currentClass == leveledUpClass)
            {
                changingClassGroup = allClasses[i];
                break;
            }
            */
        }

        if (changingClassGroup != null)
        {
            List<BaseGameUIObject> classLevelMembers = new List<BaseGameUIObject>();
            changingClassGroup.lockedAbilities.groupMembers[0].GetAllBaseUIOBjects(classLevelMembers);  
            int lastUnlockedAbilityIndex;
            if (changingClassGroup.unlockedAbilities.groupMembers.Count == 0)
            {
                lastUnlockedAbilityIndex = changingClassGroup.unlockedAbilities.transform.GetSiblingIndex() + 1;
            }
            else
            {
                lastUnlockedAbilityIndex = changingClassGroup.lockedAbilities.transform.GetSiblingIndex();
            }

            UIClassLevel uiClassLevel = (UIClassLevel) classLevelMembers[0];
            uiClassLevel.unlocked = true;

            for (int i = 0; i < classLevelMembers.Count; i++)
            {
                classLevelMembers[i].transform.SetSiblingIndex(lastUnlockedAbilityIndex);
                lastUnlockedAbilityIndex += 1;
            }
            changingClassGroup.unlockedAbilities.groupMembers.Add(classLevelMembers[0]);
            changingClassGroup.lockedAbilities.groupMembers.RemoveAt(0);
            UpdateBaseUIObjects();
        }
        else
        {
            Debug.LogError("Couldn't find Class to Adjsut");
        }
    }

    public void BeginClassLevelUp()
    {
        /*
        if(gameMenu.player.availableClassLevelUps > 0)
        {
            gameMenu.DisableMenuInputs();
            gameMenu.confirmPopupMenu.ActivateMenu("Level up " + leveledUpClass.className.ToString() + " to level " +  (leveledUpClass.currentLevel + 1) + "?",
            // function to execute if we select 'yes'
            () =>
            {
                leveledUpClass.LevelUp(gameMenu.player, true);
                UILevelUpClass(leveledUpClass);
                gameMenu.EnableMenuInputs();
            },
            // function to execute if we select 'Cancel'
            () =>
            {
                gameMenu.EnableMenuInputs();
            });
        }
        */
    }

    public override void ResetPage()
    {
        commonClass.FullClearGroupMemebers();
        uncommonClass.FullClearGroupMemebers();
        rareClass.FullClearGroupMemebers(false);
        allClasses.Clear();
        commonClasses.Clear();
        uncommonClasses.Clear();
        rareClasses.Clear();
        classDescription.ResetDescription();
        currentIndex = 0;
        topIndex = 0;
        bottomIndex = maxUIObjectsVisibleOnScreen;
        contentPanel.anchoredPosition = new Vector2(0, 0);
        UpdateBaseUIObjects();
    }
}
