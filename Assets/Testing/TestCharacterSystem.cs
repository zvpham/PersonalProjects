using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestCharacterSystem : MonoBehaviour
{
    [SerializeField]
    public TestCharacterUI characterSelectionUI;

    [SerializeField]
    public CharacterMenuSO companyData;

    [SerializeField]
    public ResourceManager resourceManager;

    [SerializeField]
    public CombatGameManager combatGameManager;
        
    public bool manageHeroes = true;

    [SerializeField]
    public AudioClip dropClip;

    [SerializeField]
    public AudioSource audioSource;

    public List<Unit> allUnits;
    public List<UnitGroup> allUnitGroups = new List<UnitGroup>();

    public List<int> activeUnitLevelFilters;
    public List<TestingUnitClassifications> unitClassificationsFilters;

    public event Action<Unit> OnUnitClicked;
    public event Action<bool> OnChangedUnitCategory;

    public Unit currentUnit;

    void Start()
    {
        PrepareUI();
        PrepareCharacterSelectionData();
    }

    public void OnOpenMenu()
    {
        manageHeroes = true;
        characterSelectionUI.OnMenuOpened();
        characterSelectionUI.ResetSelection();

        UpdateCharacterMenu(companyData.heroes);

        int heroIndex = companyData.GetIndexFirstNonEmptyHero();
        if (heroIndex != -1)
        {
            characterSelectionUI.GetCharacterProfile(heroIndex).Select();
            HandleProfileClicked(heroIndex);
        }
    }

    public void OnOpenMenuMissionStart(bool OnHeroButtonPressed)
    {
        manageHeroes = OnHeroButtonPressed;
        characterSelectionUI.ClearCharacterProfiles();

        int characterProfile = -1;
        if (OnHeroButtonPressed)
        {
            characterSelectionUI.InitializeCharacterMenuUI(companyData.size);
            UpdateCharacterMenu(companyData.heroes);
            characterProfile = companyData.GetIndexFirstNonEmptyHero();
        }
        else
        {
            characterSelectionUI.InitializeCharacterMenuUI(companyData.mercSize);
            List<Unit> unitGroupUnits = new List<Unit>();
            for (int i = 0; i < companyData.mercSize; i++)
            {
                if (companyData.mercenaries[i] != null)
                {
                    unitGroupUnits.Add(companyData.mercenaries[i].units[0]);
                }
                else
                {
                    unitGroupUnits.Add(null);
                }
            }
            UpdateCharacterMenu(unitGroupUnits);
            characterProfile = companyData.GetIndexFirstNonEmptyMercenary();
        }

        if (characterProfile != -1)
        {
            characterSelectionUI.GetCharacterProfile(characterProfile).Select();
            HandleProfileClicked(characterProfile);
        }
    }

    public void PrepareCharacterSelectionData()
    {
        companyData.OnCharacterMenuUpdated += UpdateCharacterMenu;
        companyData.OnCharacterMenuUpdatedMerc += UpdateCharacterMenuMerc;
    }

    private void UpdateCharacterMenu(List<Unit> units)
    {
        characterSelectionUI.ResetAllItems();

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] == null)
            {
                continue;
            }
            Sprite validSprite;
            if(units[i].unitClass == null)
            {
                validSprite = units[i].unitProfile;
            }
            else
            {
                validSprite = units[i].unitClass.UIUnitProfile;
            }

            characterSelectionUI.UpdateData(i, validSprite);
        }
    }

    private void UpdateCharacterMenuMerc(List<UnitGroup> unitGroups)
    {
        List<Unit> units = new List<Unit>();
        for (int i = 0; i < unitGroups.Count; i++)
        {
            if (unitGroups[i] != null)
            {
                units.Add(unitGroups[i].units[0]);
            }
            else
            {
                units.Add(null);
            }
        }
        UpdateCharacterMenu(units);
    }

    private void PrepareUI()
    {
        this.characterSelectionUI.OnSwapItems += HandleSwapItems;
        this.characterSelectionUI.OnStartDragging += HandleDragging;
        this.characterSelectionUI.OnProfileClicked += HandleProfileClicked;
        this.characterSelectionUI.OnCategoryButtonPressed += HandleCategoryButtonPressed;
        this.characterSelectionUI.OnCharacterLevelPressed += HandleCharacterLevelPressed;
        this.characterSelectionUI.OnCharacterTypePressed += HandleCharacterTypePressed;
        this.characterSelectionUI.OnNewHeroPressed += HandleNewCharacterButtonPressed;
    }

    // To Load Units put the preab into the resource manager
    public void LoadInitialUnits()
    {

        for(int i = 0; i < resourceManager.heroes.Count; i++)
        {
            Unit unit  = Instantiate(resourceManager.heroes[i]);
            unit.transform.SetParent(this.transform);
            unit.gameManager = combatGameManager;
            allUnits.Add(unit);
        }

        for(int i = 0; i < resourceManager.mercenaries.Count; i++)
        {
            UnitGroup unitGroup  = Instantiate(resourceManager.mercenaries[i]);
            unitGroup.transform.SetParent(this.transform);
            unitGroup.gameManager = combatGameManager;

            Unit[] unitChildren = unitGroup.gameObject.GetComponentsInChildren<Unit>();
            for (int j = 0; j < unitChildren.Length; j++)
            {
                unitGroup.units.Add(unitChildren[j]);
                unitChildren[j].gameManager = combatGameManager;
                unitChildren[j].group = unitGroup;
            }
            allUnitGroups.Add(unitGroup);
        }   
    }

    public void SetHero(int index, Unit newUnit)
    {
        companyData.SetHero(index, newUnit);
    }

    public void ProfileSelected(Unit unitSelected)
    {
        OnUnitClicked?.Invoke(unitSelected);
    }

    private void HandleProfileClicked(int profileIndex)
    {
        if (manageHeroes)
        {
            Unit unit = companyData.GetHeroAt(profileIndex);
            currentUnit = unit;
        }
        else
        {
            UnitGroup unit = companyData.GetMercenaryAt(profileIndex);
            if (unit != null)
            {
                currentUnit = unit.units[0];
            }
            else
            {
                currentUnit = null;
            }
        }
        OnUnitClicked?.Invoke(currentUnit);
    }

    private void HandleDragging(int profileIndex)
    {
        Unit unit = null;
        if (manageHeroes)
        {
            unit = companyData.GetHeroAt(profileIndex);
        }
        else if (companyData.GetMercenaryAt(profileIndex) != null)
        {
            unit = companyData.GetMercenaryAt(profileIndex).units[0];
        }

        if (unit == null)
        {
            return;
        }
        characterSelectionUI.CreateDraggedItem(unit.unitClass.UIUnitProfile);
    }

    private void HandleSwapItems(int itemIndex1, int itemIndex2)
    {
        if (manageHeroes)
        {
            companyData.SwapHero(itemIndex1, itemIndex2);
        }
        else
        {
            companyData.SwapMercenary(itemIndex1, itemIndex2);
        }
    }

    private void HandleCategoryButtonPressed(bool heroButtonPressed)
    {
        int profileIndex = -1;
        if (heroButtonPressed)
        {
            manageHeroes = true;
            UpdateCharacterMenu(companyData.heroes);

            characterSelectionUI.ResetSelection();
            profileIndex = companyData.GetIndexFirstNonEmptyHero();
        }
        else
        {
            manageHeroes = false;
            List<Unit> unitGroupUnits = new List<Unit>();
            for (int i = 0; i < companyData.mercenaries.Count; i++)
            {
                if (companyData.mercenaries[i] != null)
                {
                    unitGroupUnits.Add(companyData.mercenaries[i].units[0]);
                }
                else
                {
                    unitGroupUnits.Add(null);
                }
            }
            UpdateCharacterMenu(unitGroupUnits);

            characterSelectionUI.ResetSelection();
            profileIndex = companyData.GetIndexFirstNonEmptyMercenary();
        }

        if (profileIndex != -1)
        {
            characterSelectionUI.GetCharacterProfile(profileIndex).Select();
            HandleProfileClicked(profileIndex);
        }

        UpdateCharacterFilters();
        OnChangedUnitCategory?.Invoke(heroButtonPressed);
    }

    public void HandleCharacterLevelPressed(int characterLevel)
    {
        activeUnitLevelFilters.Clear();
        if(characterLevel == -1)
        {
            for(int i = 0; i <= 7 ; i++)
            {
                activeUnitLevelFilters.Add(i);
            }
        }
        else
        {
            activeUnitLevelFilters.Add(characterLevel);
        }
        UpdateCharacterFilters();
    }

    public void HandleCharacterTypePressed(int characterTypeIndex)
    {
        unitClassificationsFilters.Clear();
        if (characterTypeIndex == -1)
        {
            unitClassificationsFilters.Add(TestingUnitClassifications.Melee);
            unitClassificationsFilters.Add(TestingUnitClassifications.Ranged);
            unitClassificationsFilters.Add(TestingUnitClassifications.Support);
            unitClassificationsFilters.Add(TestingUnitClassifications.Magic);
            unitClassificationsFilters.Add(TestingUnitClassifications.Divine);
        }
        else
        {
            switch (characterTypeIndex)
            {
                case 0:
                    unitClassificationsFilters.Add(TestingUnitClassifications.Melee);
                    break;
                case 1:
                    unitClassificationsFilters.Add(TestingUnitClassifications.Ranged);
                    break;
                case 2:
                    unitClassificationsFilters.Add(TestingUnitClassifications.Support);
                    break;
                case 3:
                    unitClassificationsFilters.Add(TestingUnitClassifications.Magic);
                    break;
                case 4:
                    unitClassificationsFilters.Add(TestingUnitClassifications.Divine);
                    break;
            }
        }

        UpdateCharacterFilters();
    }

    public void UpdateCharacterFilters()
    {
        if (manageHeroes)
        {
            companyData.size = 0;
            companyData.heroes = new List<Unit>();
            List<Unit> displayedUnits = new List<Unit>();
            for (int i = 0; i < allUnits.Count; i++)
            {
                Unit tempUnit = allUnits[i];
                bool matchingClassification = false;
                for (int j = 0; j < tempUnit.testingUnitClassifications.Count; j++)
                {
                    if (unitClassificationsFilters.Contains(tempUnit.testingUnitClassifications[j]))
                    {
                        matchingClassification = true;
                        break;
                    }
                }
                if (matchingClassification && activeUnitLevelFilters.Contains(tempUnit.powerLevel))
                {
                    companyData.IncreaseSize();
                    displayedUnits.Add(tempUnit);
                    companyData.AddHero(tempUnit);
                }
            }
            UpdateCharacterMenu(displayedUnits);
        }
        else
        {
            companyData.mercSize = 0;
            companyData.mercenaries = new List<UnitGroup>();
            List<UnitGroup> displayedUnitGroups = new List<UnitGroup>();
            for (int i = 0; i < allUnitGroups.Count; i++)
            {
                UnitGroup tempUnitGroup = allUnitGroups[i];
                bool matchingClassification = false;
                for (int j = 0; j < tempUnitGroup.testingUnitClassifications.Count; j++)
                {
                    if (unitClassificationsFilters.Contains(tempUnitGroup.testingUnitClassifications[j]))
                    {
                        matchingClassification = true;
                        break;
                    }
                }
                if (matchingClassification && activeUnitLevelFilters.Contains(tempUnitGroup.powerLevel))
                {
                    companyData.IncreaseMercenarySize();
                    companyData.AddMercenary(tempUnitGroup);
                    displayedUnitGroups.Add(tempUnitGroup);
                }
            }
            UpdateCharacterMenuMerc(displayedUnitGroups);
        }
    }

    public void HandleNewCharacterButtonPressed(int ignoreThisParameter)
    {
        Unit newHero = Instantiate(resourceManager.emptyHero);
        newHero.gameManager = combatGameManager;
        allUnits.Add(newHero);
        UpdateCharacterFilters();
    }

    public UnitSuperClass GetUnitSuperClass(int index)
    {
        if (manageHeroes)
        {
            return companyData.GetHeroAt(index);
        }
        else
        {
            return companyData.GetMercenaryAt(index);
        }
    }
    
}
