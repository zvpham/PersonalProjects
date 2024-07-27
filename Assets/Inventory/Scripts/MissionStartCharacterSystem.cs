using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionStartCharacterSystem : MonoBehaviour
{
    [SerializeField]
    MissionStartSystem missionStartSystem;

    [SerializeField]
    public UICharacterMenu characterSelectionUI;

    [SerializeField]
    public MissionStartCharacterSO companyData;

    [SerializeField]
    private int systemIndex;

    [SerializeField]
    public AudioClip dropClip;

    [SerializeField]
    public AudioSource audioSource;

    //Unit and Profile INdex
    public event Action<Unit, int> OnUnitClicked;
    public Unit currentUnit;

    void Start()
    {
        PrepareUI();
        PrepareCharacterSelectionData();
    }

    /*
    public void OnOpenMenu()
    {
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
    */

    public void PrepareCharacterSelectionData()
    {

        companyData.Initialize();
        companyData.OnMissionStartCharacterMenuUpdated += UpdateCharacterMenuMissionStart;
    }

    private void UpdateCharacterMenuMissionStart(List<UnitSuperClass> units)
    {
        characterSelectionUI.ResetAllItems();

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] == null)
            {
                continue;
            }

            UnitSuperClass unit = units[i];

            Unit unitProfile = null;
            if (unit.GetType() == typeof(UnitGroup))
            {
                UnitGroup unitGroup = (UnitGroup)unit;
                unitProfile = unitGroup.units[0];
            }
            else
            {
                unitProfile = (Unit)unit;
            }

            characterSelectionUI.UpdateData(i, unitProfile.unitClass.UIUnitProfile);
        }
    }

    private void PrepareUI()
    {
        //this.characterSelectionUI.OnSwapItems += HandleSwapItems;
        this.characterSelectionUI.OnStartDragging += HandleDragging;
        this.characterSelectionUI.OnProfileClicked += HandleProfileClicked;
        this.characterSelectionUI.OnItemDropped += HandleOnItemDroppedOn;
        this.characterSelectionUI.InitializeCharacterMenuUI(companyData.size);
    }

    /*
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

        for (int i = 0; i < initialMercs.Count; i++)
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
    */

    public void SetUnit(int index, UnitSuperClass newUnit)
    {
        companyData.SetUnit(index, newUnit);
    }

    private void HandleProfileClicked(int profileIndex)
    {
        UnitSuperClass unit = companyData.GetUnitAt(profileIndex);
       
        Unit unitProfile = null;
        if (unit != null && unit.GetType() == typeof(UnitGroup))
        {
            UnitGroup unitGroup = (UnitGroup)unit;
            unitProfile = unitGroup.units[0];
        }
        else
        {
            unitProfile = (Unit)unit;
        }
        currentUnit = unitProfile;
        OnUnitClicked?.Invoke(currentUnit, profileIndex);
    }

    private void HandleDragging(int profileIndex)
    {
        UnitSuperClass unit = companyData.GetUnitAt(profileIndex);

        Unit unitProfile = null;
        if(unit != null && unit.GetType() == typeof(UnitGroup))
        {
            UnitGroup unitGroup = (UnitGroup)unit;
            unitProfile = unitGroup.units[0];
        }
        else
        {
            unitProfile = (Unit) unit;
        }

        if (unit == null)
        {
            return;
        }
        characterSelectionUI.CreateDraggedItem(unitProfile.unitClass.UIUnitProfile);
    }

    private void HandleSwapItems(int unitIndex1, int unitIndex2)
    {
        companyData.SwapUnit(unitIndex1, unitIndex2);
    }
    public void HandleOnItemDroppedOn(int profileIndex)
    {
        UnitSuperClass unitCurrentlyInProfile = companyData.GetUnitAt(profileIndex);
        UnitSuperClass unitCurrentlyInMouse = missionStartSystem.GetUnitSuperClassInMouse(unitCurrentlyInProfile, systemIndex, profileIndex);
        if(unitCurrentlyInMouse != null)
        {
            companyData.SetUnit(profileIndex, unitCurrentlyInMouse);
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
