using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Text;
using UnityEngine;
using static UnityEditor.Progress;

public class CharacterSystem : MonoBehaviour
{
    [SerializeField]
    public UICharacterMenu characterSelectionUI;

    [SerializeField]
    public CharacterMenuSO companyData;

    public List<Unit> initialHeroes = new List<Unit>();
    public List<UnitGroup> initialMercs = new List<UnitGroup>();

    public bool manageHeroes = true;

    [SerializeField]
    public AudioClip dropClip;

    [SerializeField]
    public AudioSource audioSource;

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

        characterSelectionUI.InitializeCharacterMenuUI(companyData.size);
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
            for(int i = 0; i < companyData.mercSize; i++)
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

        companyData.Initialize();
        companyData.OnCharacterMenuUpdated += UpdateCharacterMenu;
        companyData.OnCharacterMenuUpdatedMerc += UpdateCharacterMenuMerc;
    }

    private void UpdateCharacterMenu(List<Unit> units)
    {
        characterSelectionUI.ResetAllItems();

        for(int i = 0; i < units.Count; i++)
        {
            if (units[i] == null)
            {
                continue;
            }
            characterSelectionUI.UpdateData(i, units[i].unitClass.UIUnitProfile);
        }
    }

    private void UpdateCharacterMenuMerc(List<UnitGroup> unitGroups)
    {
        List<Unit> units = new List<Unit>();
        for (int i = 0; i < companyData.mercSize; i++)
        {
            if (companyData.mercenaries[i] != null)
            {
                units.Add(companyData.mercenaries[i].units[0]);
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
        this.characterSelectionUI.InitializeCharacterMenuUI(companyData.size);
    }

    public void LoadInitialUnits()
    {
        for (int i = 0; i < initialHeroes.Count; i++)
        {
            Unit unit = null;
            if (initialHeroes[i] != null)
            {
                unit = Instantiate(initialHeroes[i]);
                unit.transform.parent = this.transform;
                unit.inOverWorld = true;
            }
            companyData.SetHero(i, unit);
        }

        for(int i = 0; i < initialMercs.Count; i++)
        {
            UnitGroup unitGroup = null;
            if (initialMercs[i] != null)
            {
                unitGroup = Instantiate(initialMercs[i]);
                unitGroup.transform.parent = this.transform;
                unitGroup.inOverWorld = true;

                Unit[] unitChildren = unitGroup.gameObject.GetComponentsInChildren<Unit>();
                for (int j = 0; j < unitChildren.Length; j++)
                {
                    unitChildren[j].inOverWorld = true;
                    unitGroup.units.Add(unitChildren[j]);
                }
            }
            companyData.SetMercenary(i, unitGroup);
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
        else if(companyData.GetMercenaryAt(profileIndex) != null)
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
        if(heroButtonPressed)
        {
            manageHeroes = true;
            characterSelectionUI.InitializeCharacterMenuUI(companyData.size);
            UpdateCharacterMenu(companyData.heroes);

            characterSelectionUI.ResetSelection();
            profileIndex = companyData.GetIndexFirstNonEmptyHero();
        }
        else
        {
            manageHeroes = false;
            characterSelectionUI.InitializeCharacterMenuUI(companyData.mercSize);
            List<Unit> unitGroupUnits = new List<Unit>();
            for(int i = 0; i < companyData.mercenaries.Count; i++)
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

        OnChangedUnitCategory?.Invoke(heroButtonPressed);
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

    /*
    public void OnLoadEquipSoul(int soulSlotIndex, SoulItemSO soul)
    {
        player = Player.Instance;
        inventoryUI = UIInventoryPage.Instance;
        inventoryUI.soulSlots[soulSlotIndex].AddSoul(soul, player, true);
        player.UpdatePlayerActions();
    }
    */
}
