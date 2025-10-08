using Inventory.Model;
using Inventory.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionStartSystem : MonoBehaviour
{
    [SerializeField]
    public OverWorldMenu overWorldMenu;

    [SerializeField]
    public MenuInputManager menuInputManager;

    [SerializeField]
    public UIMissionStartMenu missionStartMenu;

    [SerializeField]
    public CharacterSystem characterSystem;

    [SerializeField]
    public CharacterSystem missionStartCharacterSystem;

    [SerializeField] 
    public MissionStartCharacterSystem frontLineSystem;

    [SerializeField]
    public MissionStartCharacterSystem backLineSystem;

    // 1 - missionStartCharacterSystem
    // 2 - frontLineSystem
    // 2 - backLineSystem
    public int currntlyBeingSelectedSystem = -1;
    public void Start()
    {
        InitializeUI();
    }

    public void InitializeUI()
    {
        missionStartCharacterSystem.OnChangedUnitCategory += HandleOnChangeUnitCategory;
        missionStartCharacterSystem.OnUnitClicked += HandleMissionStartCharacterMenuUnitClicked;
        frontLineSystem.OnUnitClicked += HandleFrontLineProfileClicked;
        backLineSystem.OnUnitClicked += HandleBackLineProfileClicked;

    }

    public void OpenMenu()
    {

        missionStartMenu.gameObject.SetActive(true);
        overWorldMenu.ChangeMenu += CloseMenu;
        menuInputManager.OnMouseUp += HandleMouseUp;

        ClearCharacters();
        OnOpenMenuLoadHeroes();
        OnOpenMenuLoadMercenaries();

        OnOpenMenuFrontLine();
        OnOpenMenuBackLine();

        missionStartCharacterSystem.OnOpenMenuMissionStart(true);
        missionStartCharacterSystem.characterSelectionUI.OnMenuOpened();
    }

    public void CloseMenu()
    {
        missionStartMenu.gameObject.SetActive(false);
        overWorldMenu.ChangeMenu -= CloseMenu;
        menuInputManager.OnMouseUp -= HandleMouseUp;
    }

    public void OnOpenMenuFrontLine()
    {
        int tempSize = frontLineSystem.companyData.size;
        for(int i = 0; i < frontLineSystem.companyData.units.Count; i++)
        {
            frontLineSystem.companyData.RemoveUnit(i);
        }
        frontLineSystem.characterSelectionUI.ClearCharacterProfiles();
        for(int i = 0; i < tempSize; i++)
        {
            frontLineSystem.characterSelectionUI.AddProfile();
        }
    }

    public void OnOpenMenuBackLine()
    {
        int tempSize = backLineSystem.companyData.size;
        for (int i = 0; i < backLineSystem.companyData.units.Count; i++)
        {
            backLineSystem.companyData.RemoveUnit(i);
        }
        backLineSystem.characterSelectionUI.ClearCharacterProfiles();
        for (int i = 0; i < tempSize; i++)
        {
            backLineSystem.characterSelectionUI.AddProfile();
        }
    }

    public void OnOpenMenuLoadHeroes()
    {
        missionStartCharacterSystem.companyData.size = 0;
        missionStartCharacterSystem.companyData.heroes = new List<Unit>();
        CharacterMenuSO companyData = characterSystem.companyData;
        for(int i = 0; i < companyData.heroes.Count; i++)
        {
            if (companyData.heroes[i] == null) continue;

            missionStartCharacterSystem.companyData.IncreaseSize();
            missionStartMenu.AddProfile(companyData.heroes[i]);
            missionStartCharacterSystem.companyData.AddHero(companyData.heroes[i]);
        }
    }

    public void OnOpenMenuLoadMercenaries()
    {
        missionStartCharacterSystem.companyData.mercSize = 0;
        missionStartCharacterSystem.companyData.mercenaries = new List<UnitGroup>();
        CharacterMenuSO companyData = characterSystem.companyData;
        for (int i = 0; i < companyData.mercenaries.Count; i++)
        {
            if (companyData.mercenaries[i] == null) continue;

            missionStartCharacterSystem.companyData.IncreaseMercenarySize();
            missionStartMenu.AddProfile(companyData.mercenaries[i].units[0]);
            missionStartCharacterSystem.companyData.AddMercenary(companyData.mercenaries[i]);
        }
    }

    public void LoadHeroes()
    {
        CharacterMenuSO companyData = missionStartCharacterSystem.companyData;
        for (int i = 0; i < companyData.heroes.Count; i++)
        {
            if (companyData.heroes[i] == null)
            {
                companyData.heroes.RemoveAt(i);
                i--;
                continue;
            }
            missionStartMenu.AddProfile(companyData .heroes[i]);
        }
        companyData.size = companyData.heroes.Count;
    }

    public void LoadMercenaries()
    {
        CharacterMenuSO companyData = missionStartCharacterSystem.companyData;
        for (int i = 0; i < companyData.mercenaries.Count; i++)
        {
            if (companyData.mercenaries[i] == null)
            {
                companyData.mercenaries.RemoveAt(i);
                i--;
                continue;
            }
            missionStartMenu.AddProfile(companyData.mercenaries[i].units[0]);
        }
        companyData.mercSize = companyData.mercenaries.Count;
    }

    public void AddUnitToInventory(UnitSuperClass unitSuperClass)
    {
        if(unitSuperClass == null)
        {
            return;
        }

        if (unitSuperClass.GetType() == typeof(UnitGroup))
        {
            missionStartCharacterSystem.companyData.mercenaries.Add((UnitGroup)unitSuperClass);
            if(!missionStartCharacterSystem.manageHeroes)
            {
                ClearCharacters();
                LoadMercenaries();
            }
            else
            {
                missionStartCharacterSystem.companyData.IncreaseMercenarySize();
            }
        }
        else
        {
            missionStartCharacterSystem.companyData.heroes.Add((Unit) unitSuperClass);
            if (missionStartCharacterSystem.manageHeroes)
            {
                ClearCharacters();
                LoadHeroes();
            }
            else
            {
                missionStartCharacterSystem.companyData.IncreaseSize();
            }
        }
    }

    public void ClearCharacters()
    {
        missionStartCharacterSystem.characterSelectionUI.ClearCharacterProfiles();
    }

    public void HandleOnChangeUnitCategory(bool heroButtonPresed)
    {
        ClearCharacters();
        if(heroButtonPresed)
        {
            LoadHeroes();
        }
        else
        {
            LoadMercenaries();
        }
        missionStartCharacterSystem.OnOpenMenuMissionStart(heroButtonPresed);
    }

    public void HandleFrontLineProfileClicked(Unit unit, int profileIndex)
    {
        missionStartCharacterSystem.ProfileSelected(unit);
        missionStartCharacterSystem.characterSelectionUI.DeselectAllItems();
        backLineSystem.characterSelectionUI.DeselectAllItems();
        frontLineSystem.characterSelectionUI.SelectProfile(profileIndex);
    }

    public void HandleBackLineProfileClicked(Unit unit, int profileIndex)
    {
        missionStartCharacterSystem.ProfileSelected(unit);
        frontLineSystem.characterSelectionUI.DeselectAllItems();
        missionStartCharacterSystem.characterSelectionUI.DeselectAllItems();
        backLineSystem.characterSelectionUI.SelectProfile(profileIndex);
    }

    public void HandleMissionStartCharacterMenuUnitClicked(Unit unit)
    {
        frontLineSystem.characterSelectionUI.DeselectAllItems();
        backLineSystem.characterSelectionUI.DeselectAllItems();
    }

    public void HandleMouseUp(UICharacterProfile characterProfile)
    {
        if (characterProfile == null)
        {
            int overallCurrentlyDraggedIndex = frontLineSystem.characterSelectionUI.GetCurrentlyDraggedIndex();
            if (overallCurrentlyDraggedIndex != -1)
            {
                UnitSuperClass unitCurrentlyInMouse = frontLineSystem.companyData.GetUnitAt(overallCurrentlyDraggedIndex);
                AddUnitToInventory(unitCurrentlyInMouse);
                frontLineSystem.companyData.RemoveUnit(overallCurrentlyDraggedIndex);
                return;
            }

            overallCurrentlyDraggedIndex = backLineSystem.characterSelectionUI.GetCurrentlyDraggedIndex();
            if (overallCurrentlyDraggedIndex != -1)
            {
                UnitSuperClass unitCurrentlyInMouse = backLineSystem.companyData.GetUnitAt(overallCurrentlyDraggedIndex);
                AddUnitToInventory(unitCurrentlyInMouse);
                backLineSystem.companyData.RemoveUnit(overallCurrentlyDraggedIndex);
                return;
            }
        }
    }

    // Get UnitSuperClass Currently being Draged by Mouse
    // Also Handles what to do with Unit currently in Profile: ie swap or set the unit to the system currenly being used
    public UnitSuperClass GetUnitSuperClassInMouse(UnitSuperClass unitCurrentlyInProfile, int profileSystemIndex, int profileIndex)
    {
        UnitSuperClass unitCurrentlyInMouse = null;
        int overallCurrentlyDraggedIndex = -1;
        // Check Character Inventory
        overallCurrentlyDraggedIndex = missionStartCharacterSystem.characterSelectionUI.GetCurrentlyDraggedIndex();
        if(overallCurrentlyDraggedIndex != -1)
        {
            currntlyBeingSelectedSystem = 1;
            unitCurrentlyInMouse = missionStartCharacterSystem.GetUnitSuperClass(overallCurrentlyDraggedIndex);
            if (missionStartCharacterSystem.manageHeroes)
            {
                missionStartCharacterSystem.companyData.RemoveHero(overallCurrentlyDraggedIndex);
                ClearCharacters();
                LoadHeroes();
            }
            else
            {
                missionStartCharacterSystem.companyData.RemoveMercenary(overallCurrentlyDraggedIndex);
                ClearCharacters();
                LoadMercenaries();
            }

            if (unitCurrentlyInProfile != null)
            {
                AddUnitToInventory(unitCurrentlyInProfile);
            }
            return unitCurrentlyInMouse;
        }

        //Check Frontline Character inventory
        overallCurrentlyDraggedIndex =  frontLineSystem.characterSelectionUI.GetCurrentlyDraggedIndex();
        if (overallCurrentlyDraggedIndex != -1)
        {
            currntlyBeingSelectedSystem = 2;
            unitCurrentlyInMouse = frontLineSystem.companyData.GetUnitAt(overallCurrentlyDraggedIndex);
            if(profileSystemIndex != 2)
            {
                frontLineSystem.companyData.SetUnit(overallCurrentlyDraggedIndex, unitCurrentlyInProfile);
            }
            else
            {
                frontLineSystem.companyData.SwapUnit(overallCurrentlyDraggedIndex, profileIndex);
            }
            return unitCurrentlyInMouse;
        }

        //Check backLine Character inventory
        overallCurrentlyDraggedIndex = backLineSystem.characterSelectionUI.GetCurrentlyDraggedIndex();
        if (overallCurrentlyDraggedIndex != -1)
        {
            currntlyBeingSelectedSystem = 3;
            unitCurrentlyInMouse = backLineSystem.companyData.GetUnitAt(overallCurrentlyDraggedIndex);
            if (profileSystemIndex != 3)
            {
                backLineSystem.companyData.SetUnit(overallCurrentlyDraggedIndex, unitCurrentlyInProfile);
            }
            else
            {
                backLineSystem.companyData.SwapUnit(overallCurrentlyDraggedIndex, profileIndex);
            }
        }
        return unitCurrentlyInMouse;
    }
}
