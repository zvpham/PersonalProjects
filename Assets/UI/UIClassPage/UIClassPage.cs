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
    private TMPHolder UIClassLevelPrefab;

    [SerializeField]
    private RectTransform contentPanel;
 
    [SerializeField]
    private TMPHolder commonClass;

    [SerializeField]
    private TMPHolder uncommonClass;

    [SerializeField]
    private TMPHolder rareClass;

    [SerializeField]
    private UIClassDescription classDescription;

    public List<UIClassGroup> commonClasses = new List<UIClassGroup>();
    public List<UIClassGroup> uncommonClasses = new List<UIClassGroup>();
    public List<UIClassGroup> rareClasses = new List<UIClassGroup>();
    public List<BaseGameUIObject> activeUIObjects = new List<BaseGameUIObject>();
    public int currentIndex;
    public int commonIndex = 0;
    public int uncommonIndex = 1;
    public int rareIndex = 2;

    public void AddClass(Class newClass)
    {
        UIClassGroup newClassGroup = null;
        if (newClass.classRarity == ClassRarity.Common)
        {
            newClassGroup = Instantiate(UIClassGroupPrefab);
            newClassGroup.transform.SetParent(commonClass.groupHeaders[0].transform, false);
            commonClass.groupMembers.Add(newClassGroup);

        }
        else if (newClass.classRarity == ClassRarity.Uncommon)
        {
            newClassGroup = Instantiate(UIClassGroupPrefab);
            newClassGroup.transform.SetParent(uncommonClass.groupHeaders[0].transform, false);
            uncommonClass.groupMembers.Add(newClassGroup);

        }
        else if (newClass.classRarity == ClassRarity.Rare)
        {
            newClassGroup = Instantiate(UIClassGroupPrefab);
            newClassGroup.transform.SetParent(rareClass.groupHeaders[0].transform, false);
            rareClass.groupMembers.Add(newClassGroup);
        }
        newClassGroup.transform.SetAsLastSibling();
        newClassGroup.SetClass(newClass.className.ToString());

        for(int i = 0; i < newClass.classLevels.Count; i++)
        {
            TMPHolder newClassLevel = Instantiate(UIClassLevelPrefab);
            newClassLevel.transform.SetParent(newClassGroup.lockedAbilities.groupHeaders[0].transform, false);
            newClassGroup.groupMembers.Add(newClassLevel);

            UIClassStats newStats = Instantiate(UIClassStatsPrefab);
            newStats.ChangeStrength(newClass.classLevels[i].Strength);
            newStats.ChangeAgility(newClass.classLevels[i].Agility);
            newStats.ChangeEndurance(newClass.classLevels[i].Endurance);
            newStats.ChangeIntelligence(newClass.classLevels[i].Intelligence);
            newStats.ChangeWisdom(newClass.classLevels[i].Wisdom);
            newStats.ChangeCharisma(newClass.classLevels[i].Charisma);
            newStats.ChangeLuck(newClass.classLevels[i].Luck);
            if (!newStats.stats.Equals(""))
            {
                newStats.transform.SetParent(newClassLevel.groupHeaders[0].transform, false);
                newStats.transform.SetAsLastSibling();
                newClassLevel.groupMembers.Add(newStats);
            }
            for(int j = 0; j < newClass.classLevels[i].ActionList.Count; i++)
            {
                UIClassAction newAction = Instantiate(UIClassActionPrefab);
                newAction.transform.SetParent(newClassLevel.groupHeaders[0].transform, false);
                newAction.SetActionDescription(newClass.classLevels[i].ActionList[j].description);
                newAction.transform.SetAsLastSibling();
                newClassLevel.groupMembers.Add(newAction);
            }
        }
        UpdateBaseUIObjects();
    }

    public void UpdateUIClassPage()
    {

    }

    public void ExpandAndCollapseGroup(BaseGameUIObject group)
    {
        bool IsActiveGroup = false;
        List<bool> activeGroups = group.GroupsActive();

        for(int i = 0; i < activeGroups.Count; i++)
        {
            if (activeGroups[i])
            {
                IsActiveGroup = true;
                break;
            }
        }

        if (IsActiveGroup)
        {
            for(int i = 0; i < group.groupHeaders.Count; i++)
            {
                group.groupHeaders[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for(int i = 0; i < group.groupHeaders.Count; i++)
            {
                group.groupHeaders[i].gameObject.SetActive(true);
            }
        }
        UpdateBaseUIObjects();
    }

    public void UseUI()
    {
        if (activeUIObjects[currentIndex].isHeadOfGroup)
        {
            ExpandAndCollapseGroup(activeUIObjects[currentIndex]);
        }
    }

    public void UpdateBaseUIObjects()
    {
        activeUIObjects = new List<BaseGameUIObject>();
        commonClass.GetBaseUIOBject(activeUIObjects);
        uncommonClass.GetBaseUIOBject(activeUIObjects);
        rareClass.GetBaseUIOBject(activeUIObjects);
    }
    public void HandleSettingClassDescription(string abilityDescription)
    {
        classDescription.ResetDescription();
        classDescription.SetDescription(abilityDescription);
    }
}
