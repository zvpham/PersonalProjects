using Inventory.Model;
using Inventory.UI;
using System;
using System.Collections;
using System.Collections.Generic;
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

    [SerializeField]
    public AudioClip dropClip;

    [SerializeField]
    public AudioSource audioSource;

    public event Action<Unit> OnUnitClicked;
    public Unit currentUnit;

    void Start()
    {
        PrepareUI();
        PrepareCharacterSelectionData();
    }

    public void OnOpenMenu()
    {
        int heroIndex = companyData.GetIndexFirstNonEmptyHero();
        if (heroIndex != -1)
        {
            characterSelectionUI.GetCharacterProfile(heroIndex).Select();
            HandleProfileClicked(heroIndex);
        }
    }

    public void PrepareCharacterSelectionData()
    {

        companyData.Initialize();
        companyData.OnCharacterMenuUpdated += UpdateCharacterMenu;
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

    private void PrepareUI()
    {
        this.characterSelectionUI.OnSwapItems += HandleSwapItems;
        this.characterSelectionUI.OnStartDragging += HandleDragging;
        this.characterSelectionUI.OnProfileClicked += HandleProfileClicked;
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
    }

    public void SetHero(int index, Unit newUnit)
    {
        companyData.SetHero(index, newUnit);
    }

    private void HandleProfileClicked(int heroIndex)
    {
        Unit unit = companyData.GetHeroAt(heroIndex);
        if (unit == null)
        {
            return;
        }
        currentUnit = unit;
        OnUnitClicked?.Invoke(currentUnit);
    }

    private void HandleDragging(int heroIndex)
    {
        Unit unit = companyData.GetHeroAt(heroIndex);
        if (unit == null)
        {
            return;
        }
        characterSelectionUI.CreateDraggedItem(unit.unitClass.UIUnitProfile);
    }

    private void HandleSwapItems(int itemIndex1, int itemIndex2)
    {
        companyData.SwapHero(itemIndex1, itemIndex2);
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
